using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static ShipWeapon;

public class ShipManager : ScriptableObject
{
    public string shipname;   
    public List<WeaponSlot> weaponlist;
    public float shipspeed;
    public float Shipvalue;

}
