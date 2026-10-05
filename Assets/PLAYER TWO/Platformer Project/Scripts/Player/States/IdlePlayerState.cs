
using UnityEngine;

public class IdlePlayerState : PlayerState
{
    /// <summary>
    /// 进入空闲状态时调用
    /// （此处留空，可用于播放空闲动画/音效）
    /// </summary>
    protected override void OnEnter(Player player) { }
    
    /// <summary>
    /// 离开空闲状态时调用
    /// （此处留空，可用于清理空闲状态效果）
    /// </summary>
    protected override void OnExit(Player player)
    {
    }
    
    /// <summary>
    /// 每帧更新空闲状态逻辑
    /// </summary>
    protected override void OnStep(Player player)
    {
        // 重力处理
        player.Gravity();
        // 贴地处理
        player.SnapToGround();
        // 跳跃处理
        player.Jump();
        // 下落处理
        player.Fall();
        // 旋转处理
        player.Spin();
        // 拾取/投掷处理
        player.PickAndThrow();
        
        var inputDirection = player.inputs.GetMovementDirection();
        Debug.Log($"Input Direction: {inputDirection}");
        // 如果有移动输入(平方和)或水平速度 > 0 → 切换到 Walk 状态
        if (inputDirection.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
        {
            player.states.Change<WalkPlayerState>();
        }
        // 如果按下下蹲/爬行 -> 切换到 Crouch 状态
        else if (player.inputs.GetCrouchAndCraw())
        {
            player.states.Change<CrouchPlayerState>();
        }
    }
    
    /// <summary>
    /// 碰撞检测逻辑
    /// - 空闲状态通常不需要额外碰撞处理
    /// </summary>
    public override void OnContact(Player player, Collider other) { }
}   
