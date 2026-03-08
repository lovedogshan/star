using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equim : item
{
    ItemType Accessory;
    int TypeNumber;
    void equiment()
    {
        Dictionary<ItemType, int> parts = new Dictionary<ItemType, int>();

       switch(TypeNumber)
        {
            case 0:
                Accessory = ItemType.ShipAccessory;
                break;
            case 1:
                Accessory = ItemType.ShipWeapon;
                break;
        }
        parts.Add(Accessory, TypeNumber);
    }
}
