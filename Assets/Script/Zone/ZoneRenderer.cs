using System.Collections.Generic;
using UnityEngine;

public sealed class ZoneRenderer : MonoBehaviour
{
    private struct CellRectangle
    {
        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;
    }

    private readonly Dictionary<int, GameObject> zoneObjects = new Dictionary<int, GameObject>();
    private GameObject previewObject;
    private Material previewMaterial;

    private void Awake()
    {
        previewMaterial = CreateMaterial(new Color(0.2f, 0.65f, 0.95f, 0.34f));
    }

    public void Render(Zone zone, Color color)
    {
        if (zone == null)
        {
            return;
        }

        ClearZone(zone.id);
        GameObject zoneObject = CreateMeshObject("Zone_" + zone.id, zone.Cells, zone, color);
        zoneObjects[zone.id] = zoneObject;
    }

    public void RenderPreview(IReadOnlyCollection<Vector2Int> cells, Zone zone, Color color)
    {
        ClearPreview();
        if (zone == null || cells == null || cells.Count == 0)
        {
            return;
        }

        previewMaterial.color = color;
        previewObject = CreateMeshObject("ZonePreview", cells, zone, color);
    }

    public void ClearZone(int zoneId)
    {
        if (!zoneObjects.TryGetValue(zoneId, out GameObject zoneObject))
        {
            return;
        }

        if (zoneObject)
        {
            Destroy(zoneObject);
        }

        zoneObjects.Remove(zoneId);
    }

    public void ClearPreview()
    {
        if (previewObject)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    private GameObject CreateMeshObject(string objectName,
        IReadOnlyCollection<Vector2Int> cells, Zone zone, Color color)
    {
        GameObject result = new GameObject(objectName);
        result.transform.SetParent(transform, false);

        MeshFilter filter = result.AddComponent<MeshFilter>();
        MeshRenderer renderer = result.AddComponent<MeshRenderer>();
        renderer.material = objectName == "ZonePreview"
            ? previewMaterial
            : CreateMaterial(color);
        renderer.sortingOrder = 1;

        List<CellRectangle> rectangles = MergeCells(cells);
        Mesh mesh = new Mesh { name = objectName + "Mesh" };
        List<Vector3> vertices = new List<Vector3>(rectangles.Count * 4);
        List<int> triangles = new List<int>(rectangles.Count * 6);

        for (int rectangleIndex = 0; rectangleIndex < rectangles.Count; rectangleIndex++)
        {
            CellRectangle rectangle = rectangles[rectangleIndex];
            int index = vertices.Count;
            vertices.Add(ToMeshPoint(result.transform, zone, rectangle.MinX - 0.5f, rectangle.MinY - 0.5f));
            vertices.Add(ToMeshPoint(result.transform, zone, rectangle.MinX - 0.5f, rectangle.MaxY + 0.5f));
            vertices.Add(ToMeshPoint(result.transform, zone, rectangle.MaxX + 0.5f, rectangle.MaxY + 0.5f));
            vertices.Add(ToMeshPoint(result.transform, zone, rectangle.MaxX + 0.5f, rectangle.MinY - 0.5f));
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        filter.sharedMesh = mesh;
        return result;
    }

    private static List<CellRectangle> MergeCells(IReadOnlyCollection<Vector2Int> cells)
    {
        HashSet<Vector2Int> remaining = new HashSet<Vector2Int>(cells);
        List<CellRectangle> rectangles = new List<CellRectangle>();
        while (remaining.Count > 0)
        {
            Vector2Int start = default;
            foreach (Vector2Int cell in remaining)
            {
                start = cell;
                break;
            }

            int maxX = start.x;
            while (remaining.Contains(new Vector2Int(maxX + 1, start.y)))
            {
                maxX++;
            }

            int maxY = start.y;
            bool canExtend = true;
            while (canExtend)
            {
                int nextY = maxY + 1;
                for (int x = start.x; x <= maxX; x++)
                {
                    if (!remaining.Contains(new Vector2Int(x, nextY)))
                    {
                        canExtend = false;
                        break;
                    }
                }

                if (canExtend)
                {
                    maxY = nextY;
                }
            }

            for (int x = start.x; x <= maxX; x++)
            {
                for (int y = start.y; y <= maxY; y++)
                {
                    remaining.Remove(new Vector2Int(x, y));
                }
            }

            rectangles.Add(new CellRectangle
            {
                MinX = start.x,
                MinY = start.y,
                MaxX = maxX,
                MaxY = maxY
            });
        }

        return rectangles;
    }

    private static Vector3 ToMeshPoint(Transform meshTransform, Zone zone, float x, float y)
    {
        Transform coordinateRoot = zone.region ? zone.region.CoordinateRoot : null;
        Vector3 worldPoint = coordinateRoot
            ? coordinateRoot.TransformPoint(new Vector3(x, y, 0f))
            : new Vector3(x, y, 0f);
        worldPoint.z += 0.02f;
        return meshTransform.InverseTransformPoint(worldPoint);
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");
        Material material = new Material(shader) { color = color };
        return material;
    }

    private void OnDestroy()
    {
        foreach (GameObject zoneObject in zoneObjects.Values)
        {
            if (zoneObject)
            {
                Destroy(zoneObject);
            }
        }

        if (previewObject)
        {
            Destroy(previewObject);
        }

        if (previewMaterial)
        {
            Destroy(previewMaterial);
        }
    }
}
