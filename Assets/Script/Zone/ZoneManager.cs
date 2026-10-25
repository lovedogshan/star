using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ZoneManager : MonoBehaviour
{
    public static ZoneManager Instance { get; private set; }

    [SerializeField]
    private List<ZoneTypeDefinition> zoneTypes = new List<ZoneTypeDefinition>();

    private readonly List<Zone> zones = new List<Zone>();
    private int nextZoneId = 1;
    private ZoneTypeDefinition runtimeStockpile;
    private ZoneRenderer zoneRenderer;

    public IReadOnlyList<Zone> Zones => zones;
    public event Action<Zone> OnZoneCreated;
    public event Action<Zone> OnZoneModified;
    public event Action<Zone> OnZoneDeleted;
    public event Action<Zone> OnZoneSettingsChanged;
    public event Action<Zone, IReadOnlyCollection<Vector2Int>> OnZoneCellsChanged;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        zoneRenderer = GetComponent<ZoneRenderer>() ?? gameObject.AddComponent<ZoneRenderer>();
        if (!GetComponent<StockpileZoneTaskGenerator>())
        {
            gameObject.AddComponent<StockpileZoneTaskGenerator>();
        }
    }

    public IReadOnlyList<ZoneTypeDefinition> GetAvailableTypes()
    {
        if (zoneTypes.Count > 0)
        {
            return zoneTypes;
        }

        if (!runtimeStockpile)
        {
            runtimeStockpile = ZoneTypeDefinition.CreateRuntimeStockpile();
        }

        return new[] { runtimeStockpile };
    }

    public Zone CreateZone(ZoneTypeDefinition type, NavigationRegion region,
        IEnumerable<Vector2Int> requestedCells)
    {
        if (!type || type.kind != ZoneKind.Stockpile)
        {
            return null;
        }

        List<Vector2Int> validCells = FilterValidCells(region, requestedCells);
        if (validCells.Count == 0)
        {
            return null;
        }

        Zone existing = FindOverlapping(type.kind, region, validCells);
        if (existing != null && existing.settings.mergeSameType)
        {
            existing.AddCells(validCells);
            zoneRenderer.Render(existing, type.previewColor);
            OnZoneModified?.Invoke(existing);
            OnZoneCellsChanged?.Invoke(existing, validCells);
            return existing;
        }

        Zone zone = new Zone();
        zone.Initialize(nextZoneId++, type.kind, region, validCells, type.CreateDefaultSettings());
        zones.Add(zone);
        zoneRenderer.Render(zone, type.previewColor);
        OnZoneCreated?.Invoke(zone);
        OnZoneCellsChanged?.Invoke(zone, validCells);
        return zone;
    }

    public bool DeleteZone(int zoneId)
    {
        Zone zone = GetZone(zoneId);
        if (zone == null)
        {
            return false;
        }

        zoneRenderer.ClearZone(zone.id);
        zones.Remove(zone);
        OnZoneDeleted?.Invoke(zone);
        return true;
    }

    public bool TryDeleteAtWorld(Vector3 worldPosition, float radius = 0.75f)
    {
        Zone zone = FindAtWorld(worldPosition, radius);
        return zone != null && DeleteZone(zone.id);
    }

    public bool SetPriority(int zoneId, int priority)
    {
        Zone zone = GetZone(zoneId);
        if (zone == null)
        {
            return false;
        }

        zone.settings.priority = Mathf.Clamp(priority, 0, 3);
        OnZoneSettingsChanged?.Invoke(zone);
        return true;
    }

    public Zone GetZone(int zoneId)
    {
        for (int i = 0; i < zones.Count; i++)
        {
            if (zones[i].id == zoneId)
            {
                return zones[i];
            }
        }

        return null;
    }

    public Zone FindAtWorld(Vector3 worldPosition, float radius = 0.75f)
    {
        float bestDistance = float.PositiveInfinity;
        Zone best = null;
        foreach (Zone zone in zones)
        {
            foreach (Vector2Int cell in zone.Cells)
            {
                float distance = Vector2.Distance(worldPosition, zone.CellToWorld(cell));
                if (distance <= radius && distance < bestDistance)
                {
                    best = zone;
                    bestDistance = distance;
                }
            }
        }

        return best;
    }

    public bool TryGetRegionAtWorld(Vector3 worldPosition, out NavigationRegion region)
    {
        NavigationRegion[] regions = FindObjectsOfType<NavigationRegion>();
        foreach (NavigationRegion candidate in regions)
        {
            if (!candidate.IsReady)
            {
                continue;
            }

            if (!candidate.ContainsWorldPoint(worldPosition))
            {
                continue;
            }

            region = candidate;
            return true;
        }

        region = null;
        return false;
    }

    public Vector2Int WorldToCell(NavigationRegion region, Vector3 worldPosition)
    {
        Transform root = region ? region.CoordinateRoot : null;
        Vector3 local = root ? root.InverseTransformPoint(worldPosition) : worldPosition;
        return new Vector2Int(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y));
    }

    public bool CanUseCell(NavigationRegion region, Vector2Int cell)
    {
        if (region == null || !region.IsReady || region.Grid == null)
        {
            return false;
        }

        if (!region.Grid.ToPathNode(cell, out PathNode node) || node == null)
        {
            return false;
        }

        return !node.isCollision;
    }

    public void RenderPreview(NavigationRegion region, IReadOnlyCollection<Vector2Int> cells,
        Color color)
    {
        Zone previewZone = new Zone();
        previewZone.Initialize(-1, ZoneKind.Stockpile, region, cells, new ZoneSettings());
        zoneRenderer.RenderPreview(cells, previewZone, color);
    }

    public void ClearPreview()
    {
        zoneRenderer.ClearPreview();
    }

    private List<Vector2Int> FilterValidCells(NavigationRegion region,
        IEnumerable<Vector2Int> requestedCells)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        HashSet<Vector2Int> unique = new HashSet<Vector2Int>();
        foreach (Vector2Int cell in requestedCells)
        {
            if (unique.Add(cell) && CanUseCell(region, cell) && !IsForbidden(cell, region))
            {
                result.Add(cell);
            }
        }

        return result;
    }

    private bool IsForbidden(Vector2Int cell, NavigationRegion region)
    {
        foreach (Zone zone in zones)
        {
            if (zone.region == region && zone.kind == ZoneKind.Stockpile && zone.Contains(cell))
            {
                continue;
            }
        }

        return false;
    }

    private Zone FindOverlapping(ZoneKind kind, NavigationRegion region,
        IEnumerable<Vector2Int> cells)
    {
        HashSet<Vector2Int> requested = new HashSet<Vector2Int>(cells);
        foreach (Zone zone in zones)
        {
            if (zone.kind != kind || zone.region != region)
            {
                continue;
            }

            foreach (Vector2Int cell in zone.Cells)
            {
                if (requested.Contains(cell))
                {
                    return zone;
                }
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (runtimeStockpile)
        {
            Destroy(runtimeStockpile);
        }
    }
}
