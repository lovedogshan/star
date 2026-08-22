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

    //��
    // ժҪ:
    //     �״εõ�������ʱ����
    public Action<AITask> OnGetTaskEvent;

    public List<AITask> AITasks => iTasks;

    //
    // ժҪ:
    //     ��ǰ����ִ�еõ�����
    public AITask CurrentTask { get; private set; }

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
    // ժҪ:
    //     �����������
    private void TaskWorkFrame()
    {
        if (AITasks.Count > 0)
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
            try
            {
                if (!ExecuteTask(CurrentTask))
                {
                    TaskWorkFrame();
                }
            }
            catch (System.Exception exception)
            {
                CurrentTask.State = Constants.TaskState.abort;
                Debug.LogException(exception);

            }
        }
        else
        {
            CurrentTask = null;
            person.ExecuteState = Constants.PersonExecuteState.Free;
        }

;
    }

    //
    // ժҪ:
    //     ִֻ��һ�ξ���������߼�
    //
    // ����:
    //   aiTask:
    //
    // ���ؽ��:
    //     �Ƿ���Ҫ������֡
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
                break;
        }

        return result;
    }

    //
    // ժҪ:
    //     �쳣�ȴ���ʱ��
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
    // ժҪ:
    //     ʧ������ȴ���ֹ
    //
    // ����:
    //   action:
    //
    //   isNew:
    //     �Ƿ��������������������ж�
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
    // ժҪ:
    //     ����ɫ����ʱ�����ⲽ
    private void PersonFree()
    {
        if (person.ExecuteState == Constants.PersonExecuteState.Free && AITasks.Find((AITask x) => x is MyRambleTask) == null)
        {
            AITasks.Add(new MyRambleTask(person));
        }
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
