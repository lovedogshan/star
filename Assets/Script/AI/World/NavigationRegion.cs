using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class NavigationRegion : MonoBehaviour
{
    private static readonly Dictionary<int, NavigationRegion> Regions = new Dictionary<int, NavigationRegion>();

    [SerializeField]
    private int regionId;
    [SerializeField]
    private NavigationDomain domain = NavigationDomain.InteriorGrid;
    [SerializeField]
    private Transform coordinateRoot;
    [SerializeField]
    private Shiper shipGridOwner;
    [SerializeField]
    private bool buildFallbackGridIfEmpty;
    [SerializeField]
    private Vector2Int fallbackGridMin = new Vector2Int(-10, -10);
    [SerializeField]
    private Vector2Int fallbackGridMax = new Vector2Int(10, 10);
    [SerializeField]
    private float agentRadius = 0.2f;
    [SerializeField]
    private LayerMask obstacleLayers = Physics2D.AllLayers;
    [SerializeField, Min(1)]
    private int obstacleChecksPerFrame = 48;
    [SerializeField]
    private bool bakeObstaclesAtRuntime;
    [SerializeField]
    private StationGridBaker stationGridBaker;
    private bool isReady;
    public int RegionId => regionId != 0 ? regionId : GetInstanceID();
    public NavigationDomain Domain => domain;
    public Transform CoordinateRoot => coordinateRoot ? coordinateRoot : transform;
    public PathGrid Grid => shipGridOwner ? shipGridOwner.ShipGrid : null;
    public bool IsReady => stationGridBaker ? stationGridBaker.IsReady : isReady;

    /// <summary>
    /// Tests region ownership independently from walkability. A click may start on
    /// a wall or facility, while the drag can still contain valid floor cells.
    /// </summary>
    public bool ContainsWorldPoint(Vector3 worldPoint)
    {
        if (!IsReady)
        {
            return false;
        }

        if (stationGridBaker && stationGridBaker.ContainsFloorPoint(worldPoint))
        {
            return true;
        }

        PathGrid grid = Grid;
        if (grid == null)
        {
            return false;
        }

        Vector3 local = CoordinateRoot.InverseTransformPoint(worldPoint);
        Vector2Int cell = new Vector2Int(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y));
        return grid.ToPathNode(cell, out PathNode node) && node != null;
    }

    private void Awake()
    {
        if (!shipGridOwner)
        {
            shipGridOwner = GetComponent<Shiper>() ?? GetComponentInParent<Shiper>();
        }

        if (!stationGridBaker)
        {
            stationGridBaker = GetComponent<StationGridBaker>() ?? GetComponentInParent<StationGridBaker>();
        }

        if (stationGridBaker)
        {
            isReady = false;
        }
        else if (buildFallbackGridIfEmpty && shipGridOwner)
        {
            shipGridOwner.EnsureFallbackGrid(fallbackGridMin, fallbackGridMax);
            isReady = true;
        }

        Regions[RegionId] = this;
        if (!stationGridBaker)
        {
            isReady = true;
        }
    }

    private IEnumerator Start()
    {
        if (stationGridBaker)
        {
            yield return stationGridBaker.BakeIncremental();
            isReady = stationGridBaker.IsReady;
        }
        else if (bakeObstaclesAtRuntime && shipGridOwner && shipGridOwner.ShipGrid != null)
        {
            isReady = false;
            yield return shipGridOwner.ShipGrid.BakeColliderObstaclesIncremental(
                CoordinateRoot,
                agentRadius,
                obstacleLayers,
                obstacleChecksPerFrame);

            isReady = true;
        }
    }

    private void OnDestroy()
    {
        if (Regions.TryGetValue(RegionId, out NavigationRegion region) && region == this)
        {
            Regions.Remove(RegionId);
        }
    }

    public static bool TryGet(int id, out NavigationRegion region)
    {
        return Regions.TryGetValue(id, out region);
    }

}
