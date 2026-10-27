using UnityEngine;

//脚本主要用于 移动端虚拟摇杆 UI 的显示控制
#if UNITY_EDITOR
// 允许该脚本在编辑器模式下运行（不需要进入 Play 模式）
[ExecuteInEditMode]
#endif
// 要求该组件必须依附在 RectTransform 上（常用于 UI 元素）
[RequireComponent(typeof(RectTransform))]
// 将此脚本添加到 Unity 菜单中，方便在 Inspector 中添加
[AddComponentMenu("PLAYER TWO/Platformer Project/Gamepad/Mobile Rig")]
public class MobileRig : MonoBehaviour
{
    /// <summary>
    /// 当对象启用时调用，检查并设置 UI 控制器的启用状态。
    /// </summary>
    protected virtual void OnEnable()
    {
        CheckEnable();
    }

#if UNITY_EDITOR
    /// <summary>
    /// 在编辑器模式下，每一帧调用，确保在非运行状态下也能动态更新状态。
    /// </summary>
    protected virtual void Update()
    {
        if (!Application.isPlaying) // 仅在编辑器下非运行时生效
        {
            CheckEnable();
        }
    }
#endif

    /// <summary>
    /// 检查平台环境，根据平台来决定是否启用虚拟摇杆 UI。
    /// </summary>
    protected virtual void CheckEnable()
    {
#if UNITY_IOS || UNITY_ANDROID
        // 如果是 iOS 或 Android 平台，启用虚拟摇杆 UI
        EnableRig(true);
#else
        // 否则（PC/主机等），禁用虚拟摇杆 UI
        EnableRig(false);
#endif
    }

    /// <summary>
    /// 控制当前对象下所有子物体的显示/隐藏。
    /// 通常子物体就是 UI 控件（如虚拟摇杆、按键等）。
    /// </summary>
    /// <param name="value">是否启用虚拟摇杆</param>
    public virtual void EnableRig(bool value)
    {
        foreach (Transform t in transform)
        {
            t.gameObject.SetActive(value);
        }
    }
}
