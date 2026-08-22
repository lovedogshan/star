using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;




public class MapUnit
{
    public Vector3 Position { get; private set; }
    public MapUnitType MapUnitType { get; set; }



    public int Visibility { get; set; } = 0;
    public MapUnit(Vector3 position, MapUnitType mapUnitType, int visibility = 0)
    {
        Position = position;
        MapUnitType = mapUnitType;
        Visibility = visibility;
    }
    [System.Obsolete]
    public void SetMapUnitType(MapUnitType mapUnitType)
    {
        MapUnitType = mapUnitType;
    }
}

