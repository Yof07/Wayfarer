using UnityEngine;

/// <summary>
/// 玩家状态：空中俯冲（Air Dive）。
///
/// - 进入该状态时，玩家会立即向前加速下冲。
/// - 在空中时受重力影响，并可与地形交互。
/// - 落地后会逐渐减速，如果完全停下则触发一个反弹跳跃，并切换到坠落状态。
/// - 若在空中接触到墙或杆，会进入相应的墙壁滑行或爬杆逻辑。
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Air Dive Player State")]
public class AirDivePlayerState : PlayerState
{
    /// <summary>
    /// 状态进入时调用：
    /// - 清空竖直速度（verticalVelocity = 0）。
    /// - 给予玩家一个沿前方方向的强大水平速度（模拟俯冲动作）。
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.verticalVelocity = Vector3.zero; // 进入俯冲 → 清空竖直速度
        player.lateralVelocity = player.transform.forward
                                 * player.stats.current.airDiveForwardForce; // 向前施加俯冲速度
    }

    /// <summary>
    /// 状态退出时调用，这里未做处理。
    /// </summary>
    protected override void OnExit(Player player) { }

    /// <summary>
    /// 状态更新（每帧执行）。
    /// - 应用重力 & 跳跃检测。
    /// - 如果开启坡度修正，则根据坡度添加额外推力。
    /// - 调整角色朝向以匹配移动方向。
    /// - 落地时，逐渐减速；完全停下后触发反弹跳跃，并切换到下落状态。
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 施加重力
        player.Gravity();
        // 检测跳跃输入（可在空中触发如多段跳）
        player.Jump();
        // 朝向移动方向
        player.FaceDirection(player.lateralVelocity);

        // 落地时的处理
        if (player.isGrounded)
        {
            // 获取玩家输入的方向（基于摄像机）
            var inputDirection = player.inputs.GetMovementCameraDirection();
            // 转换到局部坐标系
            var localInputDirection = player.transform.InverseTransformDirection(inputDirection);
            // 根据横向输入，计算旋转角度（模拟落地后的转向滑动）
            var rotation = localInputDirection.x
                           * player.stats.current.airDiveRotationSpeed
                           * Time.deltaTime;

            // 将旋转应用到当前的水平速度上
            player.lateralVelocity = Quaternion.Euler(0, rotation, 0) * player.lateralVelocity;

            // 普通地面时使用普通摩擦力减速
            player.Decelerate(player.stats.current.airDiveFriction);

            // 如果完全停下（速度为 0）
            if (player.lateralVelocity.sqrMagnitude == 0)
            {
                // 给玩家一个向上的反弹力
                player.verticalVelocity = Vector3.up * player.stats.current.airDiveGroundLeapHeight;
                // 切换到坠落状态
                player.states.Change<FallPlayerState>();
            }
        }
    }

    /// <summary>
    /// 与其他碰撞体接触时调用。
    /// - 如果玩家还在空中，检测是否可以进入墙壁滑行或爬杆状态。
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
        if (!player.isGrounded)
        {
            player.WallDrag(other); // 碰到墙壁 → 可能进入墙壁滑行
            player.GrabPole(other); // 碰到杆子 → 可能进入爬杆
        }
    }
}
