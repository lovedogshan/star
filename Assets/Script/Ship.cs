using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
   /* public GameObject[] GunSlot ;
    public GameObject[] Turretslot;
    public ShipWeapon Turret;
    public ShipWeapon Gun;*/
    public float Armor;
    public float Structure;
    [Header("挂载点信息")]
    public MountPointData[] mountPoints;
    public Vector2 previewLocalMin;
    public Vector2 previewLocalMax;

    // Start is called before the first frame update


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "shell")
        { 
            if(Armor > 0)
            {
                Armor -= collision.gameObject.GetComponent<bullet>().damage;
            }
            else
            {
                Structure -= collision.gameObject.GetComponent<bullet>().damage;
            }
            
        }
    }
 /*   void equiment(int i)
    {
        
        if(i >= 0 && i < Turretslot.Length)
        {
            GameObject instantiatedItem = Instantiate(Turret.weaponprefeb, Turretslot[i].transform.position, Turretslot[i].transform.rotation);
            instantiatedItem.transform.SetParent(Turretslot[i].transform); // ����Ϊ��۵��Ӷ��� 
        }

    }*/
}


[System.Serializable]
public class MountPointData
{
    public string pointName;         // 挂载点名称
    public Transform mountPointTransform;    // 相对飞船的位置
    public PartType pointType; // 挂载点大类
    public PartType[] compatibleParts; // 可装配的具体配件类型
    public PartSize maxSupportSize;  // 支持的最大配件尺寸
    public float maxWeaponSize;      // 扩展属性（武器最大尺寸）
    public Vector2 previewAnchorPos;
    

}