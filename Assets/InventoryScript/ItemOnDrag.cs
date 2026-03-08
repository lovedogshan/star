using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemOnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent;
    public Inventory cargo;
    private int currentItemid;//当前物品ID
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        currentItemid = originalParent.GetComponent<Slot>().slotID;
        transform.SetParent(transform.parent.parent);
        transform.position = eventData.position;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
       if(eventData.pointerCurrentRaycast.gameObject != null)
      {
            if (eventData.pointerCurrentRaycast.gameObject.name == "Item Image")//判单下面物体名字是：Item Image那么互换位置
            {
                transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform.parent.parent);
                transform.position = eventData.pointerCurrentRaycast.gameObject.transform.parent.parent.position;
                //itemList的物品存储位置改变
                var temp = cargo.itemList[currentItemid];
                cargo.itemList[currentItemid] = cargo.itemList[eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>().slotID];
                cargo.itemList[eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>().slotID] = temp;

                eventData.pointerCurrentRaycast.gameObject.transform.parent.position = originalParent.position;
                eventData.pointerCurrentRaycast.gameObject.transform.parent.SetParent(originalParent);
                GetComponent<CanvasGroup>().blocksRaycasts = true;//射线阻挡开启，不然无法再次选中移动的物品
                return;
            }
            if (eventData.pointerCurrentRaycast.gameObject.name == "slot(clone)")
            {
                //否则直接挂在检测到slot下面
                transform.SetParent(eventData.pointerCurrentRaycast.gameObject.transform);
                transform.position = eventData.pointerCurrentRaycast.gameObject.transform.position;
                //itemList的物品存储位置改变
                cargo.itemList[eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>().slotID] = cargo.itemList[currentItemid];
                if (eventData.pointerCurrentRaycast.gameObject.GetComponent<Slot>().slotID != currentItemid)
                    cargo.itemList[currentItemid] = null;
                GetComponent<CanvasGroup>().blocksRaycasts = true;
                return;
            }

            //其它任何位置都归位物品
            transform.SetParent(originalParent);
            transform.position = originalParent.position;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
       }
        
    }
}
