using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader
{
    public int sellprice;
    public Shop shop;
    public List<InventoryEntry> goodslist = new List<InventoryEntry>();
    public int money;
}

public class BuyItem : Trader
{

    public BuyItem(item item)
    {
        sellprice = item.price;
        Inventory inventory = ScriptableObject.CreateInstance<Inventory>();
        goodslist = shop.itemList;
        if ( money <= sellprice )
        {
            money -= sellprice;
            
            InventoryEntry entry = inventory.itemList.Find(e => e.itemData == item);
            if (entry == null) inventory.itemList.Add(new InventoryEntry { itemData = item, count = 1 });
            else entry.count++;

            InventoryEntry shopEntry = goodslist.Find(e => e.itemData == item);
            if (shopEntry != null)
            {
                shopEntry.count--;
                if (shopEntry.count <= 0) goodslist.Remove(shopEntry);
            }
        }
        
    }
}

class SellItem  : Trader
{   
    public SellItem(item item)
    {
        sellprice = item.price;
        Inventory inventory = ScriptableObject.CreateInstance<Inventory>();
        
        InventoryEntry entry = inventory.itemList.Find(e => e.itemData == item);
        if (entry != null)
        {
            entry.count--;
            if (entry.count <= 0) inventory.itemList.Remove(entry);
        }
        
        money += item.price;
    }
    
}
