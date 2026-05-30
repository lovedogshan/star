using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName= "New Item",menuName ="Shop/New Item")]
public class Shop : ScriptableObject
{
    // 使用 InventoryEntry 来存储商品及其对应的库存数量
    public List<InventoryEntry> itemList = new List<InventoryEntry>();
}
