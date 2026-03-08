using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 挂载在配件列表项Prefab上的脚本，用于管理单个配件的显示和交互
/// </summary>
public class SpaceshipPartItemUI : MonoBehaviour
{
    [Header("UI组件引用")]
    public Image partIconImage;      // 配件图标
    public TMP_Text partNameText;    // 配件名称文本
    public TMP_Text partStatsText;   // 配件属性文本（如：伤害:100）
    public Button selectButton;      // 选择/装配按钮

    private SpaceshipPartData partData;

    /// <summary>
    /// 初始化列表项
    /// </summary>
    /// <param name="data">配件数据</param>
    /// <param name="onSelectCallback">点击回调</param>
    public void Setup(SpaceshipPartData data, System.Action<SpaceshipPartData> onSelectCallback)
    {
        partData = data;

        // 设置基础信息
        if (partIconImage != null) partIconImage.sprite = data.partIcon;
        if (partNameText != null) partNameText.text = data.partName;

        // 设置属性文本（根据类型显示不同属性）
        if (partStatsText != null)
        {
            string stats = "";
            switch (data.partType)
            {
                case PartType.FixedCannon:
                case PartType.Turret:
                    stats = $"伤害: {data.damage}\n射速: {data.fireRate}";
                    break;
                case PartType.Engine_Basic:
                    stats = $"速度+: {data.speedBoost}";
                    break;
                case PartType.Shield_Standard:
                    stats = $"护盾: {data.shieldValue}";
                    break;
            }
            partStatsText.text = stats;
        }

        // 绑定按钮事件
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelectCallback?.Invoke(partData));
        }
    }
}
