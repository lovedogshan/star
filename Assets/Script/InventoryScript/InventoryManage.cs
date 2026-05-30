using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManage : MonoBehaviour
{
    public static InventoryManage instance;
    
    [Header("=== 数据引用 ===")]
    public Inventory Cargo;

    [Header("=== UI 详情显示 (全局共享) ===")]
    public Image itemImage;
    public Text itemInfromation;
    
    // 管理所有正在显示的 UI 界面
    private List<CargoDisplayUI> activeUIs = new List<CargoDisplayUI>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // 注册一个新的显示界面
    public void RegisterUI(CargoDisplayUI ui)
    {
        if (!activeUIs.Contains(ui)) activeUIs.Add(ui);
    }

    // 注销一个界面
    public void UnregisterUI(CargoDisplayUI ui)
    {
        if (activeUIs.Contains(ui)) activeUIs.Remove(ui);
    }

    private void OnEnable()
    {
        RefreshItem();
        if (itemInfromation != null) itemInfromation.text = "";
    }

    public static void UpdateItemimage(Image itemImage)
    {
        if (instance == null) return;
        instance.itemImage = itemImage;
    }

    public static void UpdateItemInfo(string itemDescription)
    {
        if (instance == null || instance.itemInfromation == null) return;
        instance.itemInfromation.text = itemDescription;
    }

    /// <summary>
    /// 触发所有已注册的 UI 界面进行刷新
    /// </summary>
    public static void RefreshItem()
    {
        if (instance == null) return;

        foreach (var ui in instance.activeUIs)
        {
            if (ui != null) ui.Refresh();
        }
    }
}
