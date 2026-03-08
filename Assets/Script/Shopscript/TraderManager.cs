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
    public TraderManager Instance{ get; private set; }
    
    private float playerCurrency = currency.Money ;

    public List<item> itemDatabase = new List<item>();

    private Dictionary<string, int> playerInventory = new Dictionary<string, int>();
    
    private Dictionary<string, int> npcInventory = new Dictionary<string, int>();
    
    public event Action<TransactionResult> OnTransactionCompleted;
  
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeNPCInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeNPCInventory()
    {
        npcInventory.Add("item_001", 10);
        npcInventory.Add("item_002", 5);
        // 添加更多初始物品...
    }
    
    // 购买物品（从NPC到玩家）
    public TransactionResult BuyItem(string itemID, int quantity)
    {
        
        // 获取物品数据
        item item = GetItemData(itemID);
        if (item == null || !item.isBuyable) 
            return TransactionResult.ItemNotAvailable;

        // 计算总价
        float totalCost = item.basePrice * quantity;

        // 检查玩家能否购买
        if (playerCurrency < totalCost)
            return TransactionResult.NotEnoughCurrency;

        // 检查NPC是否有足够库存
        if (!npcInventory.ContainsKey(itemID) || npcInventory[itemID] < quantity)
            return TransactionResult.ItemNotAvailable;

        // 执行交易
        playerCurrency -= totalCost;
        AddToPlayerInventory(itemID, quantity);
        npcInventory[itemID] -= quantity;

        OnTransactionCompleted?.Invoke(TransactionResult.Success);
        return TransactionResult.Success;
    }
    
    public TransactionResult SellItem(string itemID, int quantity)
    {
        item item = GetItemData(itemID);
        if (item == null || !item.isSellable) 
            return TransactionResult.ItemNotAvailable;

        // 检查玩家是否有足够物品
        if (!playerInventory.ContainsKey(itemID) || playerInventory[itemID] < quantity)
            return TransactionResult.ItemNotAvailable;

        // 计算收益
        float totalGain = item.basePrice * quantity;

        // 执行交易
        playerCurrency += totalGain;
        RemoveFromPlayerInventory(itemID, quantity);
        npcInventory[itemID] += quantity;

        OnTransactionCompleted?.Invoke(TransactionResult.Success);
        return TransactionResult.Success;
    }
    
    // 添加物品到玩家库存
    private void AddToPlayerInventory(string itemID, int quantity)
    {
        if (playerInventory.ContainsKey(itemID))
        {
            playerInventory[itemID] += quantity;
        }
        else
        {
            playerInventory.Add(itemID, quantity);
        }
    }

    // 从玩家库存移除物品
    private void RemoveFromPlayerInventory(string itemID, int quantity)
    {
        if (playerInventory.ContainsKey(itemID))
        {
            playerInventory[itemID] -= quantity;
            if (playerInventory[itemID] <= 0)
            {
                playerInventory.Remove(itemID);
            }
        }
    }

    // 获取物品数据
    public item GetItemData(string itemID)
    {
        return itemDatabase.Find(item => item.itemID == itemID);
    }

    // 获取玩家货币
    public float GetPlayerCurrency()
    {
        return playerCurrency;
    }

    // 获取玩家库存
    public Dictionary<string, int> GetPlayerInventory()
    {
        return new Dictionary<string, int>(playerInventory);
    }
}
