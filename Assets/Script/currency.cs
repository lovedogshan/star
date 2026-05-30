using UnityEngine;
using TMPro;

public class currency : MonoBehaviour
{
    [Header("=== 初始配置 ===")]
    public float startingMoney = 5000f; // 在 Inspector 中设置初始金额

    // 运行时金钱（静态属性，全局可用）
    private static float _money; 
    
    public static float Money 
    { 
        get => _money; 
        set => _money = value; 
    }

    [Header("=== UI 绑定 ===")]
    public TMP_Text moneyText; // 拖入显示玩家余额的文本

    private void Awake()
    {
        // 游戏启动时，将 Inspector 设置的初始金额同步到静态变量
        _money = startingMoney;
    }

    private void Start()
    {
        UpdateMoneyUI();
    }

    private void Update()
    {
        // 实时刷新（也可以改为事件驱动以节省性能）
        UpdateMoneyUI();
    }

    public void UpdateMoneyUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"$ {Money:F0}";
        }
    }
}
