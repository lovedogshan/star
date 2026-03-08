using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;
public class WeaponSlot :  Slot

{
    public int size;
    public ShipWeapon.ShipWeaponType type;
    public GameObject Weaponlist;
    public GameObject weapon;
    private bool isEquie;
    public  void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)//左键点击点开装备槽，点选装备
        {
            
        }
    }

    public bool IsRightWeapon(item item)//判断Item是否适合当前的槽
    {
        if ((item is ShipWeapon && ((ShipWeapon)item).Weapon == type) ||
               (item is ShipWeapon && ((ShipWeapon)item).size <=size))
        {
            return true;
        }
        return false;
    }
}
