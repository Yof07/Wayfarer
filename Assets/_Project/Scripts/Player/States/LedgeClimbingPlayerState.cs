using System.Collections;
using UnityEngine;

/// <summary>
/// 玩家抓悬挂并爬升状态
/// - 当玩家抓住悬挂物时进入该状态
/// - 通过协程控制角色从悬挂位置平滑爬到顶端
/// - 爬升完成后自动切换到 Idle 状态
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Ledge Climbing Player State")]
public class LedgeClimbingPlayerState : PlayerState
{
    // 协程引用，用于控制爬升动画
    protected IEnumerator m_routine;

    /// <summary>
    /// 进入抓悬挂状态
    /// - 启动爬升协程
    /// </summary>
    protected override void OnEnter(Player player)
    {
        m_routine = SetPositionRoutine(player);  // 创建协程
        player.StartCoroutine(m_routine);        // 启动协程
    }

    /// <summary>
    /// 离开抓悬挂状态
    /// - 重置皮肤父对象
    /// - 停止爬升协程
    /// </summary>
    protected override void OnExit(Player player)
    {
        player.ResetSkinParent();       // 重置皮肤父对象
        player.StopCoroutine(m_routine); // 停止协程
    }

    /// <summary>
    /// 控制玩家爬升的协程
    /// - 分两段移动：
    ///   1. 垂直上升到悬挂顶端
    ///   2. 水平移动到顶端安全位置
    /// </summary>
    protected virtual IEnumerator SetPositionRoutine(Player player)
    {
        var elapsedTime = 0f;
        var totalDuration = player.stats.current.ledgeClimbingDuration; // 爬升总时长
        var halfDuration = totalDuration / 2f;                           // 分段时间

        var initialPosition = player.transform.localPosition;            // 初始位置
        var targetVerticalPosition = player.transform.position + Vector3.up * (player.height + Physics.defaultContactOffset); // 垂直目标位置
        var targetLateralPosition = targetVerticalPosition + player.transform.forward * player.radius * 2f;

        // 如果玩家有父对象（如挂在移动平台上），转换到父对象局部坐标
        if (player.transform.parent != null)
        {
            targetVerticalPosition = player.transform.parent.InverseTransformPoint(targetVerticalPosition);
            targetLateralPosition = player.transform.parent.InverseTransformPoint(targetLateralPosition);
        }

        // 设置皮肤父对象，确保模型位置正确
        player.SetSkinParent(player.transform.parent);
        player.skin.position += player.transform.rotation * player.stats.current.ledgeClimbingSkinOffset;

        // 第一段：垂直上升
        while (elapsedTime <= halfDuration)
        {
            elapsedTime += Time.deltaTime;
            player.transform.localPosition = Vector3.Lerp(initialPosition, targetVerticalPosition, elapsedTime / halfDuration);
            yield return null;
        }

        elapsedTime = 0;
        player.transform.localPosition = targetVerticalPosition;

        // 第二段：水平移动到顶端位置
        while (elapsedTime <= halfDuration)
        {
            elapsedTime += Time.deltaTime;
            player.transform.localPosition = Vector3.Lerp(targetVerticalPosition, targetLateralPosition, elapsedTime / halfDuration);
            yield return null;
        }

        // 确保最终位置准确
        player.transform.localPosition = targetLateralPosition;
        // 爬升完成，切换到空闲状态
        player.states.Change<IdlePlayerState>();
    }

    /// <summary>
    /// 每帧更新抓悬挂状态逻辑
    /// - 爬升通过协程完成，这里无需额外逻辑
    /// </summary>
    protected override void OnStep(Player player) { }

    /// <summary>
    /// 碰撞处理逻辑
    /// - 抓悬挂状态通常不需要处理碰撞
    /// </summary>
    public override void OnContact(Player player, Collider other) { }
}
