using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRambleAction : AIAction
{
    private Person person;
    private Vector3 rambleTarget;
    private bool hasRambleTarget;
    private float waitingForPathTime;
    private const float MaxRambleWaitTime = 0.75f;
    private const int TargetAttempts = 10;




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

        if (person.Navigation)
        {
            if (!hasRambleTarget)
            {
                person.RunIntervalTimer -= Time.deltaTime;
                if (person.RunIntervalTimer > 0)
                {
                    return;
                }

                person.RunIntervalTimer = Random.Range(0.1f, 0.8f);
                if (!TryChooseRambleTarget())
                {
                    person.RunIntervalTimer = 0.15f;
                    return;
                }
            }

            MovementStatus status = person.Navigation.Navigate(null, rambleTarget, 0f);
            if (status == MovementStatus.Waiting)
            {
                waitingForPathTime += Time.deltaTime;
                if (waitingForPathTime >= MaxRambleWaitTime)
                {
                    ResetRambleTarget(0.05f);
                }
            }
            else
            {
                waitingForPathTime = 0f;
            }

            if (status == MovementStatus.Arrived)
            {
                ResetRambleTarget(Random.Range(0.1f, 0.8f));
            }
            else if (status == MovementStatus.Failed)
            {
                ResetRambleTarget(0.05f);
            }

            if (GameManager.Instance && GameManager.Instance.gameValue != null)
            {
                person.OnNullTimer = GameManager.Instance.gameValue.currentPeriod;
            }
            return;
        }

        if (!person.IsRun)
        {
            person.RunIntervalTimer -= Time.deltaTime;
            if (person.RunIntervalTimer > 0)
            {
                return;
            }
            person.RunIntervalTimer = Random.Range(0f, 1.5f);
            var localPosition = person.transform.localPosition;
            var roundLocation = BaseValueUtil.Round(localPosition);
            var location = new Vector2Int((int)roundLocation.x, (int)roundLocation.y);
            var randomOffset = new Vector2Int(Random.Range(-2, 3), Random.Range(-2, 3));
            var targetPosition = location + randomOffset;

            if (!person.FindPathPlus.FindingPath(location, targetPosition, out person._getPaths, 0))
            {
                person.TryOnNull();
                return;
            }
        }

        MyGoAction.MoveExecute(person);
        if (GameManager.Instance && GameManager.Instance.gameValue != null)
        {
            person.OnNullTimer = GameManager.Instance.gameValue.currentPeriod;
        }
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
        hasRambleTarget = false;
        waitingForPathTime = 0f;
    }

    private bool TryChooseRambleTarget()
    {
        Transform root = person.CurrentRegion ? person.CurrentRegion.CoordinateRoot :
            person.shiper ? person.shiper.transform : person.transform.parent;
        if (!root)
        {
            return false;
        }

        Vector3 localPosition = root.InverseTransformPoint(person.transform.position);
        Vector2Int location = new Vector2Int(
            Mathf.RoundToInt(localPosition.x),
            Mathf.RoundToInt(localPosition.y));

        for (int attempt = 0; attempt < TargetAttempts; attempt++)
        {
            Vector2Int randomOffset = new Vector2Int(Random.Range(-4, 5), Random.Range(-4, 5));
            if (randomOffset == Vector2Int.zero || randomOffset.sqrMagnitude < 4)
            {
                continue;
            }

            Vector2Int targetCell = location + randomOffset;
            Vector3 requestedTarget = root.TransformPoint(new Vector3(targetCell.x, targetCell.y, localPosition.z));
            if (!person.Navigation.TryResolveNavigablePoint(requestedTarget, out Vector3 resolvedTarget, 2) ||
                !IsRamblePointFree(resolvedTarget))
            {
                continue;
            }

            rambleTarget = resolvedTarget;
            hasRambleTarget = true;
            waitingForPathTime = 0f;
            return true;
        }

        return false;
    }

    private bool IsRamblePointFree(Vector3 worldPoint)
    {
        Collider2D ownCollider = person.GetComponent<Collider2D>();
        float radius = ownCollider
            ? Mathf.Max(0.1f, Mathf.Max(ownCollider.bounds.extents.x, ownCollider.bounds.extents.y))
            : 0.2f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPoint, radius, Physics2D.AllLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (!hit || hit == ownCollider || hit.isTrigger || hit.GetComponentInParent<Person>() == person)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private void ResetRambleTarget(float retryDelay)
    {
        person.Navigation.Cancel();
        hasRambleTarget = false;
        waitingForPathTime = 0f;
        person.RunIntervalTimer = Mathf.Max(0f, retryDelay);
    }
}
