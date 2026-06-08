using UnityEngine;

public class CrawlingPlayerState : PlayerState
{
    /// <summary>
    /// 进入爬行状态时调用
    /// - 调整角色碰撞体高度为“蹲伏高度”
    /// </summary>
    protected override void OnEnter(Player player)
    {
        player.ResizeCollider(player.stats.current.crouchHeight);
    }

    /// <summary>
    /// 离开爬行状态时调用
    /// - 恢复角色碰撞体高度为原始站立高度
    /// </summary>
    protected override void OnExit(Player player)
    {
        player.ResizeCollider(player.originalHeight);
    }

    /// <summary>
    /// 每帧更新时调用（爬行逻辑）
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 基础运动逻辑
        player.Gravity();        // 应用重力
        player.SnapToGround();   // 吸附地面，避免悬空抖动
        player.Jump();           // 检查是否执行跳跃
        player.Fall();           // 检查是否进入下落状态

        // 获取输入方向（相对于摄像机的方向）
        var inputDirection = player.inputs.GetMovementCameraDirection();

        // 判断是否保持爬行状态：
        // 1. 玩家仍然按着“蹲伏/爬行”按键
        // 2. 或者角色当前不能站起来（头顶有障碍物）
        if (player.inputs.GetCrouchAndCraw() || !player.canStandUp)
        {
            // 如果有方向输入 → 爬行移动
            if (inputDirection.sqrMagnitude > 0)
            {
                // 执行爬行加速度移动
                player.CrawlingAccelerate(inputDirection);

                // 平滑转向，使角色朝向当前移动方向
                player.FaceDirectionSmooth(player.lateralVelocity);
            }
            else
            {
                // 没有输入时 → 逐渐减速
                player.Decelerate(player.stats.current.crawlingFriction);
            }
        }
        else
        {
            // 如果松开了爬行键，并且角色可以站起来 → 切换为 Idle 状态
            player.states.Change<IdlePlayerState>();
        }
    }

    /// <summary>
    /// 爬行状态下发生碰撞时调用（此处无逻辑）
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
        //player.WallDrag(other);   // 墙面摩擦减速
    }
}
