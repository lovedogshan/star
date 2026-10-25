using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Builds an interior navigation grid from station floor colliders and removes cells
/// occupied by static walls and facilities. NPCs and other dynamic actors are ignored
/// here and continue to be handled by the runtime avoidance and reservation systems.
/// </summary>
public sealed class StationGridBaker : MonoBehaviour
{
    [SerializeField]
    private Shiper shipGridOwner;
    [SerializeField]
    private Transform coordinateRoot;
    [SerializeField]
    private StationFloorSurface[] floorSurfaces;
    [SerializeField]
    private Collider2D[] floorColliders;
    [SerializeField]
    private LayerMask obstacleLayers = Physics2D.AllLayers;
    [SerializeField, Min(0f)]
    private float agentRadius = 0.2f;
    [SerializeField, Min(1)]
    private int cellsPerFrame = 128;
    [SerializeField]
    private bool includeTriggerObstacles;
    [SerializeField]
    private bool ignorePersons = true;

    private readonly HashSet<Collider2D> floorColliderSet = new HashSet<Collider2D>();
    private Collider2D[] overlapBuffer = new Collider2D[64];
    private bool isBaking;
    private bool isReady;

    public bool IsBaking => isBaking;
    public bool IsReady => isReady;
    public Transform CoordinateRoot => coordinateRoot ? coordinateRoot : transform;

    private void Awake()
    {
        ResolveReferences();
        isReady = false;
    }

