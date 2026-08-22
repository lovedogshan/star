using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGrid :MonoBehaviour
{
    public PathGrid grid;


    public void Awake()
    {
        grid = GetPathGrid(Vector2Int.zero);
    }
    public static PathGrid GetPathGrid(Vector2Int targetInt)
    {
        var size = new Vector2Int(20, 20);
        var landGrid = new PathGrid()
        {
            centerPos = -targetInt + size / 2
        };

        for (int x = -10 ; x <=  10; x++)
        {
            for (int y = -10; y <= 10; y++)
            {
                landGrid.AddNode(new Vector2Int(x, y), out _);
            }
        }



        return landGrid;
    }

}
