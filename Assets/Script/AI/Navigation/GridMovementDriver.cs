using System.Collections.Generic;
using UnityEngine;

public class GridMovementDriver : IMovementDriver
{
    private readonly List<Vector2Int> path = new List<Vector2Int>();
    private int requestId;
    private bool requestPending;
    private bool requestFailed;
    private Vector2Int requestedTarget;
    private NavigationRegion requestedRegion;
    private bool hasRequestedTarget;
    private float stalledTime;
    private Vector2Int observedWaypoint;
    private bool hasObservedWaypoint;
    private bool waitingForDynamicBlocker;
    private float dynamicWaitTime;
    private float dynamicRecheckTime;
    private bool pathValidationNeeded;
    private int pathValidationIndex;
    private readonly HashSet<Vector2Int> temporarilyBlocked = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> dynamicBlocked = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> requestBlocked = new HashSet<Vector2Int>();

    public MovementStatus Tick(Person person, NavigationSegment segment)
    {
        Transform root = ResolveRoot(person, segment.Region);
        PathGrid grid = ResolveGrid(person, segment.Region);
        if (grid == null || person.FindPathPlus == null || !PathRequestScheduler.Instance)
        {
            return MovementStatus.Failed;
        }

        person.FindPathPlus.pathGrid = grid;
        Vector2Int target = RoundCell(root.InverseTransformPoint(segment.WorldTarget));
        if (!hasRequestedTarget || requestedRegion != segment.Region || target != requestedTarget)
        {
            temporarilyBlocked.Clear();
            dynamicBlocked.Clear();
            RequestPath(person, segment, root, target);
        }

        if (requestPending)
        {
            person.IsRun = false;
            return MovementStatus.Waiting;
        }

        if (requestFailed)
        {
            if (waitingForDynamicBlocker)
            {
                person.IsRun = false;
                dynamicWaitTime += Time.deltaTime;
                if (dynamicWaitTime >= 0.25f)
                {
                    requestFailed = false;
                    dynamicWaitTime = 0f;
                    RequestPath(person, segment, root, target);
                }

                return MovementStatus.Waiting;
            }

            return MovementStatus.Failed;
        }

        if (path.Count == 0)
        {
            person.IsRun = false;
            return MovementStatus.Arrived;
        }

        if (pathValidationNeeded)
        {
            PathValidationResult validation = ValidatePathIncremental(
                person,
                root,
                path,
                6,
                out Collider2D blocker,
                out Vector2Int blockedCell);
            if (validation == PathValidationResult.Blocked)
            {
                stalledTime = 0f;
                Person blockerPerson = blocker ? blocker.GetComponentInParent<Person>() : null;
                if (blockerPerson && blockerPerson != person)
                {
                    waitingForDynamicBlocker = true;
                    dynamicWaitTime = 0f;
                }
                else
                {
                    temporarilyBlocked.Add(blockedCell);
                    AddColliderObstacleCells(person, root, grid, blocker, temporarilyBlocked);
                }

                RequestPath(person, segment, root, target);
                person.IsRun = false;
                return MovementStatus.Waiting;
            }

            if (validation == PathValidationResult.Pending)
            {
                person.IsRun = false;
                return MovementStatus.Waiting;
            }

            pathValidationNeeded = false;
            pathValidationIndex = 0;
        }

        dynamicRecheckTime -= Time.deltaTime;
        if (dynamicRecheckTime <= 0f)
        {
            dynamicRecheckTime = 0.2f;
            dynamicBlocked.Clear();
            RefreshDynamicPersonObstacles(person, root, grid, dynamicBlocked);
            if (HasUpcomingDynamicConflict(path, dynamicBlocked, 6))
            {
                waitingForDynamicBlocker = true;
                dynamicWaitTime = 0f;
                RequestPath(person, segment, root, target);
                person.IsRun = false;
                return MovementStatus.Waiting;
            }
        }

        if (!hasObservedWaypoint || observedWaypoint != path[0])
        {
            observedWaypoint = path[0];
            hasObservedWaypoint = true;
            stalledTime = 0f;
        }

        Vector3 waypoint = root.TransformPoint(new Vector3(path[0].x, path[0].y, root.InverseTransformPoint(person.transform.position).z));
        float distanceBeforeMove = Vector2.Distance(person.transform.position, waypoint);
        float speed = Mathf.Max(0.01f, person.MoveSpeed + person.MoveSpeedSlow);
        Vector3 next = Vector3.MoveTowards(person.transform.position, waypoint, speed * Time.deltaTime);
        Vector2 direction = next - person.transform.position;
        if (Mathf.Abs(direction.x) > 0.001f)
        {
            Vector3 scale = person.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
            person.transform.localScale = scale;
        }

        Vector2 requestedDelta = next - person.transform.position;
        person.TryMoveSafely(requestedDelta, out Vector2 actualDelta);
        float distanceAfterMove = Vector2.Distance(person.transform.position, waypoint);
        float progress = distanceBeforeMove - distanceAfterMove;
        bool movementBlocked = person.LastMovementBlocker &&
                               actualDelta.sqrMagnitude < requestedDelta.sqrMagnitude * 0.81f;
        if (movementBlocked || progress <= 0.0005f)
        {
            stalledTime += Time.deltaTime;
            if (stalledTime >= 0.3f)
            {
                stalledTime = 0f;
                Person blockerPerson = person.LastMovementBlocker
                    ? person.LastMovementBlocker.GetComponentInParent<Person>()
                    : null;

                waitingForDynamicBlocker = blockerPerson && blockerPerson != person;
                dynamicWaitTime = 0f;
                if (blockerPerson && blockerPerson != person)
                {
                    // Dynamic people are included in every new A* request. If they close a
                    // narrow corridor completely, the request waits and retries as they move.
                    RequestPath(person, segment, root, target);
                }
                else
                {
                    temporarilyBlocked.Add(path[0]);
                    if (person.LastMovementBlocker)
                    {
                        AddColliderObstacleCells(person, root, grid, person.LastMovementBlocker, temporarilyBlocked);
                    }

                    RequestPath(person, segment, root, target);
                }
                person.IsRun = false;
                return MovementStatus.Waiting;
            }
        }
        else
        {
            stalledTime = 0f;
            dynamicWaitTime = 0f;
            if (!person.LastMovementBlocker)
            {
                waitingForDynamicBlocker = false;
            }
        }

        if (Vector2.Distance(person.transform.position, waypoint) <= 0.001f)
        {
            path.RemoveAt(0);
            hasObservedWaypoint = false;
            SyncLegacyPath(person);
        }

        person.IsRun = path.Count > 0;
        return person.IsRun ? MovementStatus.Running : MovementStatus.Arrived;
    }

