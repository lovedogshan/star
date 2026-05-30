using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂载在任何需要显示货舱物品的 Grid 上（支持 ScrollView 滚动重置）
/// </summary>
public class CargoDisplayUI : MonoBehaviour
{
    [Header("=== UI 配置 ===")]
    public GameObject slotGrid;      // 物品格子的父容器 (Content)
    public GameObject slotPrefab;    // 物品格子预制体 (即 EmptySlot)
    
    [Header("=== 滚动配置 (可选) ===")]
    public ScrollRect scrollRect;    // 对应的 ScrollRect 组件，用于刷新后回到顶部

    private void OnEnable()
    {
        // 注册到全局管理器
        if (InventoryManage.instance != null)
        {
            InventoryManage.instance.RegisterUI(this);
        }
        Refresh();
    }

    private void OnDisable()
    {
        // 从全局管理器注销
        if (InventoryManage.instance != null)
        {
            InventoryManage.instance.UnregisterUI(this);
        }
    }

    /// <summary>
    /// 刷新当前 Grid 的显示
    /// </summary>
    public void Refresh()
    {
        if (slotGrid == null || slotPrefab == null || InventoryManage.instance == null || InventoryManage.instance.Cargo == null)
            return;

        // 1. 彻底清空旧格子 (增加安全性判断)
        for (int i = slotGrid.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = slotGrid.transform.GetChild(i).gameObject;
            // 只有是克隆出来的才销毁，或者全部销毁
            DestroyImmediate(child); 
        }

        // 确保下一帧之前子物体数为0
        if (slotGrid.transform.childCount > 0)
        {
            foreach (Transform t in slotGrid.transform) Destroy(t.gameObject);
        }

        // 2. 根据全局 Cargo 数据生成新格子
        List<InventoryEntry> entries = InventoryManage.instance.Cargo.itemList;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].itemData == null) continue;

            GameObject newSlot = Instantiate(slotPrefab, slotGrid.transform);
            
            // 技巧：创建一个临时 item 副本，设置它的 itemHeld 为仓库里的 count
            // 这样不需要改动 Slot 脚本就能显示正确数量
            item runtimeItem = ScriptableObject.Instantiate(entries[i].itemData);
            runtimeItem.itemHeld = entries[i].count;

            Slot slotScript = newSlot.GetComponent<Slot>();
            if (slotScript != null)
            {
                slotScript.slotID = i;
                slotScript.SetupSlot(runtimeItem);
            }

            // 手动设置名字（因为 Slot 脚本没引用它）
            Text nameText = newSlot.transform.Find("itemname")?.GetComponent<Text>();
            if (nameText == null) nameText = newSlot.GetComponentInChildren<Text>(); // 兼容性查找
            if (nameText != null)
            {
                nameText.text = entries[i].itemData.itemName;
            }
        }

        // 3. 滚动位置重置（可选）
        if (scrollRect != null)
        {
            // 延迟一帧重置，确保 Layout 已经更新
            StartCoroutine(ResetScrollPosition());
        }
    }

    private IEnumerator ResetScrollPosition()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f; // 回到顶部
        }
    }
}
