using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class FindPath 
{


    public PathGrid pathGrid;

    private List<PathNode> openNodes = new List<PathNode>();
    private List<PathNode> closeNodes = new List<PathNode>();

    private Stack<PathNode> stackPath = new Stack<PathNode>();




    public const int StackPathCountMax = 500;

    public bool FindingPath(Vector2Int start, Vector2Int end, out List<Vector2Int>getPath, int spaceRange)
    {
        getPath = new List<Vector2Int>();

        pathGrid.ToPathNode(start, out PathNode startNode);
        pathGrid.ToPathNode(end, out PathNode endNode);
        if (startNode == null)
        {
            return false;
        }
        if (endNode == null)
        {
            endNode = new PathNode
            {
                insidePosition = end + pathGrid.centerPos
            };
        }
        openNodes.Clear();
        closeNodes.Clear();
        openNodes.Add(startNode);
        startNode.Parent = null;
        while (openNodes.Count > 0)
        {
            var currentNode = openNodes[0];
            foreach (var node in openNodes)
            {
                if(node.PericeSum < currentNode.PericeSum || (node.PericeSum == currentNode.PericeSum && node.perice.y < currentNode.perice.y))
                {
                    currentNode = node;
                }
            }
            openNodes.Remove(currentNode);
            closeNodes.Add(currentNode);
            if (Vector2.Distance(pathGrid.PositionFromNode(currentNode),end) <= spaceRange)
            {
                GenerateStackPath(startNode, currentNode);
                foreach (var node in stackPath)
                {
                    getPath.Add(pathGrid.PositionFromNode(node));
                }
                return true;
            }
            foreach (var node in pathGrid.GetNeighor(currentNode))
            {
                if (node.isCollision || closeNodes.Contains(node))
                {
                    continue;
                }
                var periceX = currentNode.perice.x + GetDistanceNodes(currentNode, node);

                if(periceX < node.perice.x || !openNodes.Contains(node))
                {
                    node.perice = new Vector2Int(periceX, GetDistanceNodes(node, endNode));
                    node.Parent = currentNode;
                    if(!openNodes.Contains(node))
                    {
                        openNodes.Add(node);
                    }
                }
            }

        }
        return false;
    }

    public FindPathResult findPathResult = new FindPathResult();

    public class FindPathResult
    {
        public bool isResult;
        public bool isFinding;

        public List<Vector2Int> getPath = new List<Vector2Int>();
    }






    private void GenerateStackPath(PathNode startNode, PathNode endNode)
    {
        try
        {
            stackPath.Clear();
            var node = endNode;
            while (node?.Parent != startNode && node.Parent != null && stackPath.Count < StackPathCountMax)
            {
                stackPath.Push(node);
                node = node.Parent;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }

    }

    private int GetDistanceNodes(PathNode node1, PathNode node2)
    {
        int deltaX = Mathf.Abs(node1.insidePosition.x - node2.insidePosition.x);
        int deltaY = Mathf.Abs(node1.insidePosition.y - node2.insidePosition.y);
        if (deltaX > deltaY)
        {
            return deltaY * 14 + 10 * (deltaX - deltaY);
        }
        else
        {
            return deltaX * 14 + 10 * (deltaY - deltaX);
        }
    }

}
