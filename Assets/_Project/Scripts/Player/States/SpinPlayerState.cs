using UnityEngine;


/// <summary>
/// 玩家旋转状态（Spin）
/// - 玩家在空中进行旋转动作
/// - 可在旋转期间进行加速、空中冲刺或踩踏攻击
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Spin Player State")]
public class SpinPlayerState : PlayerState
{
    /// <summary>
    /// 进入旋转状态时触发
    /// - 如果玩家在空中，旋转能让玩家往上跃升一小段
    /// </summary>
    protected override void OnEnter(Player player)
    {
        if (!player.isGrounded)
        {
            // 设置玩家向上的速度
            player.verticalVelocity = Vector3.up * player.stats.current.airSpinUpwardForce;
        }
    }

    /// <summary>
    /// 离开旋转状态时触发（此状态无特殊退出逻辑）
    /// </summary>
    protected override void OnExit(Player player)
    {
    }

    /// <summary>
    /// 每帧更新旋转状态逻辑
    /// - 施加重力
    /// - 修正玩家位置贴地
    /// - 可进行空中冲刺（AirDive）或踩踏攻击（StompAttack）
    /// - 根据输入方向加速
    /// - 判断旋转持续时间，结束旋转并切换到合适状态
    /// </summary>
    protected override void OnStep(Player player)
    {
        player.Gravity(); // 应用重力
        player.SnapToGround(); // 修正玩家贴地位置
        player.AirDive(); // 空中冲刺
        player.StompAttack(); // 空中踩踏攻击
        player.AccelerateToInputDirection(); // 根据输入方向加速移动

        // 如果旋转时间超过设定持续时间
        if (timeSinceEntered >= player.stats.current.spinDuration)
        {
            if (player.isGrounded)
            {
                // 着地 → 空闲状态
                player.states.Change<IdlePlayerState>();
            }
            else
            {
                // 空中 → 下落状态
                player.states.Change<FallPlayerState>();
            }
        }
    }

    /// <summary>
    /// 触发碰撞事件
    /// - 旋转状态下不处理碰撞
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
    }
}
