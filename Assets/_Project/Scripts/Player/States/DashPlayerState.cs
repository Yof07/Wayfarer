using UnityEngine;


/// <summary>
/// 玩家冲刺状态
/// - 进入时会给玩家一个强烈的向前推力
/// - 在持续时间内维持冲刺运动
/// - 结束时会根据是否在地面，切换到 Walk 或 Fall 状态
/// </summary>
public class DashPlayerState : PlayerState
{
    /// <summary>
    /// 进入冲刺状态时调用
    /// - 垂直速度清零（防止下落或跳跃干扰）
    /// - 设置水平速度为“角色前方 * 冲刺力度”
    /// - 触发冲刺开始事件（可用于播放音效、特效等）
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero; // 清空垂直速度
        player.lateralVelocity = player.transform.forward * player.stats.current.dashForce;
        player.playerEvents.OnDashStarted.Invoke(); // 调用事件：冲刺开始
    }

    /// <summary>
    /// 离开冲刺状态时调用
    /// - 限制当前水平速度不超过最大速度 topSpeed
    /// - 触发冲刺结束事件
    /// </summary>
    protected override void OnExit(Player player)
    {
        // 限制水平速度在最大速度范围内
        player.lateralVelocity = Vector3.ClampMagnitude(
            player.lateralVelocity, player.stats.current.topSpeed);

        player.playerEvents.OnDashEnded.Invoke(); // 调用事件：冲刺结束
    }

    /// <summary>
    /// 每帧更新冲刺逻辑
    /// - 允许在冲刺过程中跳跃
    /// - 如果超过冲刺持续时间：
    ///   - 在地面 → 切换到 Walk 状态
    ///   - 在空中 → 切换到 Fall 状态
    /// </summary>
    protected override void OnStep(Player player)
    {
        player.Jump(); // 冲刺中仍然可以跳跃

        // 判断是否超过冲刺持续时间
        if (timeSinceEntered > player.stats.current.dashDuration)
        {
            if (player.isGrounded)
                player.states.Change<WalkPlayerState>(); // 地面 → 走路
            else
                player.states.Change<FallPlayerState>(); // 空中 → 下落
        }
    }

    /// <summary>
    /// 碰撞检测逻辑
    /// - 冲刺时接触到物体的交互处理
    ///   1. 推动刚体（推开可物理交互的物体）
    ///   2. 墙面阻力（防止贴墙无限加速）
    ///   3. 抓杆逻辑（比如抓住攀爬杆/绳索）
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
        player.PushRigidbody(other);
        player.WallDrag(other);      // 墙面减速
        player.GrabPole(other);      // 抓杆
    }
}
