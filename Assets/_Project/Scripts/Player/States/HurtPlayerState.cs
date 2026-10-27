using UnityEngine;

/// <summary>
/// 玩家受伤状态
/// - 当玩家受到伤害时进入该状态
/// - 在空中或地面受到冲击时都会触发此状态
/// - 根据血量判断是否继续游戏或切换到死亡状态
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Hurt Player State")]
public class HurtPlayerState : PlayerState
{
    /// <summary>
    /// 进入受伤状态时调用
    /// （此处可扩展播放受伤动画、音效等）
    /// </summary>
    protected override void OnEnter(Player player)
    {
    }

    /// <summary>
    /// 离开受伤状态时调用
    /// （此处可以清理受伤状态效果）
    /// </summary>
    protected override void OnExit(Player player)
    {
    }

    /// <summary>
    /// 每帧更新受伤状态逻辑
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 应用重力，使受伤玩家自然下落
        player.Gravity();

        // 如果玩家着地且垂直速度 <= 0（不再上升）
        if (player.isGrounded && (player.verticalVelocity.y <= 0))
        {
            // 血量大于 0 -> 切换到 Idle 状态
            if (player.health.current > 0)
            {
                player.states.Change<IdlePlayerState>();
            }
            // 血量 <= 0 -> 切换到 Die 状态
            else
            {
                player.states.Change<DiePlayerState>();
            }
        }
    }

    /// <summary>
    /// 碰撞检测逻辑
    /// - 受伤状态通常不需要额外碰撞处理
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
    }
}
