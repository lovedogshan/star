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
    }

    public void SetupSlot(item item)
    {
        if (item == null)
        {
            iteminslot.SetActive(false);
            return;
        }

        slotImage.sprite = item.itemImage;
        slotNum.text = item.itemHeld.ToString();
        slotInfo = item.itemInfo;
    }
}
