using UnityEngine;

/// <summary>
/// 玩家滑翔状态
/// - 在空中按住滑翔键进入
/// - 角色在空中缓慢下落，控制水平移动方向
/// - 松开滑翔键或落地时退出滑翔状态
/// </summary>
public class GlidingPlayerState : PlayerState
{
    /// <summary>
    /// 进入滑翔状态时调用
    /// - 垂直速度清零（防止下落初速度影响滑翔）
    /// - 触发滑翔开始事件（可播放音效/特效）
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero;  // 清空垂直速度
        player.playerEvents.OnGlidingStart.Invoke(); // 调用事件：滑翔开始
    }

    /// <summary>
    /// 离开滑翔状态时调用
    /// - 触发滑翔结束事件
    /// </summary>
    protected override void OnExit(Player player) =>
        player.playerEvents.OnGlidingStop.Invoke(); // 调用事件：滑翔结束

    /// <summary>
    /// 每帧更新滑翔逻辑
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 获取输入方向（相对于摄像机）
        var inputDirection = player.inputs.GetMovementCameraDirection();

        // 处理滑翔重力，使下落速度受限
        HandleGlidingGravity(player);

        // 角色面向水平移动方向
        player.FaceDirection(player.lateralVelocity);

        // 水平加速，考虑滑翔转向阻力和空气加速度
        player.Accelerate(inputDirection,
            player.stats.current.glidingTurningDrag,
            player.stats.current.airAcceleration,
            player.stats.current.topSpeed);

        player.LedgeGrab();

        // 如果落地 → 切换到 Idle 状态
        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }
        // 如果松开滑翔键 → 切换到下落状态
        else if (!player.inputs.GetGlide())
        {
            player.states.Change<FallPlayerState>();
        }
    }

    /// <summary>
    /// 碰撞处理逻辑
    /// - 滑翔时碰撞物体：
    ///   1. 墙面阻力处理
    ///   2. 抓杆逻辑
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
        player.WallDrag(other);   // 墙面摩擦减速
        player.GrabPole(other);   // 抓住杆或绳索
    }

    /// <summary>
    /// 处理滑翔重力
    /// - 角色在空中缓慢下落
    /// - 下落速度不会超过 glidingMaxFallSpeed
    /// </summary>
    protected virtual void HandleGlidingGravity(Player player)
    {
        var yVelocity = player.verticalVelocity.y;

        // 按滑翔重力计算速度
        yVelocity -= player.stats.current.glidingGravity * Time.deltaTime;

        // 限制最大下落速度
        yVelocity = Mathf.Max(yVelocity, -player.stats.current.glidingMaxFallSpeed);

        // 更新垂直速度
        player.verticalVelocity = new Vector3(0, yVelocity, 0);
    }
}
