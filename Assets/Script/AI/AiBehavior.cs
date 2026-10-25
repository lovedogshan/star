using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class AIBehavior : MonoBehaviour
{
    private Person person;

    [SerializeField]
    private List<AITask> iTasks = new List<AITask>();

    private List<AIAction> failActions = new List<AIAction>();

    //锟斤拷
    // 摘要:
    //     锟阶次得碉拷锟斤拷锟斤拷锟斤拷时锟斤拷锟斤拷
    public Action<AITask> OnGetTaskEvent;

    public List<AITask> AITasks => iTasks;

    //
    // 摘要:
    //     锟斤拷前锟斤拷锟斤拷执锟叫得碉拷锟斤拷锟斤拷
    public AITask CurrentTask { get; private set; }

    public bool HasAssignedWork
    {
        get
        {
            for (int i = 0; i < AITasks.Count; i++)
            {
                AITask task = AITasks[i];
                if (!(task is MyRambleTask) && task.State != Constants.TaskState.succeed &&
                    task.State != Constants.TaskState.abort)
                {
                    return true;
                }
            }

            return false;
        }
    }

    private void Awake()
    {
        person = GetComponent<Person>();
    }

    private void Update()
    {
        TaskWorkFrame();
        PersonFree();
        FailWaitTiemr();
    }

    //
    // 摘要:
    //     锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟?
    private void TaskWorkFrame()
    {
        const int maxStateTransitionsPerFrame = 16;
        int transitions = 0;
        while (AITasks.Count > 0 && transitions++ < maxStateTransitionsPerFrame)
        {
            if (CurrentTask != AITasks[0] && CurrentTask != null)
            {
                if (CurrentTask.State == Constants.TaskState.work)
                {
                    CurrentTask.isPend = true;
                    CurrentTask.IsStop = true;
                }

                AITask aITask = AITasks[0];
                if (aITask.State == Constants.TaskState.work)
                {
                    aITask.isPend = false;
                    aITask.IsStop = false;
                }
            }

            CurrentTask = AITasks[0];
            bool shouldWaitForNextFrame;
            try
            {
                shouldWaitForNextFrame = ExecuteTask(CurrentTask);
            }
            catch (System.Exception exception)
            {
                CurrentTask.State = Constants.TaskState.abort;
                Debug.LogException(exception);
                shouldWaitForNextFrame = false;
            }

            if (shouldWaitForNextFrame)
            {
                return;
            }
        }

        if (AITasks.Count == 0)
        {
            CurrentTask = null;
            person.ExecuteState = Constants.PersonExecuteState.Free;
            TaskController.Instance?.RequestAssignment(person);
        }

;
    }

    //
    // 摘要:
    //     只执锟斤拷一锟轿撅拷锟斤拷锟斤拷锟斤拷锟斤拷呒锟?
    //
    // 锟斤拷锟斤拷:
    //   aiTask:
    //
    // 锟斤拷锟截斤拷锟?
    //     锟角凤拷锟斤拷要锟斤拷锟斤拷锟斤拷帧
    public bool ExecuteTask(AITask aiTask)
    {
        bool result = false;
        switch (aiTask.State)
        {
            case Constants.TaskState.unassign:
                aiTask.State = Constants.TaskState.assign;
                break;
            case Constants.TaskState.assign:
                aiTask.State = Constants.TaskState.start;
                break;
            case Constants.TaskState.start:
                if (aiTask.IsStop)
                {
                    aiTask.IsStop = false;
                }

                aiTask.State = Constants.TaskState.work;
                person.ExecuteState = ((!(aiTask is MyRambleTask)) ? Constants.PersonExecuteState.Working : Constants.PersonExecuteState.Free);
                break;
            case Constants.TaskState.work:
                aiTask.DoingWork();
                result = aiTask.State == Constants.TaskState.work;
                break;
            case Constants.TaskState.stop:
                if (!aiTask.IsStop)
                {
                    aiTask.IsStop = true;
                }

                person.ExecuteState = Constants.PersonExecuteState.Free;
                if (AITasks.Find((AITask task) => task.State != Constants.TaskState.stop) == null)
                {
                    result = true;
                    break;
                }

                AITasks.RemoveAt(0);
                AITasks.Add(aiTask);
                break;
            case Constants.TaskState.succeed:
                AITasks.Remove(aiTask);
                TaskController.Instance.MainTasks.Remove(aiTask);
                JobSystem.Instance?.Unregister(aiTask);
                ReservationManager.Instance?.ReleaseTask(aiTask);
                TaskController.Instance.RequestAssignment(person);
                break;
            case Constants.TaskState.abort:
                if (aiTask.ExecuteIndex < aiTask.AIActions.Count)
                {
                    AIAction aIAction = aiTask.AIActions[aiTask.ExecuteIndex];
                    if (aIAction.IsFailWait)
                    {
                        aiTask.IsStop = true;
                        aiTask.State = Constants.TaskState.stop;
                        aIAction.ActionState = AIActionState.Wait;
                        if (!aIAction.ActionFailWait.isFailWaiting)
                        {
                            aIAction.ActionFailWait.isFailWaiting = true;
                            failActions.Add(aIAction);
                        }

                        break;
                    }
                }

                aiTask.AbortSafe();
                AITasks.Remove(aiTask);
                TaskController.Instance.MainTasks.Remove(aiTask);
                JobSystem.Instance?.Unregister(aiTask);
                ReservationManager.Instance?.ReleaseTask(aiTask);
                TaskController.Instance.RequestAssignment(person);
                break;
        }

        return result;
    }

    //
    // 摘要:
    //     锟届常锟饺达拷锟斤拷时锟斤拷
    private void FailWaitTiemr()
    {
        foreach (AIAction failAction in failActions)
        {
            ActionFailWait actionFailWait = failAction.ActionFailWait;
            actionFailWait.failTimer += Time.deltaTime;
            if (actionFailWait.failTimer > actionFailWait.failTimeMax)
            {
                FailTaskEnd(failAction);
                break;
            }

            try
            {
                SleepTimerData sleepTimerData = actionFailWait.sleepTimerData;
                if (sleepTimerData.sleepTimer < sleepTimerData.SleepTimeMax)
                {
                    sleepTimerData.sleepTimer += Time.deltaTime;
                    continue;
                }

                sleepTimerData.sleepTimer = 0f;
                actionFailWait.onFailWaitInquiry?.Invoke();
                if (failAction.CheckFailRestore())
                {
                    FailTaskEnd(failAction, isNew: true);
                    break;
                }

                sleepTimerData.SleepTimeMax++;
            }
            catch (Exception ex)
            {
                FailTaskEnd(failAction);
                Debug.LogException(ex);
                break;
            }
        }
    }

    //
    // 摘要:
    //     失锟斤拷锟斤拷锟斤拷却锟斤拷锟街?
    //
    // 锟斤拷锟斤拷:
    //   action:
    //
    //   isNew:
    //     锟角凤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟叫讹拷
    private void FailTaskEnd(AIAction action, bool isNew = false)
    {
        if (isNew)
        {
            action.AITask.State = Constants.TaskState.start;
            action.ActionFailWait.isFailWaiting = false;
        }
        else
        {
            action.AITask.State = Constants.TaskState.abort;
            action.IsFailWait = false;
        }

        failActions.Remove(action);
    }

    //
    // 摘要:
    //     锟斤拷锟斤拷色锟斤拷锟斤拷时锟斤拷锟斤拷锟解步
    private void PersonFree()
    {
        bool canWander = !ScheduleSystem.Instance || ScheduleSystem.Instance.AllowsIdleWander(person);
        if (canWander && person.ExecuteState == Constants.PersonExecuteState.Free &&
            AITasks.Find((AITask x) => x is MyRambleTask) == null)
        {
            AITasks.Add(new MyRambleTask(person));
        }
    }

    public void ApplyScheduleActivity(ScheduleActivity activity)
    {
        bool allowsGeneralWork = activity == ScheduleActivity.Anything || activity == ScheduleActivity.Work;
        for (int i = AITasks.Count - 1; i >= 0; i--)
        {
            AITask task = AITasks[i];
            if (task is MyRambleTask)
            {
                if (!ScheduleSystem.Instance || !ScheduleSystem.Instance.AllowsIdleWander(person))
                {
                    task.AbortSafe();
                    AITasks.RemoveAt(i);
                    if (CurrentTask == task)
                    {
                        CurrentTask = null;
                    }
                }

                continue;
            }

            bool taskAllowed = task.IsEmergency || allowsGeneralWork ||
                (activity == ScheduleActivity.Medical && task.WorkKind == Constants.WorkKind.MedicalCare);

            if (!taskAllowed && task.State != Constants.TaskState.succeed && task.State != Constants.TaskState.abort)
            {
                if (task.State == Constants.TaskState.work && !task.IsStop)
                {
                    task.IsStop = true;
                }

                task.State = Constants.TaskState.stop;
            }
            else if (taskAllowed && task.State == Constants.TaskState.stop)
            {
                if (task.IsStop)
                {
                    task.IsStop = false;
                }

                task.State = Constants.TaskState.start;
            }
        }

        if (!allowsGeneralWork && CurrentTask != null && !CurrentTask.IsEmergency)
        {
            person.ExecuteState = Constants.PersonExecuteState.Free;
        }
    }

    public void CancelTask(AITask task)
    {
        if (task == null || !AITasks.Contains(task))
        {
            return;
        }

        task.AbortSafe();
        AITasks.Remove(task);
        TaskController.Instance.MainTasks.Remove(task);
        JobSystem.Instance?.Unregister(task);
        ReservationManager.Instance?.ReleaseTask(task);
        if (CurrentTask == task)
        {
            CurrentTask = null;
        }

        person.ExecuteState = Constants.PersonExecuteState.Free;
        TaskController.Instance.RequestAssignment(person);
    }

    public void OnDisable()
    {
        for (int num = AITasks.Count - 1; num >= 0; num--)
        {
            TaskController.Instance.RemoveAITaskImmediate(AITasks[num]);
        }

        AITasks.Clear();
    }

/*   private void OnEnable()
    {
        if (SoloPlayerSetup.IsSoloMode)
        {
            ((Behaviour)this).enabled = false;
        }
    }*/
}
