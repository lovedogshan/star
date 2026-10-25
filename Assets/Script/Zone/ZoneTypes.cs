using System;
using System.Collections.Generic;
using UnityEngine;

public enum ZoneKind
{
    Stockpile
}

public enum ZoneCategory
{
    Storage
}

public enum ZonePlacementMode
{
    Rectangle,
    Brush
}

[Serializable]
public sealed class ZoneFilter
{
    public List<string> allowedCategories = new List<string>();
    public List<string> allowedItemIds = new List<string>();
    public List<string> forbiddenItemIds = new List<string>();
    public int minimumQuality;
    public int maximumQuality = int.MaxValue;
    public float minimumDurability;
    public bool allowRotten;
    public bool allowDangerous;

    public bool Accepts(string itemId, string category)
    {
        if (forbiddenItemIds.Contains(itemId))
        {
            return false;
        }

        bool hasItemWhitelist = allowedItemIds.Count > 0;
        bool hasCategoryWhitelist = allowedCategories.Count > 0;
        if (hasItemWhitelist && allowedItemIds.Contains(itemId))
        {
            return true;
        }

        if (hasCategoryWhitelist && allowedCategories.Contains(category))
        {
            return true;
        }

        return !hasItemWhitelist && !hasCategoryWhitelist;
    }
}

[Serializable]
public sealed class ZoneSettings
{
    [Range(0, 3)]
    public int priority = 1;
    public int capacity = -1;
    public bool enabled = true;
    public bool allowAutomaticTasks = true;
    public bool allowPlayerTasks = true;
    public bool mergeSameType = true;
    public ZoneFilter filter = new ZoneFilter();

    public ZoneSettings Clone()
    {
        return new ZoneSettings
        {
            priority = priority,
            capacity = capacity,
            enabled = enabled,
            allowAutomaticTasks = allowAutomaticTasks,
            allowPlayerTasks = allowPlayerTasks,
            mergeSameType = mergeSameType,
            filter = new ZoneFilter
            {
                allowedCategories = new List<string>(filter.allowedCategories),
                allowedItemIds = new List<string>(filter.allowedItemIds),
                forbiddenItemIds = new List<string>(filter.forbiddenItemIds),
                minimumQuality = filter.minimumQuality,
                maximumQuality = filter.maximumQuality,
                minimumDurability = filter.minimumDurability,
                allowRotten = filter.allowRotten,
                allowDangerous = filter.allowDangerous
            }
        };
    }
}

[CreateAssetMenu(fileName = "ZoneType", menuName = "Star/Zone Type")]
public sealed class ZoneTypeDefinition : ScriptableObject
{
    public string typeId = "stockpile";
    public ZoneKind kind = ZoneKind.Stockpile;
    public ZoneCategory category = ZoneCategory.Storage;
    public string displayName = "物资存放区";
    [TextArea]
    public string description = "允许 NPC 将物资搬运并集中存放。";
    public Texture2D icon;
    public KeyCode shortcut = KeyCode.Alpha1;
    public int displayOrder;
    public bool requiresFloor = true;
    public bool supportsBrush = true;
    public bool supportsRectangle = true;
    public bool supportsFilters = true;
    public Color previewColor = new Color(0.2f, 0.65f, 0.95f, 0.28f);

    public ZoneSettings CreateDefaultSettings()
    {
        return new ZoneSettings
        {
            priority = 1,
            capacity = -1,
            enabled = true,
            allowAutomaticTasks = true,
            allowPlayerTasks = true,
            mergeSameType = true
        };
    }

    public static ZoneTypeDefinition CreateRuntimeStockpile()
    {
        ZoneTypeDefinition definition = CreateInstance<ZoneTypeDefinition>();
        definition.name = "RuntimeStockpileZoneType";
        definition.hideFlags = HideFlags.HideAndDontSave;
        return definition;
    }
}

[Serializable]
public sealed class Zone
{
    public int id;
    public int regionId;
    public ZoneKind kind;
    public ZoneSettings settings;
    public List<Vector2Int> serializedCells = new List<Vector2Int>();

    [NonSerialized]
    private HashSet<Vector2Int> cellSet;

    [NonSerialized]
    public NavigationRegion region;

    public IReadOnlyCollection<Vector2Int> Cells
    {
        get
        {
            EnsureCellSet();
            return cellSet;
        }
    }

    public void Initialize(int zoneId, ZoneKind zoneKind, NavigationRegion zoneRegion,
        IEnumerable<Vector2Int> cells, ZoneSettings zoneSettings)
    {
        id = zoneId;
        kind = zoneKind;
        region = zoneRegion;
        regionId = zoneRegion ? zoneRegion.RegionId : AITask.AnyRegionId;
        settings = zoneSettings ?? new ZoneSettings();
        serializedCells = new List<Vector2Int>(cells);
        cellSet = new HashSet<Vector2Int>(serializedCells);
    }

    public bool Contains(Vector2Int cell)
    {
        EnsureCellSet();
        return cellSet.Contains(cell);
    }

    public void AddCells(IEnumerable<Vector2Int> cells)
    {
        EnsureCellSet();
        foreach (Vector2Int cell in cells)
        {
            if (cellSet.Add(cell))
            {
                serializedCells.Add(cell);
            }
        }
    }

    public bool RemoveCell(Vector2Int cell)
    {
        EnsureCellSet();
        if (!cellSet.Remove(cell))
        {
            return false;
        }

        serializedCells.Remove(cell);
        return true;
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        Transform root = region ? region.CoordinateRoot : null;
        return root ? root.TransformPoint(new Vector3(cell.x, cell.y, 0f)) : new Vector3(cell.x, cell.y, 0f);
    }

    private void EnsureCellSet()
    {
        if (cellSet == null)
        {
            cellSet = new HashSet<Vector2Int>(serializedCells ?? new List<Vector2Int>());
        }
    }
}
