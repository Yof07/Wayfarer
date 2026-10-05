using UnityEngine;

/// <summary>
/// 玩家抓杆爬升状态
/// - 玩家抓住杆子进行上下爬升或左右旋转
/// - 支持跳离杆子并切换到下落状态
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Pole Climbing Player State")]
public class PoleClimbingPlayerState : PlayerState
{
    // 碰撞半径，用于计算玩家与杆子的距离
    protected float m_collisionRadius;

    // 玩家与杆子之间的微小偏移，避免穿透
    protected const float k_poleOffset = 0.01f;

    /// <summary>
    /// 进入抓杆爬升状态
    /// - 重置跳跃、空中旋转和冲刺次数
    /// - 清零玩家速度
    /// - 计算玩家到杆子的方向与碰撞半径
    /// - 调整玩家皮肤偏移
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.ResetJumps();       // 重置跳跃次数
        player.ResetAirSpins();    // 重置空中旋转次数
        player.ResetAirDash();     // 重置空中冲刺次数
        player.velocity = Vector3.zero;

        // 获取玩家到杆子的方向并计算碰撞半径
        player.pole.GetDirectionToPole(player.transform, out m_collisionRadius);

        // 调整玩家皮肤偏移以贴合杆子
        player.skin.position += player.transform.rotation * player.stats.current.poleClimbSkinOffset;
    }

    /// <summary>
    /// 离开抓杆爬升状态
    /// - 还原皮肤偏移
    /// </summary>
    protected override void OnExit(Player player)
    {
        player.skin.position -= player.transform.rotation * player.stats.current.poleClimbSkinOffset;
    }

    /// <summary>
    /// 每帧更新抓杆爬升逻辑
    /// - 玩家面向杆子方向
    /// - 根据左右输入旋转玩家
    /// - 根据上下输入进行爬升或下降
    /// - 跳跃时离开杆子并切换下落状态
    /// - 确保玩家位置始终贴合杆子
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 获取玩家到杆子的方向
        var poleDirection = player.pole.GetDirectionToPole(player.transform);
        var inputDirection = player.inputs.GetMovementDirection();

        // 玩家朝杆子方向
        player.FaceDirection(poleDirection);

        // 左右旋转（绕杆旋转）
        player.lateralVelocity = player.transform.right * inputDirection.x * player.stats.current.climbRotationSpeed;

        // 上下移动
        if (inputDirection.z != 0)
        {
            var speed = inputDirection.z > 0 ? player.stats.current.climbUpSpeed : -player.stats.current.climbDownSpeed;
            player.verticalVelocity = Vector3.up * speed;
        }
        else
        {
            player.verticalVelocity = Vector3.zero;
        }

        // 玩家跳离杆子
        if (player.inputs.GetJumpDown())
        {
            player.FaceDirection(-poleDirection); // 面向相反方向
            player.DirectionalJump(-poleDirection, player.stats.current.poleJumpHeight, player.stats.current.poleJumpDistance);
            player.states.Change<FallPlayerState>();
        }

        // 玩家到地面，切换到空闲状态
        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }


        // 计算玩家贴杆位置
        var offset = player.height * 0.5f + player.center.y;
        var center = new Vector3(player.pole.center.x, player.transform.position.y, player.pole.center.z);
        var position = center - poleDirection * m_collisionRadius;

        // 将玩家位置约束在杆子高度范围内
        player.transform.position = player.pole.ClampPointToPoleHeight(position, offset);
    }

    /// <summary>
    /// 碰撞处理逻辑
    /// - 抓杆状态通常不处理碰撞
    /// </summary>
    public override void OnContact(Player player, Collider other) { }
}
