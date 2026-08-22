using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class AIAction : IWorkProgress
{
    public virtual string Title { get; }
    public virtual AIActionState ActionState { get; set; }



    public virtual float WorkTimerMax { get; set; }
    public float WorkTimer { get; protected set; }



    public float WorkSpeedRate { get; protected set; } = 1;
    public AITask AITask { get; private set; }

    public virtual float Progress { get => WorkTimer / WorkTimerMax; }

    public virtual bool IsShowProgress { get; protected set; } = false;
    #region ÖÐ¶ÏµÈ´ý



    public virtual bool IsFailWait { get; set; }
    public ActionFailWait ActionFailWait { get; } = new ActionFailWait();
    #endregion

    public AIAction(AITask aITask)
    {
        AITask = aITask;
    }



    public virtual void Execute()
    {
        WorkTimer += Time.deltaTime * WorkSpeedRate;
//       ShipDayLog.WorkerData.SumWorkTime(this);
    }
    public virtual void OnSuccess() { }



    public virtual void OnFail() { }




    public virtual bool CheckFailRestore() { return true; }




    [System.Obsolete]
    public void CheckWorkRange(Vector2 targetPos)
    {
        if (Vector2.Distance(targetPos, AITask.Person.transform.position) > MyGoAction.GetExecuteRange(AITask))
        {
            ActionState = AIActionState.Fail;
        }
    }




    public virtual void OnRedo()
    {
        try
        {
            OnAnimation();
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }



    public virtual void OnStop() { }



    public virtual void OnAnimation() { }
}




public enum AIActionState
{
    Wait,
    Doing,
    Success,
    Fail
}




public interface IWorkProgress
{
    float Progress { get; }



    bool IsShowProgress { get; }
}



public class ActionFailWait
{



    public bool isFailWaiting;



    public float failTimer;



    public float failTimeMax = 48;




    public System.Action onFailWaitInquiry;
    public readonly SleepTimerData sleepTimerData = new SleepTimerData();
}




public class SleepTimerData
{
    #region ÐÝÃß·ÃÎÊ£¨²âÊÔÖÐ£©
    public float sleepTimer;
    private float sleepTimeMax;
    public float SleepTimeMax
    {
        get => sleepTimeMax;
        set
        {
            if (value > 5)
            {
                value = 5;
            }
            sleepTimeMax = value;
        }
    }
    #endregion
}