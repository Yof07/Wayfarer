using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI 焦点保持器，确保在使用控制器或键盘导航时，
/// 总有一个 UI 元素保持选中状态，避免焦点丢失
/// </summary>
[RequireComponent(typeof(EventSystem))] // 确保挂载此脚本的 GameObject 有 EventSystem 组件
[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Focus Keeper")] // 编辑器菜单路径
public class UIFocusKeeper : MonoBehaviour
{
    // 上一次被选中的 UI 对象
    protected GameObject m_lastSelected;

    // EventSystem 引用，用于管理 UI 事件和焦点
    protected EventSystem m_eventSystem;

    /// <summary>
    /// 初始化 EventSystem 引用
    /// </summary>
    protected virtual void Start()
    {
        m_eventSystem = GetComponent<EventSystem>();
    }

    /// <summary>
    /// 每帧更新焦点状态，确保总有 UI 元素被选中
    /// </summary>
    protected virtual void Update()
    {
        // 当前没有选中的 UI 对象
        if (!m_eventSystem.currentSelectedGameObject)
        {
            // 如果上一次选中的对象存在、处于激活状态，并且可交互
            if (m_lastSelected && m_lastSelected.activeSelf && m_lastSelected.GetComponent<Selectable>().interactable)
            {
                // 将上一次选中的对象重新设置为当前选中对象
                m_eventSystem.SetSelectedGameObject(m_lastSelected);
            }
        }
        else
        {
            // 当前有选中对象，则更新 m_lastSelected 为当前选中对象
            m_lastSelected = m_eventSystem.currentSelectedGameObject;
        }
    }
}
