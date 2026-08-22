using System;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

[Serializable]

public class AITask
{
    public virtual int AIID { get; }
    [SerializeField]
    private List<AIAction> iActions = new List<AIAction>();
    public List<AIAction> AIActions => iActions;

    public int ExecuteIndex { get; set; }

    public Person Person { get; set; }

    public virtual bool IsLongRange { get; set; }

    public virtual WorkKind WorkKind { get; }

    public virtual int Level { get; set; } = 5;


    public TaskState State { get; set; } = TaskState.unassign;
    public AIAction CurrentAction => AIActions[ExecuteIndex];// 是一个 数组或列表的索引访问表达式，用于从 AIActions 列表中获取当前正在执行的行为，ExecuteIndex 是一个 整数变量，表示当前正在执行的行为在 AIActions 列表中的 索引（位置）。
    public Action OnEnd;


    public bool IsStop
    {
        get => isStop;
        set
        {
           if(IsStop != value)
            {
                IsStop = value;
                if(value)
                {
                    Pause();
                }
                else
                {
                    Continue();
                }
            }
        }
    }

    public bool isPend;

    public bool isKill;

    private bool isStop;
    #region 供外界挂载调用事件



    public Action AbortEvent;
    public Action SuccessEvent;
    public Action InitEvent;
    public Action PauseEvent;
    public Action ContinueEvent;



    #endregion

    public virtual bool IsFindPath { get; }




    public virtual Vector2 TargetPoint { get; }


    #region 休眠访问（测试中）
    public float sleepTimer;
    private float sleepTimeMax;
    public float SleepTimeMax
    {
        get => sleepTimeMax;
        set
        {
            if (value> 5)
            {
                value = 5;
            }
            sleepTimeMax = value;
        }
    }
    public const float SleepTimeMaxOffset = 0.1f;
    #endregion

    public void DoingWork()
    {
        if ( ExecuteIndex < AIActions.Count)
        {
            var aiAciton = AIActions[ExecuteIndex];
            switch (aiAciton.ActionState)
            {
                case AIActionState.Wait:
                    aiAciton.ActionState = AIActionState.Doing;
                    break;
                case AIActionState.Doing:
                    aiAciton.Execute();
                    break;
                case AIActionState.Success:
                    aiAciton.OnSuccess();
                    OnEnd?.Invoke();
                    ExecuteIndex++;//继续下个任务
                    break;
                case AIActionState.Fail:
                    aiAciton.OnFail();
                    State = TaskState.abort; // 任务状态设置为中止
                    return;
                default:
                    break;

            }
        }
        else
        {
            State = TaskState.succeed;
        }

    }

    public virtual bool CheckSame(AITask aITask)
    {
        return this == aITask;
    }
    public virtual void Success()
    {
        SuccessEvent?.Invoke();
    }

    public virtual void Abort()
    {
        AbortEvent?.Invoke();

    }

    public virtual void Init()
    {
        InitEvent?.Invoke();
    }
    public virtual void Pause()
    {
        PauseEvent?.Invoke();
        CurrentAction.OnStop();
    }
    public virtual void Continue()
    {
        ContinueEvent?.Invoke();
        CurrentAction.OnRedo();
    }





    public void AbortSafe()
    {
        try
        {
            State = TaskState.abort;
            Abort();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    #region 功能增强




    protected void PadAction(Action action)
    {
        AIActions.Add(new AIAction(this) { ActionState = AIActionState.Success });
        OnEnd = null;
        OnEnd += action;
    }



    public virtual Entity TaskTargetEntity { get; protected set; }
    #endregion
}