    public void Cancel(Person person)
    {
        if (requestPending && PathRequestScheduler.Instance)
        {
            PathRequestScheduler.Instance.Cancel(requestId);
        }

        requestPending = false;
        requestFailed = false;
        requestId = 0;
        requestedRegion = null;
        hasRequestedTarget = false;
        temporarilyBlocked.Clear();
        dynamicBlocked.Clear();
        requestBlocked.Clear();
        path.Clear();
        stalledTime = 0f;
        hasObservedWaypoint = false;
        waitingForDynamicBlocker = false;
        dynamicWaitTime = 0f;
        dynamicRecheckTime = 0f;
        pathValidationNeeded = false;
        pathValidationIndex = 0;
        if (person)
        {
            person._getPaths.Clear();
            person.IsRun = false;
        }
    }

    private void RequestPath(Person person, NavigationSegment segment, Transform root, Vector2Int target)
    {
        if (requestPending && PathRequestScheduler.Instance)
        {
            PathRequestScheduler.Instance.Cancel(requestId);
        }

        requestPending = false;
        requestFailed = false;
        requestId = 0;
        path.Clear();
        hasObservedWaypoint = false;
        stalledTime = 0f;
        pathValidationNeeded = true;
        pathValidationIndex = 0;
        SyncLegacyPath(person);
        requestedRegion = segment.Region;
        requestedTarget = target;
        hasRequestedTarget = true;
        requestPending = true;
        PathGrid grid = ResolveGrid(person, segment.Region);
        dynamicBlocked.Clear();
        RefreshDynamicPersonObstacles(person, root, grid, dynamicBlocked);
        requestBlocked.Clear();
        requestBlocked.UnionWith(temporarilyBlocked);
        requestBlocked.UnionWith(dynamicBlocked);
        Vector2Int start = RoundCell(root.InverseTransformPoint(person.transform.position));
        int range = Mathf.Max(0, Mathf.RoundToInt(segment.StoppingDistance));
        requestBlocked.Remove(start);
        QueuePathRequest(person, start, target, range, requestBlocked, dynamicBlocked.Count > 0);
    }

