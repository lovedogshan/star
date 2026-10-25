using UnityEngine;

public class PersonNavigation : MonoBehaviour
{
    private readonly GridMovementDriver gridDriver = new GridMovementDriver();
    private readonly EvaMovementDriver evaDriver = new EvaMovementDriver();
    private readonly PortalMovementDriver portalDriver = new PortalMovementDriver();
    private readonly DirectMovementDriver directDriver = new DirectMovementDriver();

    private Person person;
    private NavigationRoute route;
    private NavigationTarget activeTarget;
    private int segmentIndex;

    public MovementStatus Status { get; private set; } = MovementStatus.Idle;
    public string FailureReason { get; private set; }

    private void Awake()
    {
        person = GetComponent<Person>();
    }

    public MovementStatus Navigate(Transform targetTransform, Vector3 position, float stoppingDistance, bool requiresEva = false, float estimatedWorkTime = 0f)
    {
        Vector3 worldTarget = targetTransform ? targetTransform.position : position;
        NavigationRegion targetRegion = targetTransform ? targetTransform.GetComponentInParent<NavigationRegion>() : person.CurrentRegion;
        if (person.CurrentRegion && !person.CurrentRegion.IsReady || targetRegion && !targetRegion.IsReady)
        {
            Status = MovementStatus.Waiting;
            return Status;
        }
        if (NeedsNewRoute(targetTransform, worldTarget, stoppingDistance, requiresEva, estimatedWorkTime, targetRegion))
        {
            NavigationTarget target = new NavigationTarget
            {
                TargetTransform = targetTransform,
                Position = worldTarget,
                Region = targetRegion,
                StoppingDistance = stoppingDistance,
                RequiresEva = requiresEva,
                EstimatedWorkTime = estimatedWorkTime
            };

            if (!TryBuildRoute(target, out NavigationRoute newRoute))
            {
                FailureReason = requiresEva ? "EVA capability or an operational airlock is unavailable." : "No route connects the navigation regions.";
                Status = MovementStatus.Failed;
                return Status;
            }

            Cancel();
            activeTarget = target;
            route = newRoute;
            segmentIndex = 0;
        }

        if (activeTarget != null && activeTarget.TargetTransform)
        {
            route.Segments[route.Segments.Count - 1].WorldTarget = activeTarget.TargetTransform.position;
        }

        if (route == null || segmentIndex >= route.Segments.Count)
        {
            Status = MovementStatus.Arrived;
            return Status;
        }

        NavigationSegment segment = route.Segments[segmentIndex];
        Status = GetDriver(segment.Domain).Tick(person, segment);
        if (Status == MovementStatus.Arrived)
        {
            GetDriver(segment.Domain).Cancel(person);
            segmentIndex++;
            Status = segmentIndex >= route.Segments.Count ? MovementStatus.Arrived : MovementStatus.Running;
        }

        return Status;
    }

    public void Cancel()
    {
        gridDriver.Cancel(person);
        evaDriver.Cancel(person);
        portalDriver.Cancel(person);
        directDriver.Cancel(person);
        route = null;
        activeTarget = null;
        segmentIndex = 0;
        Status = MovementStatus.Idle;
        FailureReason = null;
    }

    public bool CanReach(Vector3 worldPosition, float stoppingDistance)
    {
        NavigationRegion region = person.CurrentRegion;
        if (region && !region.IsReady)
        {
            return false;
        }
        if (region && region.Domain != NavigationDomain.InteriorGrid)
        {
            return !region || region.Domain != NavigationDomain.ExteriorSpace || person.EvaCapability && person.EvaCapability.CanEva;
        }

        Transform root = region ? region.CoordinateRoot : person.shiper ? person.shiper.transform : person.transform.parent;
        PathGrid grid = region && region.Grid != null ? region.Grid : person.FindPathPlus.pathGrid;
        if (!root || grid == null)
        {
            return false;
        }

        person.FindPathPlus.pathGrid = grid;
        Vector3 startLocal = root.InverseTransformPoint(person.transform.position);
        Vector3 endLocal = root.InverseTransformPoint(worldPosition);
        return person.FindPathPlus.FindingPath(new Vector2Int(Mathf.RoundToInt(startLocal.x), Mathf.RoundToInt(startLocal.y)),
            new Vector2Int(Mathf.RoundToInt(endLocal.x), Mathf.RoundToInt(endLocal.y)), out _, Mathf.RoundToInt(stoppingDistance));
    }

