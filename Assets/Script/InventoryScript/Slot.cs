using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public int slotID;
    public item slotItem;
    public Image slotImage;
    public Text slotNum;
    public string slotInfo;

    public GameObject iteminslot;
    public void ItemOnClicked()
    {
        InventoryManage.UpdateItemInfo(slotInfo);
        FindObjectOfType<Transaction>()?.SetSelectedItemID(slotItem.itemID);
    }

    public void SetupSlot(item item)
    {
        if (item == null)
        {
            if (iteminslot != null) iteminslot.SetActive(false);
            return;
        }

        if (iteminslot != null) iteminslot.SetActive(true);
        if (slotImage != null) slotImage.sprite = item.itemImage;
        if (slotNum != null) slotNum.text = item.itemHeld.ToString();
        slotInfo = item.itemInfo;
        slotItem = item; 
    }
    
}
