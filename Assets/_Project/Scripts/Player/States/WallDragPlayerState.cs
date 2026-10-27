using UnityEngine;

/// <summary>
/// 玩家墙面拖拽状态
/// - 玩家在空中靠墙时触发
/// - 支持墙面下滑、墙面跳跃等操作
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Wall Drag Player State")]
public class WallDragPlayerState : PlayerState
{
    /// <summary>
    /// 进入墙面拖拽状态
    /// </summary>
    protected override void OnEnter(Player player)
    {
        // 重置跳跃、空中旋转、空中冲刺
        player.ResetJumps();
        player.ResetAirSpins();
        player.ResetAirDash();

        // 清空当前速度
        player.velocity = Vector3.zero;

        // 根据上一次碰到的墙面法线调整玩家朝向（水平面方向）
        var direction = player.lastWallNormal;
        direction = new Vector3(direction.x, 0, direction.z).normalized;
        player.FaceDirection(direction);

        // 皮肤偏移，避免模型穿墙
        player.skin.position += player.transform.rotation * player.stats.current.wallDragSkinOffset;

    }

    /// <summary>
    /// 离开墙面拖拽状态
    /// </summary>
    protected override void OnExit(Player player)
    {
        // 恢复皮肤位置
        player.skin.position -= player.transform.rotation * player.stats.current.wallDragSkinOffset;

        // 如果离开墙面且未着地，解除父子关系
        if (!player.isGrounded && player.transform.parent != null)
            player.transform.parent = null;
    }

    /// <summary>
    /// 每帧更新墙面拖拽状态逻辑
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 墙面下滑重力
        player.verticalVelocity += Vector3.down * player.stats.current.wallDragGravity * Time.deltaTime;

        // 如果已着地或不再贴墙 → 切换到闲置状态
        if (player.isGrounded || !player.CapsuleCast(-player.transform.forward, player.radius))
        {
            player.states.Change<IdlePlayerState>();
        }
        // 玩家按下跳跃 → 墙面跳跃
        else if (player.inputs.GetJumpDown())
        {
            // 如果配置锁定移动方向 → 锁定
            if (player.stats.current.wallJumpLockMovement)
            {
                player.inputs.LockMovementDirection();
            }

            // 墙面方向跳跃
            player.DirectionalJump(
                player.transform.forward,
                player.stats.current.wallJumpHeight,
                player.stats.current.wallJumpDistance
            );

            // 跳跃后切换到下落状态
            player.states.Change<FallPlayerState>();
        }
    }

    /// <summary>
    /// 碰撞处理
    /// - 墙面拖拽状态不处理碰撞
    /// </summary>
    public override void OnContact(Player player, Collider other) { }
}
