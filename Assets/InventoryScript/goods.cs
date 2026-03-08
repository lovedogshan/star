using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goods : MonoBehaviour
{
    public item thisItem;
    public Inventory playerInventory;
    public void AddNewItem()
    {
        if(!playerInventory.itemList.Contains(thisItem))
        {
            playerInventory.itemList.Add(thisItem);

        }
        else
        {
            thisItem.itemHeld += 1;
        }
    }
  
}
