using UnityEngine;

public class DirectMovementDriver : IMovementDriver
{
    public MovementStatus Tick(Person person, NavigationSegment segment)
    {
        if (Vector2.Distance(person.transform.position, segment.WorldTarget) <= segment.StoppingDistance)
        {
            person.IsRun = false;
            return MovementStatus.Arrived;
        }

        float speed = Mathf.Max(0.01f, person.MoveSpeed + person.MoveSpeedSlow);
        Vector2 requestedDelta = Vector2.ClampMagnitude((Vector2)segment.WorldTarget - (Vector2)person.transform.position, speed * Time.deltaTime);
        person.TryMoveSafely(requestedDelta, out Vector2 actualDelta);
        if (actualDelta.sqrMagnitude <= 0.000001f)
        {
            person.IsRun = false;
            return MovementStatus.Waiting;
        }

        person.IsRun = true;
        return MovementStatus.Running;
    }

    public void Cancel(Person person)
    {
        if (person)
        {
            person.IsRun = false;
        }
    }
}
