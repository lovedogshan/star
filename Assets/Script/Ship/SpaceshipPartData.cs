using UnityEngine;

// 配件类型（和挂载点类型对应）
public enum PartType
{
    FixedCannon,    // 固定武器
    Turret,   // 炮台炮
    Engine_Basic,    // 基础引擎
    Shield_Standard  // 标准护盾
}

// 配件尺寸（限制挂载点适配性）
public enum PartSize
{
    Small,
    Medium,
    Large
}

public enum MountPointType
{
    Weapon,     // 武器挂载点
    Engine,     // 引擎挂载点
    Shield      // 护盾挂载点
}



[CreateAssetMenu(fileName = "NewPartData", menuName = "Spaceship/Part Data")]
public class SpaceshipPartData : ScriptableObject
{
    [Header("基础信息")]
    public string partName;        // 配件名称
    public Sprite partIcon;        // 配件图标
    public GameObject partPrefab;  // 配件3D模型预制体
    public PartType partType;      // 配件类型
    public PartSize partSize;      // 配件尺寸

    [Header("属性")]
    public float damage;           // 伤害（武器专属）
    public float fireRate;         // 射速（武器专属）
    public float speedBoost;       // 速度加成（引擎专属）
    public float shieldValue;      // 护盾值（护盾专属）
}