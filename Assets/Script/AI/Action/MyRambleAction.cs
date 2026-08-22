using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRambleAction : AIAction
{
    private Person person;




//   private float relaxTimeMax = 24;
//    private BuffItem relaxBuff;

    public MyRambleAction(AITask aITask, Person person) : base(aITask)
    {
        this.person = person;
    }
    public override void OnAnimation()
    {
        base.OnAnimation();
//      person.SetAction(Constants.PersonAction.Idle);
        person.ExecuteState = Constants.PersonExecuteState.Free;
    }

    public override void Execute()
    {
        base.Execute();

        OnAnimation();

        for (int i = 1; i < person.IBehavior.AITasks.Count; i++)
        {
            if (person.IBehavior.AITasks[i].State != Constants.TaskState.stop)
            {
                ActionState = AIActionState.Fail;
                return;
            }
        }

        //       if (WorkTimer > relaxTimeMax && relaxBuff == null)
        //       {
        //            relaxBuff = AITask.Person.Buffer.AddBuff(13);
        //        }

//      Debug.Log(person.IsRun);
        if (!person.IsRun)
        {
            person.RunIntervalTimer -= Time.deltaTime;
            if (person.RunIntervalTimer > 0)
            {
                return;
            }
            person.RunIntervalTimer = Random.Range(0, 1.5f);
            var localPosition = person.transform.localPosition;
            var roundLocation = BaseValueUtil.Round(localPosition);
            var location = new Vector2Int((int)roundLocation.x, (int)roundLocation.y);
            var randomOffset = new Vector2Int(Random.Range(-2, 3), Random.Range(-2, 3));
            var targetPosition = location + randomOffset;

            var isFind = person.FindPathPlus.FindingPath(location, targetPosition, out person._getPaths, 0);
            if (!isFind)
            {
                person.TryOnNull();
                return;
            }
        }
        MyGoAction.MoveExecute(person);
        person.OnNullTimer = GameManager.Instance.gameValue.currentPeriod;
    }

    public override void OnFail()
    {
        base.OnFail();
        TempExitAction();
    }

    public override void OnStop()
    {
        base.OnStop();
        TempExitAction();
    }


  private void TempExitAction()
    {
/*      if (relaxBuff != null)
        {
            relaxBuff.isEnd = true;
        }*/
        person.IsRun = false;
        person._getPaths.Clear();
    }
}
