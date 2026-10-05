using UnityEngine;

/// <summary>
/// 玩家死亡状态
/// - 进入和退出时没有额外逻辑
/// - 在该状态下，玩家仍会受到重力、摩擦力，并保持贴地
/// - 一般用于表现角色死亡后的物理行为（例如尸体倒下）
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Die Player State")]
public class DiePlayerState : PlayerState
{
    /// <summary>
    /// 进入死亡状态时调用
    /// （此处留空，可用于播放死亡动画、音效、触发事件等）
    /// </summary>
    protected override void OnEnter(Player player) { }

    /// <summary>
    /// 退出死亡状态时调用
    /// （此处留空，一般角色死亡不会主动退出该状态）
    /// </summary>
    protected override void OnExit(Player player) { }

    /// <summary>
    /// 死亡状态下每帧调用
    /// - Gravity：应用重力（尸体自然下落）
    /// - Friction：应用摩擦力（避免无限滑动）
    /// - SnapToGround：保持贴合地面（防止悬空）
    /// </summary>
    protected override void OnStep(Player player)
    {
        player.Gravity();      // 受重力影响
        player.Friction();     // 受到摩擦力
        player.SnapToGround(); // 紧贴地面
    }

    /// <summary>
    /// 碰撞检测逻辑（此处留空）
    /// 死亡状态通常不会与物体交互
    /// </summary>
    public override void OnContact(Player player, Collider other) { }
}
