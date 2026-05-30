using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 挂载在商店界面的商品 Grid 上，负责获取并显示商店里的物品
/// </summary>
public class ShopDisplayUI : MonoBehaviour
{
    [Header("=== UI 配置 ===")]
    public GameObject slotGrid;      // 商店物品格子的容器 (Content)
    public GameObject slotPrefab;    // 物品格子预制体 (即 EmptySlot)
    public ScrollRect scrollRect;    // 对应的 ScrollRect 组件

    [Header("=== 详情面板引用 (可选) ===")]
    public Image infoIcon;
    public TMP_Text infoName;
    public TMP_Text infoDesc;
    public TMP_Text infoPrice;
    public Transaction transaction;

    private void OnEnable()
    {
        // 延迟刷新以确保 TraderManager 已初始化
        if (TraderManager.Instance == null)
        {
            Invoke(nameof(Refresh), 0.05f);
        }
        else
        {
            Refresh();
        }
    }

    /// <summary>
    /// 刷新商店物品列表
    /// </summary>
    public void Refresh()
    {
        // 1. 获取当前商店数据
        if (TraderManager.Instance == null || TraderManager.Instance.currentShop == null)
        {
            Debug.LogWarning("[ShopDisplayUI] 找不到 TraderManager 或未配置 Current Shop！");
            return;
        }

        if (slotGrid == null || slotPrefab == null) return;

        // 2. 彻底清空旧格子
        for (int i = slotGrid.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = slotGrid.transform.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }

        // 3. 遍历商店物品列表生成 UI
        List<InventoryEntry> shopEntries = TraderManager.Instance.currentShop.itemList;
        foreach (var entry in shopEntries)
        {
            if (entry.itemData == null) continue;

            GameObject newSlot = Instantiate(slotPrefab, slotGrid.transform);
            
            // 创建临时副本显示商店库存
            item runtimeItem = ScriptableObject.Instantiate(entry.itemData);
            runtimeItem.itemHeld = entry.count;

            Slot slotScript = newSlot.GetComponent<Slot>();
            
            if (slotScript != null)
            {
                slotScript.slotID = -1; 
                slotScript.SetupSlot(runtimeItem);
                
                // 绑定点击事件
                Button btn = newSlot.GetComponent<Button>();
                if (btn == null) btn = newSlot.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    btn.onClick.AddListener(() => OnItemClicked(entry.itemData));
                }
            }

            // 手动设置名字
            Text nameText = newSlot.transform.Find("itemname")?.GetComponent<Text>();
            if (nameText != null) nameText.text = entry.itemData.itemName;
        }

        // 5. 重置滚动位置
        if (scrollRect != null) StartCoroutine(ResetScroll());
    }

    private void OnItemClicked(item it)
    {
        // 更新详情面板
        if (infoIcon != null) infoIcon.sprite = it.itemImage;
        if (infoName != null) infoName.text = it.itemName;
        if (infoDesc != null) infoDesc.text = it.itemInfo;
        if (infoPrice != null) infoPrice.text = $"价格: {it.basePrice}";

        // 告诉交易系统选中了谁
        if (transaction != null)
        {
            transaction.SetSelectedItemID(it.itemID);
        }
    }

    private IEnumerator ResetScroll()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 1f;
    }
}
