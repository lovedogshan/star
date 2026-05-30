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
        if (partIconImage != null)
        {
            if (data.partIcon != null)
            {
                // 关键：这里直接给 Image 组件的 sprite 属性赋值
                // 等同于在 Inspector 中设置 "Source Image"
                partIconImage.sprite = data.partIcon;
                
                // 确保 Image 颜色不透明
                partIconImage.color = Color.white;

                // 可选：保持宽高比
                partIconImage.preserveAspect = true;
                
                // 打印成功日志
                Debug.Log($"[ItemUI] 已将图片 {data.partIcon.name} 设置到 Source Image");
            }
            else
            {
                Debug.LogWarning($"[ItemUI] 配件数据 {data.partName} 中 partIcon 为空！无法设置 Source Image");
                // 如果数据中没有图标，将Image设为透明或使用默认图标，避免显示白块
                partIconImage.sprite = null;
                partIconImage.color = Color.clear;
            }
        }
        else
        {
             Debug.LogError($"[ItemUI] partIconImage 引用未赋值！请检查 Prefab 上是否拖入了 Image 组件");
        }
        
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
