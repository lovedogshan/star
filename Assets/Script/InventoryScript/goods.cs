using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goods : MonoBehaviour
{
    public item thisItem;
    public Inventory playerInventory;
    public void AddNewItem()
    {
        InventoryEntry entry = playerInventory.itemList.Find(e => e.itemData == thisItem);
        if (entry == null)
        {
            playerInventory.itemList.Add(new InventoryEntry { itemData = thisItem, count = 1 });
        }
        else
        {
            entry.count += 1;
        }
    }
  
}
