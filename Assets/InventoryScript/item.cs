using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName= "New Item",menuName ="Inventory/New Item")]


public class item : ScriptableObject
{
    public string itemName;
    public Sprite itemImage;
    public int itemHeld;
    public int price;
    [TextArea]
    public string itemInfo;
    public ItemType itemType;
    public string itemID;
    public float basePrice;  
    public bool isSellable;   
    public bool isBuyable; 

    public enum ItemType
    {
        Goods,
        ShipAccessory,
        ShipWeapon,
    }

}
