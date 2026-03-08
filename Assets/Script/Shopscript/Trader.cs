using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trader
{
    public int sellprice;
    public Shop shop;
    public List<item> goodslist = new List<item>();
    public int money;
}

public class BuyItem : Trader
{

    public BuyItem(item item)
    {
        sellprice = item.price;
        Inventory inventory = new Inventory();
        goodslist = shop.itemList;
        if ( money <= sellprice )
        {
            money -= sellprice;
            inventory.itemList.Add(item);
            goodslist.Remove(item);
        }
        
    }
}

class SellItem  : Trader
{   
    public SellItem(item item)
    {
        sellprice = item.price;
        Inventory inventory = new Inventory();
        inventory.itemList.Remove(item);
        money += item.price;
    }
    
}
