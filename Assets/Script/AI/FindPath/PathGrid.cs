using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGrid : IFindPath
{
    public Vector2Int size = new Vector2Int( 50 , 50 ) ;
    public Vector2Int centerPos = new Vector2Int( 25 , 25 ) ;
    public PathNode[,] grid;

    public PathGrid(Vector2Int size)
    {
        this.size = size;
        centerPos /= 2;
        grid = new PathNode[size.x, size.y];
    }
    public PathGrid()
    {
        grid = new PathNode[size.x, size.y];
    }

    public bool CheckBord(Vector2Int check)
    {
        check += centerPos;
        return CheckBordByLocal(check) && grid[check.x, check.y] != null;
    }


    public bool CheckBordByLocal(Vector2Int check)
    {
        return check.x >= 0 && check.x < size.x && check.y >= 0 && check.y < size.y;
    }



    public bool CheckCollision(Vector2Int check)
    {
        check += centerPos;
        return grid[check.x, check.y]?.isCollision ?? false;
    }

    public Vector2Int PositionFromNode(PathNode pathNode)
    {
        return pathNode.insidePosition - centerPos;
    }

    public bool ToPathNode(Vector2Int position, out PathNode pathNode)
    {
        var point = position + centerPos;
        pathNode = CheckBordByLocal(point) ? grid[point.x, point.y] : null;
        return pathNode != null;
    }

    public List<PathNode> GetNeighor(PathNode originNode)
    {
        var getNodes = new List<PathNode>();
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }
                Vector2Int relPosition = originNode.insidePosition + new Vector2Int(i, j);
                if (CheckBord(relPosition - centerPos))
                {
                    getNodes.Add(grid[relPosition.x, relPosition.y]);
                }
            }
        }
        return getNodes;
    }

    #region ��չ





    public void AddNode(Vector2Int relPosition, out PathNode newNode)
    {
        relPosition += centerPos;
        newNode = new PathNode { insidePosition = relPosition };
        grid[relPosition.x, relPosition.y] = newNode;
    }

    public void RemoveNode(Vector2Int relPosition)
    {
        relPosition += centerPos;
        grid[relPosition.x, relPosition.y] = null;
    }






  public bool AddCollision(Construction con)
    {
        if (!con.IsHinder)
        {
            return false;
        }
        var needNodes = new List<PathNode>();
        foreach (var item in con.Config.collisions)
        {
            Vector2Int setPos = new Vector2Int((int)con.transform.localPosition.x, (int)con.transform.localPosition.y) + item + centerPos;
            var setNode = grid[setPos.x, setPos.y];
            if (!setNode?.isCollision ?? false)
            {
                needNodes.Add(setNode);
            }
            else
                return false;
        }
        needNodes.ForEach(x => x.isCollision = true);
        return true;
    }

    public void RemoveCollision(Construction con)
    {
        if (!con.IsHinder)
        {
            return;
        }
        foreach (var item in con.Config.collisions)
        {
            Vector2Int setPos = new Vector2Int((int)con.transform.localPosition.x, (int)con.transform.localPosition.y) + item + centerPos;
            var setNode = grid[setPos.x, setPos.y];
            if (setNode != null)
            {
                setNode.isCollision = false;
            }
        }
    }
    #endregion

}
