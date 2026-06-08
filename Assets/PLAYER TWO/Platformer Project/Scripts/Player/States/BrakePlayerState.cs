using UnityEngine;


// 将该类添加到 Unity 的组件菜单，方便在 Inspector 里挂载
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Brake Player State")]
public class BrakePlayerState : PlayerState
{
    /// <summary>
    /// 进入刹车状态时调用（此处无额外逻辑）
    /// </summary>
    protected override void OnEnter(Player player)
    {
    }

    /// <summary>
    /// 离开刹车状态时调用（此处无额外逻辑）
    /// </summary>
    protected override void OnExit(Player player)
    {
    }

    /// <summary>
    /// 每帧更新时调用（刹车逻辑）
    /// </summary>
    protected override void OnStep(Player player)
    {
        player.Decelerate();
        // 贴地处理
        player.SnapToGround();
        // 跳跃处理
        player.Jump();
        // 下落处理
        player.Fall();
        
        // 如果玩家的水平速度为 0（完全停下来了）
        if (player.lateralVelocity.sqrMagnitude == 0)
        {
            // 状态切换为 Idle（待机状态）
            player.states.Change<IdlePlayerState>();
        }
        
    }
    
    /// <summary>
    /// 当处于刹车状态下发生碰撞时调用（此处无逻辑）
    /// </summary>
    public override void OnContact(Player player, Collider other) { }
}
