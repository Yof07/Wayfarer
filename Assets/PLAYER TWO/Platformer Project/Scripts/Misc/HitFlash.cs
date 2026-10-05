using System.Collections;
using UnityEngine;

// 要求该组件必须绑定在一个带有 Health 组件的对象上
[RequireComponent(typeof(Health))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Hit Flash")]
public class HitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    // 需要执行闪烁效果的蒙皮网格渲染器数组
    public SkinnedMeshRenderer[] renderers;

    // 受击闪烁时的颜色
    public Color flashColor = Color.red;

    // 闪烁持续时间
    public float flashDuration = 0.5f;

    // 缓存的血量组件
    protected Health m_health;

    /// <summary>
    /// 启动闪烁效果。
    /// 会停止当前所有的协程，
    /// 然后对指定的渲染器材质执行闪烁动画。
    /// </summary>
    public virtual void Flash()
    {
        // 先停止之前可能正在运行的闪烁协程
        StopAllCoroutines();

        // 遍历所有需要闪烁的模型渲染器
        foreach (var renderer in renderers)
        {
            // 为每个材质启动一个闪烁协程
            StartCoroutine(FlashRoutine(renderer.material));
        }
    }

    /// <summary>
    /// 执行具体的闪烁逻辑的协程。
    /// 在 flashDuration 时间内将材质颜色
    /// 从 flashColor 逐渐过渡回原始颜色。
    /// </summary>
    /// <param name="material">要闪烁的材质</param>
    protected virtual IEnumerator FlashRoutine(Material material)
    {
        var elapsedTime = 0f;                   // 已经过的时间
        var flashColor = this.flashColor;       // 闪烁目标颜色
        var initialColor = material.color;      // 材质原始颜色

        // 在闪烁持续时间内，不断插值改变颜色
        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            // 使用 Lerp 从闪烁颜色过渡回原始颜色
            material.color = Color.Lerp(flashColor, initialColor, elapsedTime / flashDuration);
            yield return null; // 等待下一帧
        }

        // 恢复成初始颜色
        material.color = initialColor;
    }

    /// <summary>
    /// 初始化时绑定 Health 组件，并监听其 onDamage 事件。
    /// 当对象受到伤害时自动触发 Flash 效果。
    /// </summary>
    protected virtual void Start()
    {
        m_health = GetComponent<Health>();
        // 当受到伤害时执行 Flash
        m_health.onDamage.AddListener(Flash);
    }
}
