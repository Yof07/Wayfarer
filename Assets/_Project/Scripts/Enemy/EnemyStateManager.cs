using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))] // 依赖 Enemy 组件，确保同物体上有 Enemy
[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy State Manager")] // 在组件菜单中显示该脚本的位置
public class EnemyStateManager : EntityStateManager<Enemy>
{
    // 存放敌人状态类名的字符串数组，用于动态创建状态实例
    [ClassTypeName(typeof(EnemyState))]
    public string[] states;

    // 重写基类方法，根据 states 字符串数组创建对应的 EnemyState 状态列表
    protected override List<EntityState<Enemy>> GetStateList()
    {
        return EnemyState.CreateListFromStringArray(states);
    }
}
