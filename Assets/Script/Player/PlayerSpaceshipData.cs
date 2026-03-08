using System.Collections.Generic;
using UnityEngine;

// 运行时玩家数据容器，可在Inspector中配置初始拥有飞船
[CreateAssetMenu(fileName = "PlayerShipData", menuName = "Spaceship/Player Ship Data")]
public class PlayerSpaceshipData : ScriptableObject
{
    // 存储玩家拥有的飞船ID（对应SpaceshipData的shipID）
    public List<string> ownedShipIDs=new List<string>();

    // 全局飞船库（拖拽所有飞船数据资产到这里）
    public List<SpaceshipData> allAvailableShips=new List<SpaceshipData>();

    /// <summary>
    /// 检查玩家是否拥有某艘飞船
    /// </summary>
    public bool HasShip(string shipID)
    {
        return ownedShipIDs.Contains(shipID);
    }

    /// <summary>
    /// 获取玩家拥有的所有飞船数据列表
    /// </summary>
    public List<SpaceshipData> GetOwnedShips()
    {
        List<SpaceshipData> ownedShips=new List<SpaceshipData>();
        foreach (var ship in allAvailableShips)
        {
            if (HasShip(ship.shipID))
            {
                ownedShips.Add(ship);
            }
        }
        return ownedShips;
    }

    /// <summary>
    /// 玩家获得新飞船（用于购买/解锁场景）
    /// </summary>
    public bool AddShip(string shipID)
    {
        if (!HasShip(shipID))
        {
            ownedShipIDs.Add(shipID);
            SaveData(); // 新增后立即保存
            return true;
        }
        return false; // 已拥有，添加失败
    }

    // 持久化相关（PlayerPrefs，适合简单数据，复杂项目可用Json+文件存储）
    private const string PREF_KEY = "OwnedShips";

    /// <summary>
    /// 保存数据到本地
    /// </summary>
    public void SaveData()
    {
        string json=JsonUtility.ToJson(new ShipListWrapper { shipIDs=ownedShipIDs });
        PlayerPrefs.SetString(PREF_KEY, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 从本地加载数据
    /// </summary>
    public void LoadData()
    {
        if (PlayerPrefs.HasKey(PREF_KEY))
        {
            string json=PlayerPrefs.GetString(PREF_KEY);
            ShipListWrapper wrapper=JsonUtility.FromJson<ShipListWrapper>(json);
            ownedShipIDs=wrapper.shipIDs;
        }
    }

    // 辅助类：解决JsonUtility无法直接序列化List<string>的问题
    [System.Serializable]
    private class ShipListWrapper
    {
        public List<string> shipIDs;
    }
}