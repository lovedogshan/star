using UnityEngine;


[CreateAssetMenu(fileName = "NewSpaceshipData", menuName = "Spaceship/Spaceship Data")]
public class SpaceshipData : ScriptableObject
{
    [Header("基础信息（必须唯一）")]
    public string shipID; // 唯一标识，如 "Ship_Frigate_01"，手动配置，避免重复
    public string shipName;
    public Sprite previewImage;
    public GameObject shipPrefab;

    // 其余属性（maxHP、speed、mountPoints等）保持不变
    [Header("飞船属性")]
    public float maxHP;
    public float speed;
    public float armor;


}



// 挂载点点击检测脚本（可拆分到单独文件）
public class MountPointClickDetector : MonoBehaviour
{
    public MountPointData mountPointData;
    public System.Action onClick;

    private void OnMouseDown()
    {
        onClick?.Invoke();
    }
}