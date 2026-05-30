using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryEntry
{
    public item itemData;    // 物品基础数据 (图标、价格等)
    public int count;        // 该位置持有的数量 (库存)
}

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory/New Inventory")]
public class Inventory : ScriptableObject
{
    public List<InventoryEntry> itemList = new List<InventoryEntry>();
}
