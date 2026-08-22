using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnetDeck : MonoBehaviour
{
    private PathGrid landGrid;
    public static PathGrid GetPathGrid(Vector2Int targetInt, KeyValuePair<string, KeyValuePair<Constants.MapUnitType, List<MapUnit>>> area)
    {
        var size = new Vector2Int(100, 100);
        var landGrid = new PathGrid(size)
        {
            centerPos = -targetInt + size / 2
        };



        foreach (var mapUnit in area.Value.Value)
        {
            try
            {
                landGrid.AddNode(new Vector2Int((int)mapUnit.Position.x, (int)mapUnit.Position.y), out _);
            }
            catch (System.Exception)
            {
                Debug.Log("µ∫”ÏÕ¯∏Ò‘ΩΩÁ");
            }

        }

        return landGrid;
    }
}
