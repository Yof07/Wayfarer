using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using System.Collections;

/// <summary>
/// 水平自动滚动 UI 组件，用于 ScrollRect 内的子元素导航，
/// 支持手柄或键盘输入平滑滚动到指定子元素
/// </summary>
[RequireComponent(typeof(ScrollRect))] // 确保挂载此脚本的 GameObject 有 ScrollRect 组件
[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Horizontal Auto Scroll")] // 编辑器菜单路径
public class UIHorizontalAutoScroll : MonoBehaviour
{
    // 滚动动画持续时间
    public float scrollDuration = 0.25f;

    // 当前选中子元素索引
    protected int m_currentChild;
    // ScrollRect 中总子元素数量
    protected int m_totalChildren;

    // 是否正在滚动
    protected bool m_moving;
    // 输入首次触发时间
    protected float m_moveInitTime;
    // 上一次滚动触发时间
    protected float m_moveRepeatTime;
    // 上一次输入值
    protected float m_lastInput;

    // ScrollRect 组件引用
    protected ScrollRect m_scrollRect;
    // 输入模块引用，用于读取手柄/键盘输入
    protected InputSystemUIInputModule m_input;

    // 输入重复滚动的最小间隔时间
    protected const float k_inputRepeatDelay = 0.1f;

    /// <summary>
    /// 开始滚动到当前子元素
    /// </summary>
    protected virtual void Scroll()
    {
        StopAllCoroutines(); // 停止之前的滚动协程
        StartCoroutine(ScrollRoutine());
    }

    /// <summary>
    /// 滚动协程，实现平滑滚动动画
    /// </summary>
    protected virtual IEnumerator ScrollRoutine()
    {
        // 初始水平归一化位置
        var initial = m_scrollRect.horizontalNormalizedPosition;
        // 目标水平归一化位置，根据当前子元素索引计算
        var target = m_currentChild / ((float)m_totalChildren - 1);
        var elapsedTime = 0f;

        // 平滑插值滚动
        while (elapsedTime < scrollDuration)
        {
            elapsedTime += Time.deltaTime;
            m_scrollRect.horizontalNormalizedPosition = Mathf.Lerp(initial, target, elapsedTime / scrollDuration);
            yield return null;
        }

        // 最终位置校准
        m_scrollRect.horizontalNormalizedPosition = target;
    }

    /// <summary>
    /// 初始化组件引用和子元素数量
    /// </summary>
    protected virtual void Start()
    {
        m_scrollRect = GetComponent<ScrollRect>();
        m_input = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        m_totalChildren = m_scrollRect.content.childCount;
    }

    /// <summary>
    /// 每帧更新输入检测，实现自动滚动
    /// </summary>
    protected virtual void Update()
    {
        // 获取水平输入值
        var horizontal = m_input.move.action.ReadValue<Vector2>().x;

        if (horizontal != 0)
        {
            // 检测首次触发滚动
            if (m_moveInitTime + m_input.moveRepeatDelay < Time.time)
            {
                if (!m_moving)
                {
                    m_moving = true;
                    m_moveInitTime = Time.time;
                }

                // 控制连续滚动间隔
                if (m_moveRepeatTime + k_inputRepeatDelay < Time.time)
                {
                    if (horizontal > 0)
                    {
                        m_currentChild++; // 向右滚动
                    }
                    else
                    {
                        m_currentChild--; // 向左滚动
                    }

                    m_moveRepeatTime = Time.time;
                    // 限制子元素索引在合法范围
                    m_currentChild = Mathf.Clamp(m_currentChild, 0, m_totalChildren - 1);
                    Scroll(); // 执行滚动
                }
            }
        }
        else
        {
            // 无输入时重置状态
            m_moving = false;
            m_moveInitTime = m_moveRepeatTime = 0;
        }
    }
}
