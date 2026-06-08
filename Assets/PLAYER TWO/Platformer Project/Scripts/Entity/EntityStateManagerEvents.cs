using System;
using UnityEngine.Events;

/// <summary>
/// 用于管理实体状态机事件的序列化类。
/// 通过该类可以在 Inspector 中绑定和触发状态相关事件。
/// </summary>
[Serializable]
public class EntityStateManagerEvents
{
    /// <summary>
    /// 当状态发生切换时触发的事件。
    /// </summary>
    public UnityEvent onChange;

    /// <summary>
    /// 当进入某个状态时触发的事件。
    /// 传递被进入状态的类型信息（Type），方便外部根据状态类型做不同处理。
    /// </summary>
    public UnityEvent<Type> onEnter;

    /// <summary>
    /// 当退出某个状态时触发的事件。
    /// 传递被退出状态的类型信息（Type），方便外部根据状态类型做不同处理。
    /// </summary>
    public UnityEvent<Type> onExit;
}