using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 监听实体状态管理器（EntityStateManager）状态切换事件的组件。
/// 可绑定在实体或其子物体上，监听特定状态的进入和退出事件，并触发对应的 UnityEvent。
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity State Manager Listener")]
public class EntityStateManagerListener : MonoBehaviour
{
    /// <summary>
    /// 当进入指定状态时触发的事件，可在 Inspector 中绑定响应动作。
    /// </summary>
    public UnityEvent onEnter;

    /// <summary>
    /// 当退出指定状态时触发的事件，可在 Inspector 中绑定响应动作。
    /// </summary>
    public UnityEvent onExit;

    /// <summary>
    /// 需要监听的状态名称列表，只监听这些状态的进入和退出。
    /// </summary>
    public List<string> states;

    /// <summary>
    /// 持有的状态管理器引用，负责管理实体状态。
    /// </summary>
    protected EntityStateManager m_manager;

    /// <summary>
    /// Unity 生命周期方法，启动时获取 EntityStateManager 组件并订阅其事件。
    /// </summary>
    protected virtual void Start()
    {
        // 如果没有手动赋值，则尝试从父物体获取 EntityStateManager 组件
        if (!m_manager)
        {
            m_manager = GetComponentInParent<EntityStateManager>();
        }

        // 订阅状态管理器的状态进入和退出事件，绑定回调方法
        m_manager.events.onEnter.AddListener(OnEnter);
        m_manager.events.onExit.AddListener(OnExit);
    }

    /// <summary>
    /// 当状态管理器触发进入状态事件时调用。
    /// 如果进入的状态名称包含在监听列表中，则触发 onEnter UnityEvent。
    /// </summary>
    /// <param name="state">进入的状态类型。</param>
    protected virtual void OnEnter(Type state)
    {
        if (states.Contains(state.Name))
        {
            onEnter.Invoke();
        }
    }

    /// <summary>
    /// 当状态管理器触发退出状态事件时调用。
    /// 如果退出的状态名称包含在监听列表中，则触发 onExit UnityEvent。
    /// </summary>
    /// <param name="state">退出的状态类型。</param>
    protected virtual void OnExit(Type state)
    {
        if (states.Contains(state.Name))
        {
            onExit.Invoke();
        }
    }
}
