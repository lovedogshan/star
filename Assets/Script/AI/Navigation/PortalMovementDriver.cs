using UnityEngine;

public class PortalMovementDriver : IMovementDriver
{
    private float elapsed;
    private AirlockPortal activePortal;

    public MovementStatus Tick(Person person, NavigationSegment segment)
    {
        AirlockPortal portal = segment.Portal;
        NavigationRegion destination = segment.Region;
        if (!portal || !destination || !portal.CanTraverse(person, destination.RegionId))
        {
            Cancel(person);
            return MovementStatus.Failed;
        }

        if (activePortal != portal)
        {
            Cancel(person);
            activePortal = portal;
            if (ReservationManager.Instance && !ReservationManager.Instance.TryReserve(person, portal))
            {
                return MovementStatus.Waiting;
            }
        }

        if (destination.Domain == NavigationDomain.ExteriorSpace &&
            (!person.EvaCapability || !person.EvaCapability.HasOxygenBudget(0f, segment.RequiredOxygenSeconds)))
        {
            Cancel(person);
            return MovementStatus.Failed;
        }

        elapsed += Time.deltaTime;
        portal.SetOpen(true);
        if (elapsed < portal.CycleDuration)
        {
            return MovementStatus.Waiting;
        }

        Vector3 exitPosition = portal.GetExitPosition(destination.RegionId);
        Transform destinationRoot = destination.CoordinateRoot;
        if (destinationRoot && person.transform.parent != destinationRoot)
        {
            person.transform.SetParent(destinationRoot, true);
        }

        person.transform.position = exitPosition;
        person.SetCurrentRegion(destination);
        portal.SetOpen(false);
        ReservationManager.Instance?.Release(person, portal);
        activePortal = null;
        elapsed = 0f;
        return MovementStatus.Arrived;
    }

    public void Cancel(Person person)
    {
        if (activePortal)
        {
            activePortal.SetOpen(false);
            ReservationManager.Instance?.Release(person, activePortal);
        }

        activePortal = null;
        elapsed = 0f;
    }
}
