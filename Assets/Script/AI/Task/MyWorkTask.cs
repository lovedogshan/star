using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MyWorkTask : AITask
{
    public override int AIID => 21;
    public override Constants.WorkKind WorkKind { get; } = Constants.WorkKind.Operation;
    public override Entity TaskTargetEntity => ConWork as Entity;
    private IConWork _conWork;
    public IConWork ConWork => _conWork;
    public MyWorkTask(IConWork conWork)
    {
        _conWork = conWork;
        MakeTask();
    }

    private void MakeTask()
    {
        AIActions.Add(new MyGoAction(this, (_conWork as Entity).transform));
        OnEnd = null;
        OnEnd += () =>
        {
            AIActions.Add(new MyWorkAction(this, _conWork));
            OnEnd = null;
        };
    }

    public override bool CheckSame(AITask aITask)
    {
        return base.CheckSame(aITask) || (aITask is MyWorkTask task && task._conWork == _conWork);
    }
/*    public override void Abort()
    {
        base.Abort();
        if (_conWork != null && _conWork is Construction construction)
        {
            construction.ConstructionState = ConstructionState.Free;
        }
    }
    public override void AbortLog()
    {
        if (_conWork != null && _conWork is Construction construction)
        {
            LogPanel.OnLog($"¡Á  {Config.title}", construction.DefaultIcon, ColorUtil.ColorKind.Yellow);
            return;
        }
        base.AbortLog();
    }
    public override void Pause()
    {
        base.Pause();
        if (_conWork != null && _conWork is Construction construction)
        {
            construction.ConstructionState = ConstructionState.Free;
        }
    }

    public override void Continue()
    {
        base.Continue();
        if (_conWork != null && _conWork is Construction construction)
        {
            construction.ConstructionState = ConstructionState.Work;
        }
    }*/
}
