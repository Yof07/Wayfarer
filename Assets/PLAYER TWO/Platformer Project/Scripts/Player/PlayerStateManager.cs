
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))] // 强制要求该组件所在的物体必须挂有player组件
public class PlayerStateManager : EntityStateManager<Player>
{
    [ClassTypeName(typeof(PlayerState))]
    public string[] states;
    
    /// <summary>
    /// 重写基类方法，获取该玩家的状态列表。
    /// 会将 states 中的字符串类名数组转换为真正的状态对象列表。
    /// </summary>
    /// <returns>返回一个包含所有状态的 List<EntityState<Player>>。</returns>
    protected override List<EntityState<Player>> GetStateList()
    {
        // 调用 PlayerState 的辅助方法，把字符串数组转换为状态对象集合
        // 例如： "RunPlayerState" → new RunPlayerState()
        return PlayerState.CreateListFromStringArray(states);
    }
}
