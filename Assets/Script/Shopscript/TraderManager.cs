using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TransactionResult
{
    Success,
    NotEnoughCurrency,
    NotEnoughInventorySpace,
    ItemNotAvailable,
    InvalidTransaction
}
public class TraderManager : MonoBehaviour
{
    public static TraderManager Instance{ get; private set; }
    
    [Header("=== 玩家货舱 ===")]
    public Inventory playerCargo; // 需要在 Inspector 中赋值玩家的 Cargo 数据

    [Header("=== 商店配置 ===")]
    public Shop currentShop;      // 当前正在交易的商店数据（可以在与不同NPC对话时切换）

    public List<item> itemDatabase = new List<item>();
    
    public event Action<TransactionResult> OnTransactionCompleted;
  
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log($"[TraderManager] 单例已初始化。当前对象: {gameObject.name}, 初始商店: {(currentShop != null ? currentShop.name : "无")}");
        }
        else
        {
            Debug.LogWarning($"[TraderManager] 发现多余的管理器，已销毁: {gameObject.name}");
            Destroy(gameObject);
        }
    }
    
    // 购买物品（从NPC到玩家）
    public TransactionResult BuyItem(string itemID, int quantity)
    {
        // 1. 获取物品数据
        item item = GetItemData(itemID);
        if (item == null)
        {
            Debug.LogError($"[TraderManager] 购买失败：在 ItemDatabase 中找不到 ID 为 '{itemID}' 的物品！请检查 TraderManager 物体上的列表。");
            return TransactionResult.ItemNotAvailable;
        }

        if (!item.isBuyable)
        {
            Debug.LogError($"[TraderManager] 购买失败：物品 '{item.itemName}' 的 'Is Buyable' 属性未勾选！");
            return TransactionResult.ItemNotAvailable;
        }

        // 2. 检查商店是否有足够库存
        if (currentShop == null)
        {
            Debug.LogError("[TraderManager] 购买失败：当前没有活跃的商店数据 (currentShop 为空)！");
            return TransactionResult.ItemNotAvailable;
        }

        InventoryEntry shopEntry = currentShop.itemList.Find(e => e.itemData != null && e.itemData.itemID == itemID);
        if (shopEntry == null)
        {
            Debug.LogError($"[TraderManager] 购买失败：当前商店 '{currentShop.name}' 并不出售 ID 为 '{itemID}' 的物品！");
            return TransactionResult.ItemNotAvailable;
        }

        if (shopEntry.count < quantity)
        {
            Debug.LogError($"[TraderManager] 购买失败：商店库存不足！剩余: {shopEntry.count}, 请求: {quantity}");
            return TransactionResult.ItemNotAvailable;
        }

        // 3. 计算总价
        float unitPrice = item.basePrice > 0 ? item.basePrice : item.price; // 优先用 basePrice，没有就用 price
        float totalCost = unitPrice * quantity;
        
        Debug.Log($"[TraderManager] 准备购买：物品={item.itemName}, 单价={unitPrice}, 数量={quantity}, 总价={totalCost}");
        Debug.Log($"[TraderManager] 扣费前玩家金钱: {currency.Money}");

        if (currency.Money < totalCost)
        {
            Debug.LogWarning($"[TraderManager] 购买失败：钱不够！需要: {totalCost}, 当前: {currency.Money}");
            return TransactionResult.NotEnoughCurrency;
        }

        // 4. 执行交易
        currency.Money -= totalCost;
        Debug.Log($"[TraderManager] 扣费后玩家金钱: {currency.Money}");
        shopEntry.count -= quantity;
        
        // 如果商店库存清空，则移除该项
        if (shopEntry.count <= 0)
        {
            currentShop.itemList.Remove(shopEntry);
        }

        AddToPlayerInventory(itemID, quantity);

        // 5. 刷新 UI
        if (InventoryManage.instance != null) InventoryManage.RefreshItem();
        
        // 强制刷新两个界面
        var shopDisplay = FindObjectOfType<ShopDisplayUI>();
        if (shopDisplay != null) shopDisplay.Refresh();
        
        var shopList = FindObjectOfType<ShopListUI>();
        if (shopList != null) shopList.Refresh();

        OnTransactionCompleted?.Invoke(TransactionResult.Success);
        return TransactionResult.Success;
    }
    
    public TransactionResult SellItem(string itemID, int quantity)
    {
        item item = GetItemData(itemID);
        if (item == null || !item.isSellable) 
            return TransactionResult.ItemNotAvailable;

        // 检查玩家是否有足够物品
        if (!PlayerHasItem(itemID, quantity))
            return TransactionResult.ItemNotAvailable;

        // 计算收益
        float totalGain = item.basePrice * quantity;

        // 执行交易
        currency.Money += totalGain;
        RemoveFromPlayerInventory(itemID, quantity);

        // 增加商店库存
        InventoryEntry shopEntry = currentShop.itemList.Find(e => e.itemData != null && e.itemData.itemID == itemID);
        if (shopEntry != null)
        {
            shopEntry.count += quantity;
        }
        else
        {
            // 如果商店原本没有这个物品，卖给它后创建一个新项
            currentShop.itemList.Add(new InventoryEntry { itemData = item, count = quantity });
        }

        // 刷新 UI
        if (InventoryManage.instance != null)
        {
            InventoryManage.RefreshItem();
        }
        
        var shopDisplay = FindObjectOfType<ShopDisplayUI>();
        if (shopDisplay != null) shopDisplay.Refresh();
        
        var shopList = FindObjectOfType<ShopListUI>();
        if (shopList != null) shopList.Refresh();

        OnTransactionCompleted?.Invoke(TransactionResult.Success);
        return TransactionResult.Success;
    }
    
    // 检查玩家是否有足够物品
    private bool PlayerHasItem(string itemID, int quantity)
    {
        if (playerCargo == null) return false;

        InventoryEntry entry = playerCargo.itemList.Find(e => e.itemData != null && e.itemData.itemID == itemID);
        return entry != null && entry.count >= quantity;
    }

    // 添加物品到玩家库存
    private void AddToPlayerInventory(string itemID, int quantity)
    {
        if (playerCargo == null) return;

        InventoryEntry entry = playerCargo.itemList.Find(e => e.itemData != null && e.itemData.itemID == itemID);
        if (entry != null)
        {
            entry.count += quantity;
        }
        else
        {
            item itemData = GetItemData(itemID);
            if (itemData != null)
            {
                playerCargo.itemList.Add(new InventoryEntry { itemData = itemData, count = quantity });
            }
        }
    }

    // 从玩家库存移除物品
    private void RemoveFromPlayerInventory(string itemID, int quantity)
    {
        if (playerCargo == null) return;

        InventoryEntry entry = playerCargo.itemList.Find(e => e.itemData != null && e.itemData.itemID == itemID);
        if (entry != null)
        {
            entry.count -= quantity;
            if (entry.count <= 0)
            {
                playerCargo.itemList.Remove(entry);
            }
        }
    }

    // 获取物品数据 (动态搜索模式：不再依赖手动填写的数据库)
    public item GetItemData(string itemID)
    {
        // 1. 先从当前商店里找
        if (currentShop != null)
        {
            var entry = currentShop.itemList.Find(e => e.itemData != null && e.itemData.itemID == itemID);
            if (entry != null) return entry.itemData;
        }

        // 2. 如果没找到，从玩家货舱里找
        if (playerCargo != null)
        {
            var entry = playerCargo.itemList.Find(e => e.itemData != null && e.itemData.itemID == itemID);
            if (entry != null) return entry.itemData;
        }

        // 3. 最后才从手动填写的数据库里找 (作为备选)
        return itemDatabase.Find(it => it != null && it.itemID == itemID);
    }

    // 获取玩家货币
    public float GetPlayerCurrency()
    {
        return currency.Money;
    }

    // 获取玩家库存
    public Dictionary<string, int> GetPlayerInventory()
    {
        Dictionary<string, int> inventoryDict = new Dictionary<string, int>();
        if (playerCargo != null)
        {
            foreach (var entry in playerCargo.itemList)
            {
                if (entry.itemData != null && entry.count > 0)
                {
                    inventoryDict[entry.itemData.itemID] = entry.count;
                }
            }
        }
        return inventoryDict;
    }
}
