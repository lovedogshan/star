using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManage : MonoBehaviour
{
    static InventoryManage instance;
    public Inventory Cargo;
    public GameObject slotGrid;
    //public Slot slotprefab;
    public GameObject emptyslot;
    public  Image itemImage;
    public Text itemInfromation;
    public List<GameObject> slots = new List<GameObject>();

    private void Awake()
    {
        if (instance != null)
            Destroy(this);
        instance = this;
    }
    private void OnEnable()
    {
        RefreshItem();
        instance.itemInfromation.text = "";
    }

    public static void UpdateItemimage(Image itemImage)
    {
        instance.itemImage = itemImage;
    }
    public static void UpdateItemInfo(string itemDescription)
    {
        instance.itemInfromation.text = itemDescription;
    }
    // public static void CreateNewItem(item item)
    // {
    // Slot newItem = Instantiate(instance.slotprefab, instance.slotGrid.transform.position, Quaternion.identity);
    //newItem.gameObject.transform.SetParent(instance.slotGrid.transform);
    //newItem.slotItem = item;
    //newItem.slotImage.sprite = item.itemImage;
    //newItem.slotNum.text = item.itemHeld.ToString();
    //}
    public static void RefreshItem()
    {
        for(int i=0; i<instance.slotGrid.transform.childCount;i++)
        {
            if (instance.slotGrid.transform.childCount == 0)
                break;
            Destroy(instance.slotGrid.transform.GetChild(i).gameObject);
        }

        for(int i=0;i < instance.Cargo.itemList.Count;i++)
        {
            //CreateNewItem(instance.Cargo.itemList[i]);
            instance.slots.Add(Instantiate(instance.emptyslot));
            instance.slots[i].transform.SetParent(instance.slotGrid.transform);
            instance.slots[i].GetComponent<Slot>().slotID = i;
            instance.slots[i].GetComponent<Slot>().SetupSlot(instance.Cargo.itemList[i]);
        }
    }

}
