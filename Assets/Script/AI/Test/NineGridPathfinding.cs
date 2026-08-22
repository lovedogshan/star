using System.Collections.Generic;
using UnityEngine;

// 九宫格寻路系统完整示例
public class NineGridPathfinding : MonoBehaviour
{

    public PathGrid pathGrid;
 

    void Awake()
    {
        InitializeGrid();

    }

    // 1. 初始化3x3寻路网格
    void InitializeGrid()
    {
        pathGrid = new PathGrid(new Vector2Int(3, 3));
        pathGrid.centerPos = new Vector2Int(1, 1); // 中心偏移

        // 填充所有节点（相对坐标范围：(-1,-1) 到 (1,1)）
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int relPos = new Vector2Int(x, y);
                pathGrid.AddNode(relPos, out PathNode node);

                // 设置中心点为障碍物
                if (x == 0 && y == 0)
                {
                    node.isCollision = true;
                }
            }
        }
    }


}

