using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public sealed class ZonePlacementController : MonoBehaviour
{
    private Camera mainCamera;
    private UIDocument document;
    private ZoneTypeDefinition activeType;
    private NavigationRegion activeRegion;
    private readonly HashSet<Vector2Int> previewCells = new HashSet<Vector2Int>();
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();
    private Vector2Int startCell;
    private Vector3 startWorld;
    private bool isDragging;

    public bool IsActive { get; private set; }
    public ZonePlacementMode Mode { get; private set; } = ZonePlacementMode.Rectangle;

    private void Awake()
    {
        mainCamera = Camera.main;
        document = GetComponent<UIDocument>();
    }

    private void Update()
    {
        if (!IsActive || !ZoneManager.Instance)
        {
            return;
        }

        // UI blocks a new world click, but must not swallow mouse-up while a
        // rectangle is already being dragged across the top command overlay.
        if (!isDragging && IsPointerOverUi())
        {
            return;
        }

        mainCamera = mainCamera ? mainCamera : Camera.main;
        if (!mainCamera)
        {
            return;
        }

        Vector3 world = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            startWorld = world;
            if (!ZoneManager.Instance.TryGetRegionAtWorld(world, out activeRegion))
            {
                activeRegion = null;
            }

            startCell = ZoneManager.Instance.WorldToCell(activeRegion, world);
            isDragging = true;
            UpdatePreview(world);
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            UpdatePreview(world);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            UpdatePreview(world);
            Commit();
        }
    }

    public void Begin(ZoneTypeDefinition type)
    {
        activeType = type;
        activeRegion = null;
        Mode = type && type.supportsRectangle
            ? ZonePlacementMode.Rectangle
            : ZonePlacementMode.Brush;
        IsActive = activeType != null;
        isDragging = false;
        previewCells.Clear();
        ZoneManager.Instance?.ClearPreview();
    }

    public void Cancel()
    {
        IsActive = false;
        isDragging = false;
        previewCells.Clear();
        activeRegion = null;
        ZoneManager.Instance?.ClearPreview();
    }

    private void UpdatePreview(Vector3 world)
    {
        if (activeType == null)
        {
            return;
        }

        if (!activeRegion)
        {
            // Retry when dragging into a floor or when its incremental bake finishes.
            if (!ZoneManager.Instance.TryGetRegionAtWorld(world, out activeRegion))
            {
                previewCells.Clear();
                ZoneManager.Instance.ClearPreview();
                return;
            }

            startCell = ZoneManager.Instance.WorldToCell(activeRegion, startWorld);
        }

        Vector2Int currentCell = ZoneManager.Instance.WorldToCell(activeRegion, world);
        previewCells.Clear();

        if (Mode == ZonePlacementMode.Brush)
        {
            previewCells.Add(currentCell);
        }
        else
        {
            int minX = Mathf.Min(startCell.x, currentCell.x);
            int maxX = Mathf.Max(startCell.x, currentCell.x);
            int minY = Mathf.Min(startCell.y, currentCell.y);
            int maxY = Mathf.Max(startCell.y, currentCell.y);
            PathGrid grid = activeRegion.Grid;
            if (grid == null)
            {
                ZoneManager.Instance.ClearPreview();
                return;
            }

            minX = Mathf.Max(minX, -grid.centerPos.x);
            minY = Mathf.Max(minY, -grid.centerPos.y);
            maxX = Mathf.Min(maxX, grid.size.x - grid.centerPos.x - 1);
            maxY = Mathf.Min(maxY, grid.size.y - grid.centerPos.y - 1);
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    previewCells.Add(new Vector2Int(x, y));
                }
            }
        }

        HashSet<Vector2Int> validCells = new HashSet<Vector2Int>();
        foreach (Vector2Int cell in previewCells)
        {
            if (ZoneManager.Instance.CanUseCell(activeRegion, cell))
            {
                validCells.Add(cell);
            }
        }

        previewCells.Clear();
        previewCells.UnionWith(validCells);
        ZoneManager.Instance.RenderPreview(activeRegion, previewCells, activeType.previewColor);
    }

    private void Commit()
    {
        isDragging = false;
        if (previewCells.Count > 0)
        {
            if (CommandSystem.Instance)
            {
                CommandResult result = CommandSystem.Instance.Issue(
                    ZoneEditCommand.CreateCreate(activeType, activeRegion, previewCells));
                if (!result.Succeeded)
                {
                    Debug.LogWarning(result.Message, this);
                }
            }
            else
            {
                Debug.LogWarning("CommandSystem 尚未就绪，无法发布区域划定指令。", this);
            }
        }

        previewCells.Clear();
        ZoneManager.Instance.ClearPreview();
    }

    private bool IsPointerOverUi()
    {
        if (EventSystem.current)
        {
            uiHits.Clear();
            EventSystem.current.RaycastAll(
                new PointerEventData(EventSystem.current) { position = Input.mousePosition }, uiHits);
            foreach (RaycastResult hit in uiHits)
            {
                // Toolkit's full-screen panel and world physics hits are not UI controls.
                if (hit.module is GraphicRaycaster)
                {
                    return true;
                }
            }
        }

        if (document == null || document.rootVisualElement == null || document.rootVisualElement.panel == null)
        {
            return false;
        }

        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(
            document.rootVisualElement.panel,
            new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
        VisualElement picked = document.rootVisualElement.panel.Pick(panelPosition);
        while (picked != null)
        {
            if (picked is UnityEngine.UIElements.Button ||
                picked.ClassListContains("command-panel") ||
                picked.name == "zone-type-dropdown")
            {
                return true;
            }

            picked = picked.parent;
        }

        return false;
    }
}
