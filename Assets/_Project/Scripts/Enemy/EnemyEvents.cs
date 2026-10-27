using System;
using UnityEngine.Events;

[Serializable]
public class EnemyEvents
{
    /// <summary>
    /// 当玩家进入敌人的视野时触发
    /// </summary>
    public UnityEvent OnPlayerSpotted;

    /// <summary>
    /// 当玩家离开敌人的视野时触发
    /// </summary>
    public UnityEvent OnPlayerScaped;

    /// <summary>
    /// 当敌人与玩家接触时触发
    /// </summary>
    public UnityEvent OnPlayerContact;

    /// <summary>
    /// 当敌人受到伤害时触发
    /// </summary>
    public UnityEvent OnDamage;

    /// <summary>
    /// 当敌人生命值归零死亡时触发
    /// </summary>
    public UnityEvent OnDie;
}
