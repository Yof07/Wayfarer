using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UI 动画控制器，用于管理界面元素的显示与隐藏动画
/// </summary>
[RequireComponent(typeof(Animator))] // 确保挂载此脚本的 GameObject 必须有 Animator 组件
[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Animator")] // 编辑器菜单路径
public class UIAnimator : MonoBehaviour
{
    /// <summary>
    /// 当执行 Show 动作时调用的事件
    /// </summary>
    public UnityEvent OnShow;

    /// <summary>
    /// 当执行 Hide 动作时调用的事件
    /// </summary>
    public UnityEvent OnHide;

    // 是否在 Awake 时隐藏 UI
    public bool hidenOnAwake;

    // Animator 中的触发器名称
    public string normalTrigger = "Normal"; // 默认状态触发器
    public string showTrigger = "Show";     // 显示动画触发器
    public string hideTrigger = "Hide";     // 隐藏动画触发器

    // 内部 Animator 引用
    protected Animator m_animator;


    /// <summary>
    /// 初始化 Animator 组件，并根据 hidenOnAwake 设置初始状态
    /// </summary>
    protected virtual void Awake()
    {
        m_animator = GetComponent<Animator>(); // 获取 Animator 组件引用

        if (hidenOnAwake)
        {
            // 如果 Awake 时需要隐藏，则播放隐藏动画的最后一帧
            m_animator.Play(hideTrigger, 0, 1);
        }
    }

    /// <summary>
    /// 触发显示动画
    /// </summary>
    public virtual void Show()
    {
        m_animator.SetTrigger(showTrigger); // 设置 Animator 触发器
        OnShow?.Invoke();                   // 调用显示事件（如果有绑定）
    }

    /// <summary>
    /// 触发隐藏动画
    /// </summary>
    public virtual void Hide()
    {
        m_animator.SetTrigger(hideTrigger); // 设置 Animator 触发器
        OnHide?.Invoke();                   // 调用隐藏事件（如果有绑定）
    }

    /// <summary>
    /// 设置 GameObject 的激活状态
    /// </summary>
    /// <param name="value">要设置的激活状态</param>
    public virtual void SetActive(bool value) => gameObject.SetActive(value);

}
