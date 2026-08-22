using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode 
{
    public Vector2Int insidePosition;


    public Vector2Int perice;
    public bool isCollision;


    public int PericeSum => perice.x + perice.y;
    public PathNode Parent;
}


public interface IFindPath
{





    bool CheckBord(Vector2Int check);




    bool CheckCollision(Vector2Int check);



    bool ToPathNode(Vector2Int position, out PathNode pathNode);



    Vector2Int PositionFromNode(PathNode pathNode);
}
