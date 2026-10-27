using UnityEngine;

/// <summary>
/// 泛型抽象类，用于定义实体的属性数据。
/// 继承自 ScriptableObject，方便通过 Unity 编辑器创建和管理数据资产。
/// T 是具体的 ScriptableObject 类型，限定属性数据的具体实现。
/// </summary>
/// <typeparam name="T">继承自 ScriptableObject 的具体属性类型。</typeparam>
public abstract class EntityStats<T> : ScriptableObject where T : ScriptableObject { }