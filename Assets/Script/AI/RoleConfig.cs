
using System.Collections.Generic;
using UnityEngine;
using static Constants;
//using UnityEngine.Localization.Settings;
using Unity.Collections;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "RoleConfig", menuName = "Config/Role/RoleConfig")]
public class RoleConfig : ScriptableObject
{
    [Title("通用描述")]
//   [ReadOnly]
    public string guid;
    [Tooltip("也是roleId")]
    public int id;
//    [ValueDropdown(nameof(GetEntityDropdown))]
    public int prefabID;

    [PreviewField(ObjectFieldAlignment.Left)] // 在Inspector中显示图标预览
    public Sprite icon; // 代表角色的图标

    public string fullName; // 角色的全名
    [TextArea(3, 5)] // 多行文本字段，用于输入角色介绍
    public string intro; // 角色的介绍或描述
    public Gender gender; // 角色的性别（可能是一个枚举）

    [Title("能力")] // 在Inspector中显示“能力”部分的标题
    public Vector2Int speedRange; // 速度属性的范围
    public Vector2Int strengthRange; // 力量属性的范围
    public Vector2Int mentalityRange; // 精神属性的范围
    public Vector2Int charmRange; // 魅力属性的范围

    public List<RoleTrait> roleTraits = new List<RoleTrait>(); // 与该角色关联的特质列表
    [SerializeReference] // 允许序列化多态引用
//    public List<RoleEvent> roleEvents = new List<RoleEvent>(); // 与该角色关联的事件列表
    [Tooltip("解锁所需声望")] // 在Inspector中显示提示，说明这是解锁所需的声望值
    public float unlockExp; // 解锁该角色所需的声望值

/*   private ValueDropdownList<int> GetEntityDropdown()
    {
        return EntityConfigComponent.EditorGetDropdown();
    }*

    #region 本地化 (Localization)
    [Title("本地化")] // 在Inspector中显示“本地化”部分的标题
    [SerializeField]
    private string translationNameFullName; // 用于本地化的全名键
    [SerializeField]
    private string translationNameIntro; // 用于本地化的介绍键

    // 更新角色的本地化内容
/*    public void OnTranslation()
    {
        var fullName = LocalizationSettings.StringDatabase.GetLocalizedString(Constants.localeTableUI, translationNameFullName);
        var intro = LocalizationSettings.StringDatabase.GetLocalizedString(Constants.localeTableUI, translationNameIntro);
        this.fullName = string.IsNullOrEmpty(fullName) ? this.fullName : fullName; // 如果翻译存在，则更新全名
        this.intro = string.IsNullOrEmpty(intro) ? this.intro : intro; // 如果翻译存在，则更新介绍

        roleEvents.ForEach(x => x.OnTranslation()); // 更新所有角色事件的本地化内容
    }
    #endregion*/

}

// 嵌套类，用于定义角色特质
[System.Serializable]
public class RoleTrait
{
    public string title; // 特质的标题
    [TextArea(3, 5)] // 多行文本字段，用于输入特质的说明
    public string explain; // 特质的说明
    [Tooltip("是否为增益")] // 在Inspector中显示提示，说明这是否是一个增益特质
    public bool isGain; // 该特质是否为增益效果

    #region 本地化 (Localization)
    [Title("本地化")] // 在Inspector中显示“本地化”部分的标题
    [SerializeField]
    public string translationNameTitle; // 用于本地化的特质标题键
    [SerializeField]
    private string translationNameExplain; // 用于本地化的特质说明键

    // 更新特质的本地化内容
/*   public void OnTranslation()
    {
        var title = LocalizationSettings.StringDatabase.GetLocalizedString(Constants.localeTableUI, translationNameTitle);
        var explain = LocalizationSettings.StringDatabase.GetLocalizedString(Constants.localeTableUI, translationNameExplain);
        this.title = string.IsNullOrEmpty(title) ? this.title : title; // 如果翻译存在，则更新标题
        this.explain = string.IsNullOrEmpty(explain) ? this.explain : explain; // 如果翻译存在，则更新说明
    }*/
    #endregion
}

