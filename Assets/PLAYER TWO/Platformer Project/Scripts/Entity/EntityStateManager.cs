
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抽象基类，用于管理实体状态机，带有事件支持。
/// </summary>
public abstract class EntityStateManager : MonoBehaviour
{
    /// <summary>
    /// 状态管理相关事件集合（进入状态、退出状态、状态切换等）。
    /// 具体定义在 EntityStateManagerEvents 中。
    /// </summary>
    public EntityStateManagerEvents events;
}

/// <summary>
/// 泛型抽象类，继承自 EntityStateManager，管理特定实体类型 T 的状态机。
/// T 必须继承自 Entity<T>。
/// </summary>
/// <typeparam name="T">实体类型，必须继承自 Entity<T>。</typeparam>
public abstract class EntityStateManager<T> : EntityStateManager where T : Entity<T>
{

    /// <summary>
    /// 持有所有状态实例的列表，顺序定义状态管理器的状态顺序。
    /// </summary>
    protected List<EntityState<T>> m_list = new List<EntityState<T>>();

    /// <summary>
    /// 状态字典，键为状态类型，值为对应状态实例，方便快速查找。
    /// </summary>
    protected Dictionary<Type, EntityState<T>> m_states = new Dictionary<Type, EntityState<T>>();

    /// <summary>
    /// 当前激活的状态实例。
    /// </summary>
    public EntityState<T> current { get; protected set; }

    /// <summary>
    /// 上一个状态实例。
    /// </summary>
    public EntityState<T> last { get; protected set; }

    /// <summary>
    /// 当前状态在状态列表中的索引位置。
    /// </summary>
    public int index => m_list.IndexOf(current);

    /// <summary>
    /// 上一个状态在状态列表中的索引位置。
    /// </summary>
    public int lastIndex => m_list.IndexOf(last);

    /// <summary>
    /// 该状态管理器关联的实体实例。
    /// </summary>
    public T entity { get; protected set; }


    /// <summary>
    /// Unity 生命周期 Start，负责初始化实体和状态。
    /// </summary>
    protected virtual void Start()
    {
        InitializeEntity();
        InitializeStates();
    }

    /// <summary>
    /// 虚方法，默认实现为从当前 GameObject 获取实体组件 T。
    /// 可以被子类重写自定义实体初始化逻辑。
    /// </summary>
    protected virtual void InitializeEntity() => entity = GetComponent<T>();

    /// <summary>
    /// 抽象方法，必须由子类实现，用于返回所有状态实例的列表。
    /// </summary>
    protected abstract List<EntityState<T>> GetStateList();

    /// <summary>
    /// 初始化状态列表和状态字典。
    /// 会调用 GetStateList() 获取状态列表，并将状态加入字典以便快速查找。
    /// 同时默认将 current 设为状态列表的第一个状态（如果存在）。
    /// </summary>
    protected virtual void InitializeStates()
    {
        m_list = GetStateList();

        foreach (var state in m_list)
        {
            var type = state.GetType();

            // 避免重复添加相同类型的状态
            if (!m_states.ContainsKey(type))
            {
                m_states.Add(type, state);
            }
        }

        // 如果状态列表不为空，默认激活第一个状态
        if (m_list.Count > 0)
        {
            current = m_list[0];
        }
    }

    /// <summary>
    /// 每帧调用，用于更新当前状态的逻辑。
    /// </summary>
    public virtual void Step()
    {
        // 确保当前状态存在且游戏未暂停
        if (current != null && Time.timeScale > 0)
        {
            current.Step(entity);
        }
    }

    /// <summary>
    /// 根据状态类型泛型参数切换状态。
    /// </summary>
    /// <typeparam name="TState">目标状态类型，必须继承自 EntityState<T>。</typeparam>
    public virtual void Change<TState>() where TState : EntityState<T>
    {
        var type = typeof(TState);

        if (m_states.ContainsKey(type))
        {
            Change(m_states[type]);
        }
    }

    /// <summary>
    /// 根据状态列表索引切换当前状态。
    /// </summary>
    /// <param name="to">目标状态在状态列表中的索引。</param>
    public virtual void Change(int to)
    {
        if (to >= 0 && to < m_list.Count)
        {
            Change(m_list[to]);
        }
    }

    /// <summary>
    /// 根据状态实例切换当前状态。
    /// 执行状态的退出与进入回调，并触发相关事件。
    /// </summary>
    /// <param name="to">目标状态实例。</param>
    public virtual void Change(EntityState<T> to)
    {
        // 确保目标状态不为空且游戏未暂停（Time.timeScale > 0）
        if (to != null && Time.timeScale > 0)
        {
            // 如果有当前状态，调用退出逻辑并触发退出事件
            if (current != null)
            {
                current.Exit(entity);
                events.onExit.Invoke(current.GetType());
                last = current;
            }

            // 切换到目标状态，调用进入逻辑并触发进入事件和状态切换事件
            current = to;
            current.Enter(entity);
            events.onEnter.Invoke(current.GetType());
            events.onChange?.Invoke();
        }
    }

    /// <summary>
    /// 判断当前状态是否为指定类型。
    /// </summary>
    /// <param name="type">需要比较的状态类型。</param>
    /// <returns>如果当前状态类型等于参数类型返回 true，否则返回 false。</returns>
    public virtual bool IsCurrentOfType(Type type)
    {
        if (current == null)
        {
            return false;
        }

        return current.GetType() == type;
    }

    /// <summary>
    /// 判断状态管理器是否包含指定类型的状态。
    /// </summary>
    /// <param name="type">状态类型。</param>
    /// <returns>如果包含返回 true，否则 false。</returns>
    public virtual bool ContainsStateOfType(Type type) => m_states.ContainsKey(type);

    /// <summary>
    /// 当实体与其他碰撞体接触时调用，将事件传递给当前状态。
    /// </summary>
    /// <param name="other">碰撞到的其他碰撞体。</param>
    public virtual void OnContact(Collider other)
    {
        if (current != null && Time.timeScale > 0)
        {
            current.OnContact(entity, other);
        }
    }
}
