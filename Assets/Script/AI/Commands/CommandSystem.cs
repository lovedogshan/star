using System;
using System.Collections.Generic;
using UnityEngine;

public enum CommandType
{
    Move,
    Work,
    Prioritize,
    Emergency,
    Cancel,
    Zone
}

public readonly struct CommandResult
{
    public bool Succeeded { get; }
    public string Message { get; }

    private CommandResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public static CommandResult Success(string message = null) => new CommandResult(true, message);
    public static CommandResult Failure(string message) => new CommandResult(false, message);
}

public interface ICommand
{
    string Id { get; }
    CommandType Type { get; }
    string DisplayName { get; }
    Person Issuer { get; }
    bool IsQueued { get; }
    bool IsCancelled { get; }
    bool IsFinished { get; }
    AITask Task { get; }

    bool Validate(out string reason);
    CommandResult Execute();
    CommandResult Cancel();
}

public interface ICommandTaskProvider
{
    bool TryCreateCommandTask(CommandType commandType, Person worker, out AITask task, out string reason);
}

public sealed class TaskCommand : ICommand
{
    private readonly bool assignToIssuer;
    private readonly bool emergency;

    public string Id { get; }
    public CommandType Type { get; }
    public string DisplayName { get; }
    public Person Issuer { get; }
    public bool IsQueued { get; }
    public bool IsCancelled { get; private set; }
    public AITask Task { get; }
    public bool IsFinished => IsCancelled || Task == null ||
                              Task.State == Constants.TaskState.succeed ||
                              Task.State == Constants.TaskState.abort;

    public TaskCommand(
        AITask task,
        CommandType type,
        string displayName,
        Person issuer = null,
        bool isQueued = false,
        bool assignToIssuer = false,
        bool emergency = false)
    {
        Task = task;
        Type = type;
        DisplayName = displayName;
        this.assignToIssuer = assignToIssuer;
        this.emergency = emergency;
        Id = Guid.NewGuid().ToString("N");
        Issuer = issuer;
        IsQueued = isQueued;
    }

    public bool Validate(out string reason)
    {
        if (IsCancelled)
        {
            reason = "该指令已经取消。";
            return false;
        }

        if (Task == null)
        {
            reason = "指令没有生成可执行任务。";
            return false;
        }

        if (!TaskController.Instance)
        {
            reason = "任务管理器尚未就绪。";
            return false;
        }

        if (assignToIssuer && (!Issuer || !Issuer.IBehavior))
        {
            reason = "必须先选择一名可以执行 AI 任务的员工。";
            return false;
        }

        reason = null;
        return true;
    }

    public CommandResult Execute()
    {
        if (!Validate(out string reason))
        {
            return CommandResult.Failure(reason);
        }

        Task.IsPlayerIssued = true;
        Task.CommandId = Id;
        Task.CommandLabel = DisplayName;
        Task.IsEmergency |= emergency;
        Task.Level = Mathf.Max(Task.Level, emergency ? 100 : 20);

        bool added = assignToIssuer
            ? TaskController.Instance.AssignPlayerCommand(Task, Issuer, IsQueued)
            : TaskController.Instance.AddTaskToMain(Task);
        return added
            ? CommandResult.Success(IsQueued ? "指令已加入队列。" : "指令已发布。")
            : CommandResult.Failure("目标已被占用、存在重复任务，或员工无法接受该任务。");
    }

    public CommandResult Cancel()
    {
        if (IsCancelled)
        {
            return CommandResult.Success();
        }

        IsCancelled = true;
        if (!TaskController.Instance)
        {
            return CommandResult.Failure("任务管理器尚未就绪。");
        }

        return TaskController.Instance.CancelTask(Task)
            ? CommandResult.Success("指令已取消。")
            : CommandResult.Failure("该指令已经结束，无法取消。");
    }

    public bool IsNear(Vector2 worldPosition, float radius)
    {
        return Task != null && Task.TryGetTargetPosition(out Vector3 target) &&
               Vector2.Distance(target, worldPosition) <= radius;
    }
}

public sealed class CommandSystem : MonoBehaviour
{
    private readonly List<ICommand> activeCommands = new List<ICommand>();

    public static CommandSystem Instance { get; private set; }
    public IReadOnlyList<ICommand> ActiveCommands => activeCommands;

    public event Action<ICommand> OnCommandIssued;
    public event Action<ICommand> OnCommandCancelled;
    public event Action<ICommand> OnCommandFinished;
    public event Action<ICommand, string> OnCommandFailed;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        for (int i = activeCommands.Count - 1; i >= 0; i--)
        {
            ICommand command = activeCommands[i];
            if (!command.IsFinished)
            {
                continue;
            }

            activeCommands.RemoveAt(i);
            OnCommandFinished?.Invoke(command);
        }
    }

    public CommandResult Issue(ICommand command)
    {
        if (command == null)
        {
            return CommandResult.Failure("指令为空。");
        }

        CommandResult result = command.Execute();
        if (result.Succeeded)
        {
            activeCommands.Add(command);
            OnCommandIssued?.Invoke(command);
        }
        else
        {
            OnCommandFailed?.Invoke(command, result.Message);
        }

        return result;
    }

    public CommandResult Cancel(ICommand command)
    {
        if (command == null)
        {
            return CommandResult.Failure("指令为空。");
        }

        CommandResult result = command.Cancel();
        if (result.Succeeded)
        {
            activeCommands.Remove(command);
            OnCommandCancelled?.Invoke(command);
        }
        else
        {
            OnCommandFailed?.Invoke(command, result.Message);
        }

        return result;
    }

    public CommandResult IssueTask(
        AITask task,
        CommandType type,
        string displayName,
        Person issuer = null,
        bool isQueued = false,
        bool assignToIssuer = false,
        bool emergency = false)
    {
        return Issue(new TaskCommand(task, type, displayName, issuer, isQueued, assignToIssuer, emergency));
    }

    public CommandResult CancelAt(Vector2 worldPosition, float radius = 1f)
    {
        for (int i = activeCommands.Count - 1; i >= 0; i--)
        {
            if (activeCommands[i] is TaskCommand taskCommand && taskCommand.IsNear(worldPosition, radius))
            {
                return Cancel(taskCommand);
            }
        }

        return CommandResult.Failure("目标位置没有可取消的玩家指令。");
    }

    public CommandResult PrioritizeAt(Vector2 worldPosition, bool emergency, float radius = 1f)
    {
        for (int i = activeCommands.Count - 1; i >= 0; i--)
        {
            ICommand command = activeCommands[i];
            if (command is TaskCommand taskCommand && taskCommand.IsNear(worldPosition, radius) &&
                TaskController.Instance.PrioritizeTask(command.Task, emergency))
            {
                return CommandResult.Success(emergency ? "任务已设为紧急。" : "任务优先级已提高。");
            }
        }

        return CommandResult.Failure("目标位置没有可提高优先级的任务。");
    }
}
