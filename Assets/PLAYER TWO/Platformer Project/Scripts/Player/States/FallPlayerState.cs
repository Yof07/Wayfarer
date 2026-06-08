using UnityEngine;

public class FallPlayerState : PlayerState
{
    /// <summary>
    /// 进入下落状态时调用
    /// （此处没有额外逻辑，可扩展播放下落动画/音效）
    /// </summary>
    protected override void OnEnter(Player player)
    {
    }

    /// <summary>
    /// 离开下落状态时调用
    /// （此处没有额外逻辑）
    /// </summary>
    protected override void OnExit(Player player)
    {
    }

    /// <summary>
    /// 每帧更新下落逻辑
    /// </summary>
    protected override void OnStep(Player player)
    {
        player.Gravity();                      // 应用重力，让角色自然下落
        player.SnapToGround();                // 吸附地面，防止悬空抖动
        player.FaceDirectionSmooth(player.lateralVelocity); // 平滑转向，使角色朝向移动方向
        player.AccelerateToInputDirection();  // 根据玩家输入方向加速
        player.Jump();                        // 空中可跳跃
        // player.Spin();                        // 空中旋转动作
        // player.PickAndThrow();                // 空中拾取或投掷物体
        // player.Dash();                        // 空中冲刺
        // player.AirDive();                     // 空中俯冲动作
        // player.StompAttack();                 // 空中踩击攻击
        // player.Glide();                       // 滑翔动作
        // player.LedgeGrab();                   // 抓取悬挂
        
        // 如果落地 → 切换到 Idle 状态
        if (player.isGrounded)
        {
            player.states.Change<IdlePlayerState>();
        }
    }

    /// <summary>
    /// 碰撞检测逻辑
    /// - 下落状态与物体接触时：
    ///   1. 推动物理刚体
    ///   2. 墙面阻力处理
    ///   3. 抓杆逻辑
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
        // player.PushRigidbody(other);
        // player.WallDrag(other);       // 墙面摩擦减速
        // player.GrabPole(other);       // 抓住杆或绳索
    }
}