    public IEnumerator BakeIncremental(bool force = false)
    {
        if (isBaking)
        {
            while (isBaking)
            {
                yield return null;
            }

            yield break;
        }

        if (isReady && !force)
        {
            yield break;
        }

        ResolveReferences();
        Physics2D.SyncTransforms();
        if (!shipGridOwner || !CoordinateRoot || !TryBuildFloorBounds(out Bounds localBounds))
        {
            isReady = false;
            Debug.LogWarning("StationGridBaker needs a Shiper, coordinate root and at least one valid floor collider.", this);
            yield break;
        }

        PathGrid grid = shipGridOwner.ShipGrid;
        if (grid == null)
        {
            isReady = false;
            yield break;
        }

        isBaking = true;
        isReady = false;

        Vector2Int minCell = new Vector2Int(
            Mathf.CeilToInt(localBounds.min.x),
            Mathf.CeilToInt(localBounds.min.y));
        Vector2Int maxCell = new Vector2Int(
            Mathf.FloorToInt(localBounds.max.x),
            Mathf.FloorToInt(localBounds.max.y));

        if (minCell.x > maxCell.x || minCell.y > maxCell.y)
        {
            isBaking = false;
            yield break;
        }

        Vector2Int size = maxCell - minCell + Vector2Int.one;
        Vector2Int center = new Vector2Int(-minCell.x, -minCell.y);
        grid.Reset(size, center);

        int budget = Mathf.Max(1, cellsPerFrame);
        int checks = 0;

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (IsCellInsideFloor(cell))
                {
                    grid.AddNode(cell, out PathNode node);
                    node.isCollision = false;
                }

                checks++;
                if (checks >= budget)
                {
                    checks = 0;
                    yield return null;
                }
            }
        }

        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            for (int y = minCell.y; y <= maxCell.y; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!grid.ToPathNode(cell, out PathNode node) || node == null)
                {
                    continue;
                }

                node.isCollision = IsOccupiedByStaticObstacle(cell);
                checks++;
                if (checks >= budget)
                {
                    checks = 0;
                    yield return null;
                }
            }
        }

        ApplyConstructionOccupancy(grid);
        isBaking = false;
        isReady = true;
    }

    public bool ContainsFloorPoint(Vector3 worldPoint)
    {
        Vector2 point = worldPoint;
        for (int i = 0; i < floorColliders.Length; i++)
        {
            Collider2D floor = floorColliders[i];
            if (floor && floor.enabled && floor.OverlapPoint(point))
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveReferences()
    {
        if (!shipGridOwner)
        {
            shipGridOwner = GetComponent<Shiper>() ?? GetComponentInParent<Shiper>();
        }

        if (!coordinateRoot)
        {
            coordinateRoot = shipGridOwner ? shipGridOwner.transform : transform;
        }

        floorColliderSet.Clear();
        List<Collider2D> resolved = new List<Collider2D>();

        if (floorSurfaces == null || floorSurfaces.Length == 0)
        {
            floorSurfaces = GetComponentsInChildren<StationFloorSurface>(true);
        }

        if (floorSurfaces != null)
        {
            for (int i = 0; i < floorSurfaces.Length; i++)
            {
                StationFloorSurface surface = floorSurfaces[i];
                if (!surface)
                {
                    continue;
                }

                Collider2D[] sources = surface.Colliders;
                for (int j = 0; j < sources.Length; j++)
                {
                    AddFloorCollider(sources[j], resolved);
                }
            }
        }

        if (floorColliders != null)
        {
            for (int i = 0; i < floorColliders.Length; i++)
            {
                AddFloorCollider(floorColliders[i], resolved);
            }
        }

        floorColliders = resolved.ToArray();
    }

    private void AddFloorCollider(Collider2D collider, List<Collider2D> resolved)
    {
        if (collider && collider.enabled && floorColliderSet.Add(collider))
        {
            resolved.Add(collider);
        }
    }

    private bool TryBuildFloorBounds(out Bounds localBounds)
    {
        localBounds = default;
        if (floorColliders == null || floorColliders.Length == 0)
        {
            return false;
        }

        bool hasBounds = false;
        for (int i = 0; i < floorColliders.Length; i++)
        {
            Collider2D floor = floorColliders[i];
            if (!floor || !floor.enabled)
            {
                continue;
            }

            Bounds worldBounds = floor.bounds;
            Vector3[] corners =
            {
                new Vector3(worldBounds.min.x, worldBounds.min.y, 0f),
                new Vector3(worldBounds.min.x, worldBounds.max.y, 0f),
                new Vector3(worldBounds.max.x, worldBounds.min.y, 0f),
                new Vector3(worldBounds.max.x, worldBounds.max.y, 0f)
            };

            for (int cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
            {
                Vector3 local = CoordinateRoot.InverseTransformPoint(corners[cornerIndex]);
                if (!hasBounds)
                {
                    localBounds = new Bounds(local, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(local);
                }
            }
        }

        return hasBounds;
    }

    private bool IsCellInsideFloor(Vector2Int cell)
    {
        float half = 0.45f;
        Vector2[] localPoints =
        {
            cell,
            new Vector2(cell.x - half, cell.y - half),
            new Vector2(cell.x - half, cell.y + half),
            new Vector2(cell.x + half, cell.y - half),
            new Vector2(cell.x + half, cell.y + half)
        };

        for (int pointIndex = 0; pointIndex < localPoints.Length; pointIndex++)
        {
            Vector2 worldPoint = CoordinateRoot.TransformPoint(localPoints[pointIndex]);
            if (!ContainsFloorPoint(worldPoint))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsOccupiedByStaticObstacle(Vector2Int cell)
    {
        Vector2 worldCenter = CoordinateRoot.TransformPoint(new Vector2(cell.x, cell.y));
        Vector3 scale = CoordinateRoot.lossyScale;
        Vector2 worldSize = new Vector2(
            Mathf.Abs(scale.x) * (1f + agentRadius * 2f),
            Mathf.Abs(scale.y) * (1f + agentRadius * 2f));
        float angle = CoordinateRoot.eulerAngles.z;
        int hitCount = Physics2D.OverlapBoxNonAlloc(
            worldCenter,
            worldSize,
            angle,
            overlapBuffer,
            obstacleLayers);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (IsStaticObstacle(hit))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsStaticObstacle(Collider2D collider)
    {
        if (!collider || !collider.enabled || !collider.gameObject.activeInHierarchy ||
            floorColliderSet.Contains(collider))
        {
            return false;
        }

        if (!includeTriggerObstacles && collider.isTrigger)
        {
            return false;
        }

        if (ignorePersons && collider.GetComponentInParent<Person>() != null)
        {
            return false;
        }

        Transform root = CoordinateRoot;
        return collider.transform == root || collider.transform.IsChildOf(root);
    }

    private void ApplyConstructionOccupancy(PathGrid grid)
    {
        if (!shipGridOwner || shipGridOwner.ShipComponents == null)
        {
            return;
        }

        foreach (Construction construction in shipGridOwner.ShipComponents)
        {
            if (construction && !(construction is Cabin) && construction.IsHinder && construction.Config != null)
            {
                grid.AddCollision(construction);
            }
        }
    }
}
