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

    public void Reset(Vector2Int newSize, Vector2Int newCenterPos)
    {
        size = new Vector2Int(Mathf.Max(1, newSize.x), Mathf.Max(1, newSize.y));
        centerPos = newCenterPos;
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
        return CheckBordByLocal(check) && (grid[check.x, check.y]?.isCollision ?? false);
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
        return GetNeighor(originNode, null);
    }

    public List<PathNode> GetNeighor(PathNode originNode, ISet<Vector2Int> temporarilyBlocked)
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
                if (!CheckBord(relPosition - centerPos))
                {
                    continue;
                }

                PathNode neighbour = grid[relPosition.x, relPosition.y];
                if (neighbour == null || neighbour.isCollision ||
                    temporarilyBlocked != null && temporarilyBlocked.Contains(PositionFromNode(neighbour)))
                {
                    continue;
                }

                // Do not let a diagonal step pass through the corner of two blocked cells.
                if (i != 0 && j != 0 &&
                    (CheckCollision(PositionFromNode(originNode) + new Vector2Int(i, 0)) ||
                     CheckCollision(PositionFromNode(originNode) + new Vector2Int(0, j)) ||
                     IsTemporarilyBlocked(temporarilyBlocked, PositionFromNode(originNode) + new Vector2Int(i, 0)) ||
                     IsTemporarilyBlocked(temporarilyBlocked, PositionFromNode(originNode) + new Vector2Int(0, j))))
                {
                    continue;
                }

                getNodes.Add(neighbour);
        }
        }
        return getNodes;
    }

    private static bool IsTemporarilyBlocked(ISet<Vector2Int> temporarilyBlocked, Vector2Int position)
    {
        return temporarilyBlocked != null && temporarilyBlocked.Contains(position);
    }

    public void BakeColliderObstacles(Transform coordinateRoot, float agentRadius, LayerMask obstacleLayers)
    {
        if (!coordinateRoot || grid == null)
        {
            return;
        }

        float radius = Mathf.Max(0.01f, agentRadius);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                PathNode node = grid[x, y];
                if (node == null || node.isCollision)
                {
                    continue;
                }

                Vector2 localPosition = PositionFromNode(node);
                Vector2 worldPosition = coordinateRoot.TransformPoint(localPosition);
                Vector2 worldRadius = coordinateRoot.TransformVector(Vector2.one * radius);
                float worldSampleRadius = Mathf.Max(Mathf.Abs(worldRadius.x), Mathf.Abs(worldRadius.y));
                Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, worldSampleRadius, obstacleLayers);
                foreach (Collider2D hit in hits)
                {
                    if (!hit || hit.isTrigger || !hit.enabled || !hit.gameObject.activeInHierarchy ||
                        hit.GetComponentInParent<Person>() != null ||
                        !hit.transform.IsChildOf(coordinateRoot))
                    {
                        continue;
                    }

                    node.isCollision = true;
                    break;
                }
            }
        }
    }

    public IEnumerator BakeColliderObstaclesIncremental(
        Transform coordinateRoot,
        float agentRadius,
        LayerMask obstacleLayers,
        int physicsChecksPerFrame)
    {
        if (!coordinateRoot || grid == null)
        {
            yield break;
        }

        float radius = Mathf.Max(0.01f, agentRadius);
        int budget = Mathf.Max(1, physicsChecksPerFrame);
        int checks = 0;
        Collider2D[] hits = new Collider2D[32];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                PathNode node = grid[x, y];
                if (node == null || node.isCollision)
                {
                    continue;
                }

                Vector2 localPosition = PositionFromNode(node);
                Vector2 worldPosition = coordinateRoot.TransformPoint(localPosition);
                Vector2 worldRadius = coordinateRoot.TransformVector(Vector2.one * radius);
                float worldSampleRadius = Mathf.Max(Mathf.Abs(worldRadius.x), Mathf.Abs(worldRadius.y));
                int hitCount = Physics2D.OverlapCircleNonAlloc(worldPosition, worldSampleRadius, hits, obstacleLayers);
                for (int hitIndex = 0; hitIndex < hitCount; hitIndex++)
                {
                    Collider2D hit = hits[hitIndex];
                    if (!hit || hit.isTrigger || !hit.enabled || !hit.gameObject.activeInHierarchy ||
                        hit.GetComponentInParent<Person>() != null ||
                        !hit.transform.IsChildOf(coordinateRoot))
                    {
                        continue;
                    }

                    node.isCollision = true;
                    break;
                }

                checks++;
                if (checks >= budget)
                {
                    checks = 0;
                    yield return null;
                }
            }
        }
    }

    #region ��չ





    public void AddNode(Vector2Int relPosition, out PathNode newNode)
    {
        relPosition += centerPos;
        if (!CheckBordByLocal(relPosition))
        {
            newNode = null;
            return;
        }

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
            if (!CheckBordByLocal(setPos))
            {
                continue;
            }

            var setNode = grid[setPos.x, setPos.y];
            if (setNode != null && !setNode.isCollision)
            {
                needNodes.Add(setNode);
            }
            else if (setNode != null)
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
