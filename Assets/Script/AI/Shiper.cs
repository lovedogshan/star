using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Shiper : MonoBehaviour
{
    public PathGrid ShipGrid = new PathGrid();

    public List<Construction> ShipComponents { get; private set; } = new List<Construction>();

    void Awake()
    {
        ModifyCompents(GameObject.Find("Cabin").GetComponent<Cabin>(), true);
    }

    public void ModifyCompents(Construction con, bool isAdd = true)
    {
        if (isAdd)
        {
            ShipComponents.Add(con);
            if (con is Cabin)
            { 
                Cabin cabin = con as Cabin;
                Tilemap tilemap = cabin.Tilemap;
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
}
