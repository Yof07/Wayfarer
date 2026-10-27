using UnityEngine;
using UnityEngine.Splines;


/// <summary>
/// 玩家滑轨磨轨状态
/// - 玩家沿着轨道进行磨轨动作
/// - 支持前进/倒退、刹车和磨轨冲刺
/// </summary>
public class RailGrindPlayerState : PlayerState
{
    // 玩家是否沿轨道反向移动
    protected bool m_backwards;

    // 玩家当前沿轨道速度
    protected float m_speed;

    // 上一次磨轨冲刺时间
    protected float m_lastDahTime;

    /// <summary>
    /// 进入磨轨状态
    /// - 确定玩家在轨道上的位置和方向
    /// - 设置初始速度
    /// - 使用自定义碰撞
    /// </summary>
    protected override void OnEnter(Player player)
    {
        Evaluate(player, out var point, out var forward, out var upward, out _);
        UpdatePosition(player, point, upward);

        // 判断玩家移动方向是否与轨道正向相反
        m_backwards = Vector3.Dot(player.transform.forward, forward) < 0;

        // 初始速度为玩家当前速度或最小初始速度
        m_speed = Mathf.Max(player.lateralVelocity.magnitude,
            player.stats.current.minGrindInitialSpeed);

        // 清空玩家其他速度
        player.velocity = Vector3.zero;

        // 启用自定义碰撞
        player.UseCustomCollision(player.stats.current.useCustomCollision);
    }

    /// <summary>
    /// 离开磨轨状态
    /// - 退出轨道
    /// - 关闭自定义碰撞
    /// </summary>
    protected override void OnExit(Player player)
    {
        player.ExitRail();
        player.UseCustomCollision(false);
    }

    /// <summary>
    /// 每帧更新磨轨逻辑
    /// - 支持跳跃
    /// - 沿轨道移动、旋转
    /// - 考虑坡度影响速度
    /// - 刹车和冲刺处理
    /// </summary>
    protected override void OnStep(Player player)
    {
        player.Jump();

        if (player.onRails)
        {
            // 获取轨道当前位置、方向和上向量
            Evaluate(player, out var point, out var forward, out var upward, out var t);

            // 移动方向
            var direction = m_backwards ? -forward : forward;

            // 计算坡度对速度的影响
            var factor = Vector3.Dot(Vector3.up, direction);
            var multiplier = factor <= 0
                ? player.stats.current.slopeDownwardForce
                : player.stats.current.slopeUpwardForce;

            // 刹车处理
            HandleDeceleration(player);

            // 冲刺处理
            HandleDash(player);

            // 应用坡度影响
            if (player.stats.current.applyGrindingSlopeFactor)
                m_speed -= factor * multiplier * Time.deltaTime;

            // 限制速度范围
            m_speed = Mathf.Clamp(m_speed,
                player.stats.current.minGrindSpeed,
                player.stats.current.grindTopSpeed);

            // 旋转玩家贴合轨道
            Rotate(player, direction, upward);

            // 更新玩家速度
            player.velocity = direction * m_speed;

            // 如果轨道是闭合的，或在非终点范围内，持续更新位置
            if (player.rails.Spline.Closed || (t > 0 && t < 0.9f))
                UpdatePosition(player, point, upward);
        }
        else
        {
            // 离开轨道 → 下落状态
            player.states.Change<FallPlayerState>();
        }
    }

    /// <summary>
    /// 碰撞逻辑（磨轨状态通常不处理碰撞）
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
    }

    /// <summary>
    /// 计算玩家在轨道上的位置、前向和上向量
    /// </summary>
    protected virtual void Evaluate(Player player, out Vector3 point,
        out Vector3 forward, out Vector3 upward, out float t)
    {
        var origin = player.rails.transform.InverseTransformPoint(player.transform.position);

        // 获取玩家最近轨道点及t值（归一化位置）
        SplineUtility.GetNearestPoint(player.rails.Spline, origin, out var nearest, out t);

        point = player.rails.transform.TransformPoint(nearest);
        forward = Vector3.Normalize(player.rails.EvaluateTangent(t));
        upward = Vector3.Normalize(player.rails.EvaluateUpVector(t));
    }

    /// <summary>
    /// 刹车处理逻辑
    /// </summary>
    protected virtual void HandleDeceleration(Player player)
    {
        if (player.stats.current.canGrindBrake && player.inputs.GetGrindBrake())
        {
            var decelerationDelta = player.stats.current.grindBrakeDeceleration * Time.deltaTime;
            m_speed = Mathf.MoveTowards(m_speed, 0, decelerationDelta);
        }
    }

    /// <summary>
    /// 磨轨冲刺逻辑
    /// </summary>
    protected virtual void HandleDash(Player player)
    {
        if (player.stats.current.canGrindDash &&
            player.inputs.GetDashDown() &&
            Time.time >= m_lastDahTime + player.stats.current.grindDashCoolDown)
        {
            m_lastDahTime = Time.time;
            m_speed = player.stats.current.grindDashForce;
            player.playerEvents.OnDashStarted.Invoke();
        }
    }

    /// <summary>
    /// 更新玩家在轨道上的位置
    /// </summary>
    protected virtual void UpdatePosition(Player player, Vector3 point, Vector3 upward) =>
        player.transform.position = point + upward * GetDistanceToRail(player);

    /// <summary>
    /// 根据轨道方向和上向量旋转玩家
    /// </summary>
    protected virtual void Rotate(Player player, Vector3 forward, Vector3 upward)
    {
        if (forward != Vector3.zero)
            player.transform.rotation = Quaternion
                .LookRotation(forward, player.transform.up);

        player.transform.rotation = Quaternion
            .FromToRotation(player.transform.up, upward) * player.transform.rotation;
    }

    /// <summary>
    /// 获取玩家到轨道的垂直距离（贴合高度）
    /// </summary>
    protected virtual float GetDistanceToRail(Player player) =>
        player.originalHeight * 0.5f + player.stats.current.grindRadiusOffset;
}
