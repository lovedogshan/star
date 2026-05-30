using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipWeapon : item
{
    // Start is called before the first frame update
    public int size;
    public ShipWeaponType Weapon;
    public GameObject weaponprefeb;

    public enum ShipWeaponType
    {
        Gun,
        Turret
    }
}
