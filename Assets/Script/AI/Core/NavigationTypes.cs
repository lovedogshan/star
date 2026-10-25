using System;
using System.Collections.Generic;
using UnityEngine;

public enum NavigationDomain
{
    InteriorGrid,
    ExteriorSpace,
    Portal,
    RemoteSpace
}

public enum MovementStatus
{
    Idle,
    Waiting,
    Running,
    Arrived,
    Failed
}

[Serializable]
public class NavigationTarget
{
    public Transform TargetTransform;
    public Vector3 Position;
    public NavigationRegion Region;
    public float StoppingDistance = 1f;
    public bool RequiresEva;
    public float EstimatedWorkTime;

    public Vector3 WorldPosition => TargetTransform ? TargetTransform.position : Position;
}

[Serializable]
public class NavigationSegment
{
    public NavigationDomain Domain;
    public NavigationRegion Region;
    public Vector3 WorldTarget;
    public float StoppingDistance;
    public AirlockPortal Portal;
    public bool EnteringExterior;
    public float RequiredOxygenSeconds;
}

[Serializable]
public class NavigationRoute
{
    [SerializeField]
    private List<NavigationSegment> segments = new List<NavigationSegment>();

    public List<NavigationSegment> Segments => segments;
    public bool IsValid => segments.Count > 0;

    public void Add(NavigationSegment segment)
    {
        if (segment != null)
        {
            segments.Add(segment);
        }
    }
}