    public bool TryResolveNavigablePoint(Vector3 requestedWorldPosition, out Vector3 resolvedWorldPosition,
        int searchRadius = 5)
    {
        resolvedWorldPosition = requestedWorldPosition;
        NavigationRegion region = person.CurrentRegion;
        if (!region || region.Domain != NavigationDomain.InteriorGrid || !region.IsReady)
        {
            return !region || region.Domain != NavigationDomain.InteriorGrid;
        }

        Transform root = region.CoordinateRoot;
        PathGrid grid = region.Grid;
        if (!root || grid == null)
        {
            return false;
        }

        Vector3 requestedLocal = root.InverseTransformPoint(requestedWorldPosition);
        Vector2Int requestedCell = new Vector2Int(
            Mathf.RoundToInt(requestedLocal.x),
            Mathf.RoundToInt(requestedLocal.y));
        if (IsOpenCell(grid, requestedCell))
        {
            resolvedWorldPosition = root.TransformPoint(new Vector3(requestedCell.x, requestedCell.y, requestedLocal.z));
            return true;
        }

        Vector3 personLocal = root.InverseTransformPoint(person.transform.position);
        Vector2Int bestCell = default;
        float bestScore = float.PositiveInfinity;
        bool found = false;
        int radius = Mathf.Max(1, searchRadius);
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int candidate = requestedCell + new Vector2Int(x, y);
                if (!IsOpenCell(grid, candidate))
                {
                    continue;
                }

                float score = Vector2.SqrMagnitude(candidate - requestedCell) +
                              Vector2.SqrMagnitude(candidate - (Vector2)personLocal) * 0.001f;
                if (score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                bestCell = candidate;
                found = true;
            }
        }

        if (!found)
        {
            return false;
        }

        resolvedWorldPosition = root.TransformPoint(new Vector3(bestCell.x, bestCell.y, requestedLocal.z));
        return true;
    }

    private static bool IsOpenCell(PathGrid grid, Vector2Int cell)
    {
        return grid.ToPathNode(cell, out PathNode node) && node != null && !node.isCollision;
    }

    private bool TryBuildRoute(NavigationTarget target, out NavigationRoute newRoute)
    {
        newRoute = new NavigationRoute();
        NavigationRegion from = person.CurrentRegion;
        NavigationRegion to = target.Region ? target.Region : from;

        float travelSeconds = Vector2.Distance(person.transform.position, target.WorldPosition) / Mathf.Max(0.01f, person.MoveSpeed + person.MoveSpeedSlow);
        if (target.RequiresEva && (!person.EvaCapability || !person.EvaCapability.HasOxygenBudget(travelSeconds * 2f, target.EstimatedWorkTime)))
        {
            return false;
        }

        if (!from || !to || from.RegionId == to.RegionId)
        {
            NavigationDomain domain = to ? to.Domain : NavigationDomain.InteriorGrid;
            newRoute.Add(CreateSegment(domain, to, target.WorldPosition, target.StoppingDistance));
            return true;
        }

        System.Collections.Generic.List<AirlockPortal> portals = new System.Collections.Generic.List<AirlockPortal>();
        if (!AirlockPortal.TryFindRoute(person, from.RegionId, to.RegionId, portals))
        {
            return false;
        }

        NavigationRegion currentRegion = from;
        foreach (AirlockPortal portal in portals)
        {
            NavigationRegion nextRegion = portal.GetOtherRegion(currentRegion.RegionId);
            if (!nextRegion)
            {
                return false;
            }

            newRoute.Add(CreateSegment(currentRegion.Domain, currentRegion, portal.GetEntryPosition(currentRegion.RegionId), 0f));
            newRoute.Add(new NavigationSegment
            {
                Domain = NavigationDomain.Portal,
                Region = nextRegion,
                Portal = portal,
                WorldTarget = portal.GetExitPosition(nextRegion.RegionId),
                EnteringExterior = nextRegion.Domain == NavigationDomain.ExteriorSpace,
                RequiredOxygenSeconds = nextRegion.Domain == NavigationDomain.ExteriorSpace ? travelSeconds * 2f + target.EstimatedWorkTime : 0f
            });
            currentRegion = nextRegion;
        }

        newRoute.Add(CreateSegment(to.Domain, to, target.WorldPosition, target.StoppingDistance));
        return true;
    }

    private bool NeedsNewRoute(Transform targetTransform, Vector3 position, float stoppingDistance, bool requiresEva, float estimatedWorkTime, NavigationRegion region)
    {
        if (activeTarget == null || route == null || Status == MovementStatus.Failed)
        {
            return true;
        }

        return activeTarget.TargetTransform != targetTransform || activeTarget.Region != region || activeTarget.RequiresEva != requiresEva ||
               Mathf.Abs(activeTarget.EstimatedWorkTime - estimatedWorkTime) > 0.01f ||
               Mathf.Abs(activeTarget.StoppingDistance - stoppingDistance) > 0.01f ||
               (!targetTransform && Vector2.Distance(activeTarget.Position, position) > 0.5f);
    }

    private static NavigationSegment CreateSegment(NavigationDomain domain, NavigationRegion region, Vector3 target, float stoppingDistance)
    {
        return new NavigationSegment
        {
            Domain = domain,
            Region = region,
            WorldTarget = target,
            StoppingDistance = Mathf.Max(0f, stoppingDistance)
        };
    }

    private IMovementDriver GetDriver(NavigationDomain domain)
    {
        switch (domain)
        {
            case NavigationDomain.InteriorGrid:
                return gridDriver;
            case NavigationDomain.ExteriorSpace:
                return evaDriver;
            case NavigationDomain.Portal:
                return portalDriver;
            default:
                return directDriver;
        }
    }
}
