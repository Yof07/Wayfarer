using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 挂载时要求必须有一个 Image 组件（通常用于 UI 遮罩）
[RequireComponent(typeof(Image))]
// 在 Unity 的菜单栏中显示的路径
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Flash")]
public class Flash : Singleton<Flash>
{
    // 闪屏效果维持的时间（全亮持续时间）
    public float duration = 0.1f;
    // 渐隐消失的时间
    public float fadeDuration = 0.5f;

    // 绑定的 UI Image
    protected Image image;

    /// <summary>
    /// 触发闪屏效果（使用默认的 duration 和 fadeDuration）
    /// </summary>
    public void Trigger() => Trigger(duration, fadeDuration);

    /// <summary>
    /// 触发闪屏效果（自定义持续时间和渐隐时间）
    /// </summary>
    /// <param name="duration">闪屏全亮持续时间</param>
    /// <param name="fadeDuration">渐隐消失时间</param>
    public void Trigger(float duration, float fadeDuration)
    {
        // 防止多个协程叠加，先停止所有协程
        StopAllCoroutines();
        // 启动闪屏效果的协程
        StartCoroutine(Routine(duration, fadeDuration));
    }

    /// <summary>
    /// 闪屏的执行逻辑（协程）
    /// </summary>
    /// <param name="duration">闪屏全亮持续时间</param>
    /// <param name="fadeDuration">渐隐消失时间</param>
    protected IEnumerator Routine(float duration, float fadeDuration)
    {
        var elapsedTime = 0f;       // 记录经过的时间
        var color = image.color;    // 获取当前颜色

        // 1. 设置 alpha = 1（完全不透明，全亮）
        color.a = 1;
        image.color = color;

        // 2. 等待全亮持续时间
        yield return new WaitForSeconds(duration);

        // 3. 渐隐：alpha 从 1 逐渐插值到 0
        while (elapsedTime < fadeDuration)
        {
            color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            image.color = color;

            yield return null; // 等待下一帧
        }

        // 4. 最后确保完全透明
        color.a = 0;
        image.color = color;
    }

    /// <summary>
    /// Unity 生命周期：初始化时获取 Image 组件
    /// </summary>
    protected virtual void Start()
    {
        image = GetComponent<Image>();
    }


}
