using UnityEngine;

public class EvaMovementDriver : IMovementDriver
{
    private const float ObstacleCheckInterval = 0.25f;
    private float nextObstacleCheck;
    private Vector2 avoidanceDirection;

    public MovementStatus Tick(Person person, NavigationSegment segment)
    {
        EvaCapability eva = person.EvaCapability;
        if (!eva || !eva.CanEva)
        {
            return MovementStatus.Failed;
        }

        Vector2 position = person.transform.position;
        Vector2 target = segment.WorldTarget;
        float remaining = Vector2.Distance(position, target);
        if (remaining <= segment.StoppingDistance)
        {
            person.IsRun = false;
            return MovementStatus.Arrived;
        }

        Vector2 desired = (target - position).normalized;
        if (Time.time >= nextObstacleCheck)
        {
            nextObstacleCheck = Time.time + ObstacleCheckInterval;
            RaycastHit2D hit = Physics2D.Raycast(position, desired, Mathf.Min(2f, remaining));
            avoidanceDirection = hit.collider ? Vector2.Perpendicular(desired) * Mathf.Sign(Vector2.Dot(Vector2.Perpendicular(desired), hit.normal)) : Vector2.zero;
        }

        Vector2 direction = (desired + avoidanceDirection * 0.75f).normalized;
        float speed = Mathf.Max(0.01f, person.MoveSpeed + person.MoveSpeedSlow);
        person.TryMoveSafely(direction * (speed * Time.deltaTime), out _);
        eva.ConsumeOxygen(Time.deltaTime);
        person.IsRun = true;
        return MovementStatus.Running;
    }

    public void Cancel(Person person)
    {
        avoidanceDirection = Vector2.zero;
        if (person)
        {
            person.IsRun = false;
        }
    }
}
