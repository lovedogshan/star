using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 挂载点点击检测器（直接实现IPointerClickHandler，绕过Button的坑）
/// </summary>
public class MountPointClickHandler : MonoBehaviour, IPointerClickHandler
{
    public Action onClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[MountPointClickHandler] 点击事件被触发！物体名: {gameObject.name}");
        onClick?.Invoke();
    }
}
