using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Shiper : MonoBehaviour
{
    public PathGrid ShipGrid = new PathGrid();

    public List<Construction> ShipComponents { get; private set; } = new List<Construction>();

    public PathGrid Grid => ShipGrid;

    public bool HasNavigableNodes
    {
        get
        {
            if (ShipGrid == null || ShipGrid.grid == null)
            {
                return false;
            }

            for (int x = 0; x < ShipGrid.grid.GetLength(0); x++)
            {
                for (int y = 0; y < ShipGrid.grid.GetLength(1); y++)
                {
                    if (ShipGrid.grid[x, y] != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    void Awake()
    {
        Cabin cabin = GetComponentInChildren<Cabin>(true);
        if (cabin)
        {
            ModifyCompents(cabin, true);
        }
    }

    public void ModifyCompents(Construction con, bool isAdd = true)
    {
        if (!con)
        {
            return;
        }

        if (isAdd)
        {
            if (!ShipComponents.Contains(con))
            {
                ShipComponents.Add(con);
            }
            if (con is Cabin)
            { 
                Cabin cabin = con as Cabin;
                Tilemap tilemap = cabin.Tilemap;
                if (!tilemap)
                {
                    return;
                }
                List<Vector2Int> usedTiles = new List<Vector2Int>();
               BoundsInt bounds = tilemap.cellBounds;
               foreach (Vector3Int tilePosition  in bounds.allPositionsWithin)
               {
                   if (tilemap.HasTile(tilePosition))
                   {
                       usedTiles.Add(new Vector2Int(tilePosition.x,tilePosition.y));
                   }
               }

               foreach (Vector2Int position in usedTiles)
               {
                   ShipGrid.AddNode(position,out _);
               }
            }
            else
            {
                ShipGrid.AddCollision(con);
            }
        }
        else
        {
            ShipComponents.Remove(con);
        }
    }

    public void EnsureFallbackGrid(Vector2Int minInclusive, Vector2Int maxInclusive)
    {
        if (HasNavigableNodes)
        {
            return;
        }

        int minX = Mathf.Min(minInclusive.x, maxInclusive.x);
        int maxX = Mathf.Max(minInclusive.x, maxInclusive.x);
        int minY = Mathf.Min(minInclusive.y, maxInclusive.y);
        int maxY = Mathf.Max(minInclusive.y, maxInclusive.y);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                if (ShipGrid.CheckBordByLocal(position + ShipGrid.centerPos) && !ShipGrid.ToPathNode(position, out _))
                {
                    ShipGrid.AddNode(position, out _);
                }
            }
        }
    }
}
