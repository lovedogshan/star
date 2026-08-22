using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRambleTask : AITask
{
    public override int AIID => 2;
//    public override TimePlanKind PlanKind => TimePlanKind.life;
    public override int Level => 0;
//   public override bool IsWarn => true;
    public MyRambleTask(Person person)
    {
        Person = person;
        MakeTask();
    }

    public void MakeTask()
    {
        AIActions.Add(new MyRambleAction(this, Person));
    }

    public override void Abort() { }
}
