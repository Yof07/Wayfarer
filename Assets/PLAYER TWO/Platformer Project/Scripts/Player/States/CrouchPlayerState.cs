using UnityEngine;

public class CrouchPlayerState : PlayerState
{
    /// <summary>
    /// 进入下蹲状态时调用
    /// - 调整碰撞体高度为“蹲伏高度”
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.ResizeCollider(player.stats.current.crouchHeight);
    }

    /// <summary>
    /// 离开下蹲状态时调用
    /// - 恢复碰撞体为原始高度（站立高度）
    /// </summary>
    protected override void OnExit(Player player)
    {
        player.ResizeCollider(player.originalHeight);
    }

    /// <summary>
    /// 每帧更新下蹲逻辑
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 基础物理更新
        player.Gravity(); // 应用重力
        player.SnapToGround(); // 吸附地面
        player.Fall(); // 检查下落
        player.Decelerate(player.stats.current.crouchFriction); // 应用下蹲摩擦力，逐渐减速

        // 获取输入的移动方向（相对世界，不考虑摄像机）
        var inputDirection = player.inputs.GetMovementDirection();

        // 如果玩家仍然按下“下蹲/爬行”键，或因障碍物不能站起来
        if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
        {
            // 1. 玩家有方向输入，并且角色手上没拿东西
            if (inputDirection.sqrMagnitude > 0 && !player.holding)
            {
                // 如果速度为 0 -> 进入爬行状态（从蹲姿转为爬行移动）
                if (player.lateralVelocity.sqrMagnitude == 0)
                {
                    player.states.Change<CrawlingPlayerState>();
                }
            }
            // 2. 玩家在下蹲状态按下“跳跃键” -> 执行后空翻
            else if (player.inputs.GetJumpDown())
            {
                //player.Backflip(player.stats.current.backflipBackwardForce);
            }
        }
        else
        {
            // 如果玩家松开下蹲键，且角色可以站起来 -> 切换为 Idle 状态
            player.states.Change<IdlePlayerState>();
        }
    }

    /// <summary>
    /// 碰撞检测时调用（此状态下没有额外逻辑）
    /// </summary>
    public override void OnContact(Player player, Collider other) { }
}
