using System.Collections.Generic;
using UnityEngine;

public class AirlockPortal : MonoBehaviour
{
    private static readonly List<AirlockPortal> Portals = new List<AirlockPortal>();

    [SerializeField]
    private NavigationRegion interiorRegion;
    [SerializeField]
    private NavigationRegion exteriorRegion;
    [SerializeField]
    private Transform interiorPoint;
    [SerializeField]
    private Transform exteriorPoint;
    [SerializeField]
    private bool isOperational = true;
    [SerializeField]
    private bool isOpen;
    [SerializeField]
    private float cycleDuration = 1.5f;

    public NavigationRegion InteriorRegion => interiorRegion;
    public NavigationRegion ExteriorRegion => exteriorRegion;
    public Transform InteriorPoint => interiorPoint ? interiorPoint : transform;
    public Transform ExteriorPoint => exteriorPoint ? exteriorPoint : transform;
    public bool IsOperational => isOperational;
    public bool IsOpen => isOpen;
    public float CycleDuration => Mathf.Max(0f, cycleDuration);

    private void OnEnable()
    {
        if (!Portals.Contains(this))
        {
            Portals.Add(this);
        }
    }

    private void OnDisable()
    {
        Portals.Remove(this);
    }

    public bool Connects(int fromRegionId, int toRegionId)
    {
        if (!interiorRegion || !exteriorRegion)
        {
            return false;
        }

        return interiorRegion.RegionId == fromRegionId && exteriorRegion.RegionId == toRegionId ||
               exteriorRegion.RegionId == fromRegionId && interiorRegion.RegionId == toRegionId;
    }

    public bool CanTraverse(Person person, int destinationRegionId)
    {
        return person && CanTraverseFrom(person, person.CurrentRegionId, destinationRegionId);
    }

    public bool CanTraverseFrom(Person person, int fromRegionId, int destinationRegionId)
    {
        if (!isOperational || !person || !Connects(fromRegionId, destinationRegionId))
        {
            return false;
        }

        bool enteringExterior = exteriorRegion && exteriorRegion.RegionId == destinationRegionId;
        return !enteringExterior || person.EvaCapability && person.EvaCapability.CanEva;
    }

    public Vector3 GetEntryPosition(int fromRegionId)
    {
        return interiorRegion && interiorRegion.RegionId == fromRegionId ? InteriorPoint.position : ExteriorPoint.position;
    }

    public Vector3 GetExitPosition(int destinationRegionId)
    {
        return exteriorRegion && exteriorRegion.RegionId == destinationRegionId ? ExteriorPoint.position : InteriorPoint.position;
    }

    public NavigationRegion GetRegion(int regionId)
    {
        if (interiorRegion && interiorRegion.RegionId == regionId)
        {
            return interiorRegion;
        }

        return exteriorRegion && exteriorRegion.RegionId == regionId ? exteriorRegion : null;
    }

    public NavigationRegion GetOtherRegion(int regionId)
    {
        if (interiorRegion && interiorRegion.RegionId == regionId)
        {
            return exteriorRegion;
        }

        return exteriorRegion && exteriorRegion.RegionId == regionId ? interiorRegion : null;
    }

    public void SetOpen(bool value)
    {
        isOpen = value;
    }

    public static bool TryFind(int fromRegionId, int toRegionId, out AirlockPortal portal)
    {
        portal = Portals.Find(item => item && item.isOperational && item.Connects(fromRegionId, toRegionId));
        return portal;
    }

    public static bool CanTravel(Person person, int fromRegionId, int toRegionId)
    {
        List<AirlockPortal> route = new List<AirlockPortal>();
        return TryFindRoute(person, fromRegionId, toRegionId, route);
    }

    public static bool TryFindRoute(Person person, int fromRegionId, int toRegionId, List<AirlockPortal> route)
    {
        route.Clear();
        if (!person)
        {
            return false;
        }

        if (fromRegionId == toRegionId)
        {
            return true;
        }

        Queue<int> open = new Queue<int>();
        HashSet<int> visited = new HashSet<int>();
        Dictionary<int, int> previousRegion = new Dictionary<int, int>();
        Dictionary<int, AirlockPortal> previousPortal = new Dictionary<int, AirlockPortal>();
        open.Enqueue(fromRegionId);
        visited.Add(fromRegionId);

        while (open.Count > 0)
        {
            int current = open.Dequeue();
            foreach (AirlockPortal portal in Portals)
            {
                if (!portal || !portal.isOperational)
                {
                    continue;
                }

                NavigationRegion nextRegion = portal.GetOtherRegion(current);
                if (!nextRegion || visited.Contains(nextRegion.RegionId) || !portal.CanTraverseFrom(person, current, nextRegion.RegionId))
                {
                    continue;
                }

                visited.Add(nextRegion.RegionId);
                previousRegion[nextRegion.RegionId] = current;
                previousPortal[nextRegion.RegionId] = portal;
                if (nextRegion.RegionId == toRegionId)
                {
                    int cursor = toRegionId;
                    while (cursor != fromRegionId)
                    {
                        route.Add(previousPortal[cursor]);
                        cursor = previousRegion[cursor];
                    }

                    route.Reverse();
                    return true;
                }

                open.Enqueue(nextRegion.RegionId);
            }
        }

        return false;
    }
}