    private void QueuePathRequest(Person person, Vector2Int start, Vector2Int target,
        int range, ISet<Vector2Int> blockedCells, bool includesDynamicObstacles)
    {
        requestPending = true;
        requestId = PathRequestScheduler.Instance.RequestPath(person.FindPathPlus, start, target, range,
            person.IBehavior != null && person.IBehavior.CurrentTask != null && person.IBehavior.CurrentTask.IsEmergency,
            blockedCells,
            (success, result) =>
            {
                requestPending = false;
                requestFailed = !success;
                waitingForDynamicBlocker = !success && includesDynamicObstacles;
                dynamicWaitTime = 0f;
                path.Clear();
                if (success && result != null)
                {
                    path.AddRange(result);
                    dynamicRecheckTime = 0.2f;
                }

                SyncLegacyPath(person);
            });
    }

    private void SyncLegacyPath(Person person)
    {
        if (!person)
        {
            return;
        }

        person._getPaths.Clear();
        person._getPaths.AddRange(path);
    }

    private static Transform ResolveRoot(Person person, NavigationRegion region)
    {
        if (region)
        {
            return region.CoordinateRoot;
        }

        if (person.shiper)
        {
            return person.shiper.transform;
        }

        return person.transform.parent ? person.transform.parent : person.transform;
    }

    private static PathGrid ResolveGrid(Person person, NavigationRegion region)
    {
        return region && region.Grid != null ? region.Grid : person.shiper ? person.shiper.ShipGrid : person.FindPathPlus.pathGrid;
    }

