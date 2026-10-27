using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 这个组件依赖于 Image 组件（必须挂载在带有 UI Image 的对象上）
[RequireComponent(typeof(Image))]
// 在 Unity 菜单中显示的位置
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Fader")]
public class Fader : Singleton<Fader>
{
    // 控制渐变的速度，数值越大，渐变越快
    public float speed = 1f;

    // 用于存储 UI Image 组件的引用
    protected Image m_image;

    /// <summary>
    /// 渐隐（变黑/全透明度为1），没有回调函数
    /// </summary>
    public void FadeOut() => FadeOut(() => { });

    /// <summary>
    /// 渐显（显示出来/透明度为0），没有回调函数
    /// </summary>
    public void FadeIn() => FadeIn(() => { });

    /// <summary>
    /// 渐隐，并在结束时执行回调函数
    /// </summary>
    /// <param name="onFinished">渐隐完成后要执行的动作</param>
    public void FadeOut(Action onFinished)
    {
        // 先停止所有正在运行的协程，防止多个渐变冲突
        StopAllCoroutines();
        // 启动渐隐协程
        StartCoroutine(FadeOutRoutine(onFinished));
    }

    /// <summary>
    /// 渐显，并在结束时执行回调函数
    /// </summary>
    /// <param name="onFinished">渐显完成后要执行的动作</param>
    public void FadeIn(Action onFinished)
    {
        // 同样，先停止所有协程，保证只有一个渐变执行
        StopAllCoroutines();
        // 启动渐显协程
        StartCoroutine(FadeInRoutine(onFinished));
    }

    /// <summary>
    /// 直接设置透明度，不带过渡效果
    /// </summary>
    /// <param name="alpha">透明度（0=完全透明，1=完全不透明）</param>
    public virtual void SetAlpha(float alpha)
    {
        var color = m_image.color;
        color.a = alpha; // 修改 alpha 通道
        m_image.color = color; // 应用回去
    }

    /// <summary>
    /// 协程：逐渐增加透明度直到完全不透明（alpha=1），然后调用回调
    /// </summary>
    protected virtual IEnumerator FadeOutRoutine(Action onFinished)
    {
        // 循环执行，直到 alpha >= 1
        while (m_image.color.a < 1)
        {
            var color = m_image.color;
            // 每帧根据速度和时间增大 alpha
            color.a += speed * Time.deltaTime;
            m_image.color = color;
            // 等待下一帧继续
            yield return null;
        }

        // 执行回调
        onFinished?.Invoke();
    }

    /// <summary>
    /// 协程：逐渐减小透明度直到完全透明（alpha=0），然后调用回调
    /// </summary>
    protected virtual IEnumerator FadeInRoutine(Action onFinished)
    {
        // 循环执行，直到 alpha <= 0
        while (m_image.color.a > 0)
        {
            var color = m_image.color;
            // 每帧根据速度和时间减小 alpha
            color.a -= speed * Time.deltaTime;
            m_image.color = color;
            // 等待下一帧继续
            yield return null;
        }

        // 执行回调
        onFinished?.Invoke();
    }

    /// <summary>
    /// Unity 生命周期方法，在 Awake 阶段执行
    /// </summary>
    protected override void Awake()
    {
        // 调用父类 Singleton 的 Awake，保证单例正确初始化
        base.Awake();
        // 获取 Image 组件
        m_image = GetComponent<Image>();
    }
}
