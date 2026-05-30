using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopListUI : MonoBehaviour
{
    public Transform content;          // ShopList/Content
    public GameObject itemEntryPrefab; // ShopItemEntry 预制
    public Image infoIcon;
    public TMP_Text infoName;
    public TMP_Text infoDesc;
    public TMP_Text infoPrice;
    public Transaction transaction;    // 拖入 Transaction 组件

    void OnEnable()
    {
        // 如果管理器还没准备好，延迟一小会儿再刷新
        if (TraderManager.Instance == null)
        {
            Invoke(nameof(Refresh), 0.05f);
        }
        else
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (content == null)
        {
            Debug.LogError($"[ShopListUI] 关键引用丢失：Content 尚未在 Inspector 中赋值！", this);
            return;
        }
        
        foreach (Transform t in content) Destroy(t.gameObject);

        if (TraderManager.Instance == null)
        {
            Debug.LogWarning("[ShopListUI] TraderManager.Instance 尚未准备好，尝试重新寻找...");
            return;
        }

        var shop = TraderManager.Instance.currentShop;
        if (shop == null)
        {
            Debug.LogWarning($"[ShopListUI] 无法显示：{TraderManager.Instance.gameObject.name} 上的 currentShop 为空！");
            return;
        }

        Debug.Log($"[ShopListUI] 正在刷新商店物品列表，物品数量: {shop.itemList.Count}");
        
        if (itemEntryPrefab == null)
        {
            Debug.LogError("[ShopListUI] 关键引用丢失：itemEntryPrefab 尚未赋值！");
            return;
        }

        foreach (var entry in shop.itemList)
        {
            if (entry.itemData == null) continue;
            var it = entry.itemData;

            var go = Instantiate(itemEntryPrefab, content);
            
            // 1. 处理通用 Slot 脚本逻辑 (显示图片和数量)
            Slot slotScript = go.GetComponent<Slot>();
            if (slotScript != null)
            {
                // 使用运行时副本显示数量
                item runtimeItem = ScriptableObject.Instantiate(it);
                runtimeItem.itemHeld = entry.count;
                slotScript.slotID = -1;
                slotScript.SetupSlot(runtimeItem);
            }

            // 2. 处理名字显示 (手动寻找 itemname 物体)
            Text nameTextObj = go.transform.Find("itemname")?.GetComponent<Text>();
            if (nameTextObj != null)
            {
                nameTextObj.text = it.itemName;
            }

            // 3. 处理专用 ShopItemEntry 脚本逻辑 (详情点击)
            var entryScript = go.GetComponent<ShopItemEntry>();
            if (entryScript != null)
            {
                entryScript.Setup(it, () => UpdateDetailPanel(it));
            }
            else
            {
                // 如果没有专用脚本，给整个格子加一个按钮功能
                Button btn = go.GetComponent<Button>();
                if (btn == null) btn = go.GetComponentInChildren<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => UpdateDetailPanel(it));
                }
            }
        }
    }

    private void UpdateDetailPanel(item it)
    {
        if (infoIcon != null) infoIcon.sprite = it.itemImage;
        if (infoName != null) infoName.text = it.itemName;
        if (infoDesc != null) infoDesc.text = it.itemInfo;
        if (infoPrice != null) infoPrice.text = $"价格：{it.basePrice}";
        if (transaction != null) transaction.SetSelectedItemID(it.itemID);
    }
}

public class ShopItemEntry : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button selectButton;

    public void Setup(item data, System.Action onSelect)
    {
        if (icon != null) icon.sprite = data.itemImage;
        if (nameText != null) nameText.text = data.itemName;
        if (priceText != null) priceText.text = $"{data.basePrice}";
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelect?.Invoke());
        }
    }
}