    private static Vector2Int RoundCell(Vector3 position)
    {
        return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    private static void AddColliderObstacleCells(Person person, Transform root, PathGrid grid, Collider2D blocker,
        HashSet<Vector2Int> blockedCells)
    {
        if (!person || !root || grid == null || !blocker || blocker.isTrigger || blockedCells == null)
        {
            return;
        }

        Bounds bounds = blocker.bounds;
        Vector3[] corners =
        {
            new Vector3(bounds.min.x, bounds.min.y),
            new Vector3(bounds.min.x, bounds.max.y),
            new Vector3(bounds.max.x, bounds.min.y),
            new Vector3(bounds.max.x, bounds.max.y)
        };

        Vector2 localMin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 localMax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        foreach (Vector3 corner in corners)
        {
            Vector3 local = root.InverseTransformPoint(corner);
            localMin = Vector2.Min(localMin, local);
            localMax = Vector2.Max(localMax, local);
        }

        int minX = Mathf.FloorToInt(localMin.x) - 1;
        int maxX = Mathf.CeilToInt(localMax.x) + 1;
        int minY = Mathf.FloorToInt(localMin.y) - 1;
        int maxY = Mathf.CeilToInt(localMax.y) + 1;
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!grid.CheckBord(cell))
                {
                    continue;
                }

                Vector2 worldCell = root.TransformPoint(new Vector2(x, y));
                bool overlapsBlocker = blocker.OverlapPoint(worldCell);
                if (!overlapsBlocker)
                {
                    Collider2D[] nearby = Physics2D.OverlapCircleAll(
                        worldCell,
                        GetAgentSampleRadius(person),
                        Physics2D.AllLayers);
                    foreach (Collider2D nearbyCollider in nearby)
                    {
                        if (nearbyCollider == blocker)
                        {
                            overlapsBlocker = true;
                            break;
                        }
                    }
                }

                if (overlapsBlocker)
                {
                    blockedCells.Add(cell);
                }
            }
        }
    }

    private static float GetAgentSampleRadius(Person person)
    {
        Collider2D collider = person.GetComponent<Collider2D>();
        if (!collider)
        {
            return 0.2f;
        }

        return Mathf.Max(0.05f, Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y));
    }

    private enum PathValidationResult
    {
        Clear,
        Pending,
        Blocked
    }

    private PathValidationResult ValidatePathIncremental(Person person, Transform root, List<Vector2Int> routePath,
        int segmentsPerFrame,
        out Collider2D blocker, out Vector2Int blockedCell)
    {
        blocker = null;
        blockedCell = default;
        Collider2D movementCollider = person.GetComponent<Collider2D>();
        if (!person || !root || routePath == null || !movementCollider || !movementCollider.enabled)
        {
            return PathValidationResult.Clear;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(Physics2D.AllLayers);
        filter.useTriggers = false;
        RaycastHit2D[] hits = new RaycastHit2D[32];
        int startIndex = Mathf.Clamp(pathValidationIndex, 0, routePath.Count);
        Vector2 current = startIndex == 0
            ? (Vector2)person.transform.position
            : (Vector2)root.TransformPoint(new Vector3(
                routePath[startIndex - 1].x,
                routePath[startIndex - 1].y,
                root.InverseTransformPoint(person.transform.position).z));
        float localZ = root.InverseTransformPoint(person.transform.position).z;
        int endIndex = Mathf.Min(routePath.Count, startIndex + Mathf.Max(1, segmentsPerFrame));

        for (int i = startIndex; i < endIndex; i++)
        {
            Vector2Int cell = routePath[i];
            Vector2 target = root.TransformPoint(new Vector3(cell.x, cell.y, localZ));
            Vector2 delta = target - current;
            float distance = delta.magnitude;
            if (distance <= 0.02f)
            {
                current = target;
                continue;
            }

            Vector2 direction = delta / distance;
            float radius = GetAgentSampleRadius(person);
            int hitCount = Physics2D.CircleCast(
                current,
                radius,
                direction,
                filter,
                hits,
                distance + 0.02f);
            for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
            {
                RaycastHit2D hit = hits[hitIndex];
                Person hitPerson = hit.collider ? hit.collider.GetComponentInParent<Person>() : null;
                if (!hit.collider || hit.collider == movementCollider || hit.collider.isTrigger ||
                    hitPerson == person || hit.distance >= distance - 0.02f ||
                    hit.distance <= 0.001f && IsMovingAwayFromCollider(hit.collider, current, target))
                {
                    continue;
                }

                blocker = hit.collider;
                blockedCell = cell;
                pathValidationIndex = 0;
                return PathValidationResult.Blocked;
            }

            current = target;
        }

        pathValidationIndex = endIndex;
        if (pathValidationIndex < routePath.Count)
        {
            return PathValidationResult.Pending;
        }

        pathValidationIndex = 0;
        return PathValidationResult.Clear;
    }

    private static bool IsMovingAwayFromCollider(Collider2D blocker, Vector2 from, Vector2 to)
    {
        if (!blocker)
        {
            return false;
        }

        float fromDistance = Vector2.SqrMagnitude(from - blocker.ClosestPoint(from));
        float toDistance = Vector2.SqrMagnitude(to - blocker.ClosestPoint(to));
        return toDistance > fromDistance + 0.0001f;
    }

    private static void RefreshDynamicPersonObstacles(Person requester, Transform root, PathGrid grid,
        HashSet<Vector2Int> blockedCells)
    {
        if (!requester || !root || grid == null || blockedCells == null || !TaskController.Instance)
        {
            return;
        }

        foreach (Person other in TaskController.Instance.People)
        {
            if (!other || other == requester || !other.gameObject.activeInHierarchy ||
                other.CurrentRegion != requester.CurrentRegion)
            {
                continue;
            }

            Transform otherRoot = ResolveRoot(other, other.CurrentRegion);
            if (otherRoot != root)
            {
                continue;
            }

            Collider2D otherCollider = other.GetComponent<Collider2D>();
            if (!otherCollider || !otherCollider.enabled || otherCollider.isTrigger)
            {
                continue;
            }

            AddColliderObstacleCells(requester, root, grid, otherCollider, blockedCells);
            int reservationCount = Mathf.Min(3, other._getPaths.Count);
            for (int pathIndex = 0; pathIndex < reservationCount; pathIndex++)
            {
                Vector2Int reservedCell = other._getPaths[pathIndex];
                if (grid.CheckBord(reservedCell))
                {
                    blockedCells.Add(reservedCell);
                }
            }
        }
    }

    private static bool HasUpcomingDynamicConflict(List<Vector2Int> routePath,
        HashSet<Vector2Int> blockedCells, int lookAheadCells)
    {
        int count = Mathf.Min(routePath.Count, Mathf.Max(1, lookAheadCells));
        for (int i = 0; i < count; i++)
        {
            if (blockedCells.Contains(routePath[i]))
            {
                return true;
            }
        }

        return false;
    }
}
