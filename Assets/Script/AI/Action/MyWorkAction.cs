using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyWorkAction : AIAction
{
    public Construction workCon;
    public override bool IsShowProgress { get; protected set; } = true;
    public MyWorkAction(AITask aITask, IConWork conWork) : base(aITask)
    {
        workCon = conWork as Construction;
        WorkSpeedRate = 1 + AITask.Person.Mentality * Person.AbilityRate + AITask.Person.SkillUnit.operateLevel * SkillUnit.OperateRate;

        if (workCon is IConWorker conWorker)
        {
            if (conWorker.IsInfiniteTime)
            {
                IsShowProgress = false;
                WorkTimerMax = 99 * GameValue.PeriodRate;
            }
            else
            {
                IsShowProgress = true;
                WorkTimerMax = conWorker.TimerMax;
            }
        }
        else
        {
            WorkTimerMax = 1;
        }

        OnAnimation();
    }

    public override void Execute()
    {
        base.Execute();
        Work();
    }

    private void Work()
    {
        workCon.ConstructionState = ConstructionState.Work;
        if (WorkTimer > WorkTimerMax)
        {
            if (workCon is IConWorker worker)
            {
                worker.WorkerEnd(AITask.Person);
            }
            workCon.ConstructionState = ConstructionState.Free;
//          AITask.Person.InspirationUnit.GetInspiration(3);

            ActionState = AIActionState.Success;
            return;
        }
    }
/*   public override void OnAnimation()
    {
        base.OnAnimation();
        AITask.Person.SetAction(Constants.PersonAction.Work);
        AITask.Person.WorkLine.end = workCon.transform;
    }*/
}
