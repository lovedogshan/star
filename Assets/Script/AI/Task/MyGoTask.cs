using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyGoTask : AITask
{
    public override int AIID => 11;
    public override Constants.WorkKind WorkKind => Constants.WorkKind.None;
    public override int Level => 10;
    private Vector2 point;
    public override Vector2 TargetPoint => point;

    public MyGoTask(Vector2 point)
    {
        this.point = point;
        MakeTask();
    }

    private void MakeTask()
    {
        AIActions.Add(new MyGoAction(this, null, point));
        OnEnd = null;
    }
}
