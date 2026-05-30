using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 增加对 TextMeshPro 的支持

public class Transaction : MonoBehaviour
{
    [Header("=== 交易 UI 绑定 ===")]
    public string currentSelectedItemID; // 当前选中的物品 ID
    public int transactionQuantity = 1;  // 每次交易的数量，默认 1
    public TMP_Text quantityText;        // 改为 TMP_Text 以匹配你的 UI

    private void Start()
    {
        UpdateQuantityUI();
    }

    // 获取当前选中物品在商店或货舱中的最大可用数量
    private int GetMaxAvailableQuantity()
    {
        if (string.IsNullOrEmpty(currentSelectedItemID)) return 1;

        // 检查商店库存
        if (TraderManager.Instance.currentShop != null)
        {
            var shopEntry = TraderManager.Instance.currentShop.itemList.Find(e => e.itemData != null && e.itemData.itemID == currentSelectedItemID);
            if (shopEntry != null) return shopEntry.count;
        }

        // 检查玩家货舱库存
        if (TraderManager.Instance.playerCargo != null)
        {
            var cargoEntry = TraderManager.Instance.playerCargo.itemList.Find(e => e.itemData != null && e.itemData.itemID == currentSelectedItemID);
            if (cargoEntry != null) return cargoEntry.count;
        }

        return 99; // 默认上限
    }

    // 增加交易数量
    public void IncreaseQuantity()
    {
        int max = GetMaxAvailableQuantity();
        if (transactionQuantity < max)
        {
            transactionQuantity++;
            UpdateQuantityUI();
        }
        else
        {
            Debug.LogWarning($"[Transaction] 已达到最大库存限制: {max}");
        }
    }

    // 减少交易数量
    public void DecreaseQuantity()
    {
        if (transactionQuantity > 1)
        {
            transactionQuantity--;
            UpdateQuantityUI();
        }
    }

    // 刷新数量显示
    private void UpdateQuantityUI()
    {
        if (quantityText != null)
        {
            quantityText.text = transactionQuantity.ToString();
            // 强制开启物体，防止被意外隐藏
            quantityText.gameObject.SetActive(true); 
        }
        else
        {
            // 如果 TMP_Text 为空，尝试寻找传统的 Text 组件作为备选
            var legacyText = GetComponentInChildren<Text>();
            if (legacyText != null) legacyText.text = transactionQuantity.ToString();
        }
    }

    // 供其他 UI 脚本（如 Slot）调用，更新当前选中的物品
    public void SetSelectedItemID(string itemID)
    {
        currentSelectedItemID = itemID;
        transactionQuantity = 1; // 切换物品时，数量重置为 1
        UpdateQuantityUI();      // 立即刷新显示
        Debug.Log($"[Transaction] 当前选中物品：{itemID}，数量重置为 1");
    }

    public void OnBuyButtonClicked()
    {
        if (string.IsNullOrEmpty(currentSelectedItemID))
        {
            Debug.LogWarning("[Transaction] 未选择任何物品！");
            return;
        }

        TransactionResult result = TraderManager.Instance.BuyItem(currentSelectedItemID, transactionQuantity);
        
        if (result == TransactionResult.Success)
        {
            Debug.Log($"[Transaction] 成功购买 {transactionQuantity} 个 {currentSelectedItemID}！");
            transactionQuantity = 1; // 交易成功后重置
            UpdateQuantityUI();
        }
        else
        {
            Debug.LogError($"[Transaction] 购买失败：{result}");
        }
    }

    public void OnSellButtonClicked()
    {
        if (string.IsNullOrEmpty(currentSelectedItemID))
        {
            Debug.LogWarning("[Transaction] 未选择任何物品！");
            return;
        }

        TransactionResult result = TraderManager.Instance.SellItem(currentSelectedItemID, transactionQuantity);
        
        if (result == TransactionResult.Success)
        {
            Debug.Log($"[Transaction] 成功卖出 {transactionQuantity} 个 {currentSelectedItemID}！");
            transactionQuantity = 1; // 交易成功后重置
            UpdateQuantityUI();
        }
        else
        {
            Debug.LogError($"[Transaction] 卖出失败：{result}");
        }
    }
}
