using UnityEngine;
using UnityEngine.EventSystems; // 必须引入

public class ShopNPC : MonoBehaviour, IPointerClickHandler // 实现接口
{
    [Header("=== 商店配置 ===")]
    public Shop myShopData;          // 这个 NPC 卖的东西（拖入对应的 Shop 资源）
    public GameObject shopUIPanel;   // 商店 UI 的总面板

    /// <summary>
    /// 当玩家与该 NPC 交互时调用（例如通过点击或按 F 键）
    /// </summary>
    public void OpenShop()
    {
        Debug.Log($"[ShopNPC] 尝试打开商店: {gameObject.name}");
        if (TraderManager.Instance == null)
        {
            Debug.LogError("[ShopNPC] 错误：TraderManager.Instance 为空！请确保场景中有 TraderManager 物体。");
            return;
        }

        if (myShopData == null)
        {
            Debug.LogError("[ShopNPC] 错误：My Shop Data 未赋值！请拖入 Shop 资源。");
            return;
        }

        // 1. 【核心动作】告诉管家，现在的商店是这一个！
        TraderManager.Instance.currentShop = myShopData;
        Debug.Log($"[ShopNPC] 已将 TraderManager 的当前商店设为: {myShopData.name}");

        // 2. 打开商店 UI
        if (shopUIPanel != null)
        {
            shopUIPanel.SetActive(true);
            Debug.Log("[ShopNPC] 商店 UI 面板已设为 Active");
            
            // 3. 触发 UI 刷新
            var displayUI = shopUIPanel.GetComponentInChildren<ShopDisplayUI>();
            if (displayUI != null) displayUI.Refresh();

            var listUI = shopUIPanel.GetComponentInChildren<ShopListUI>();
            if (listUI != null) listUI.Refresh();
        }
        else
        {
            Debug.LogError("[ShopNPC] 错误：Shop UI Panel 未赋值！");
        }
    }

    // 现代点击检测接口（更稳定，推荐）
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[ShopNPC] IPointerClickHandler 检测到点击");
        OpenShop();
    }

    // 传统点击检测（备选）
    private void OnMouseDown()
    {
        Debug.Log("[ShopNPC] OnMouseDown 检测到点击");
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[ShopNPC] 点击被 UI 遮挡，忽略");
            return;
        }
        OpenShop();
    }
}