using UnityEngine;


/// <summary>
/// 玩家重击（Stomp）状态
/// - 玩家在空中蓄力然后快速下落
/// - 可触发空中计时、下落力、落地事件及落地反弹
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Stomp Player State")]
public class StompPlayerState : PlayerState
{
    // 空中计时器，用于计算蓄力时间
    protected float m_airTimer;

    // 落地计时器，用于控制落地停留时间
    protected float m_groundTimer;

    // 玩家是否开始下落
    protected bool m_falling;

    // 玩家是否已经落地
    protected bool m_landed;

    /// <summary>
    /// 进入重击状态
    /// - 初始化计时器与状态
    /// - 停止玩家速度
    /// - 触发重击开始事件
    /// </summary>
    protected override void OnEnter(Player player)
    {
        m_landed = m_falling = false;
        m_airTimer = m_groundTimer = 0;
        player.velocity = Vector3.zero;
        player.playerEvents.OnStompStarted?.Invoke();
    }

    /// <summary>
    /// 离开重击状态
    /// - 触发重击结束事件
    /// </summary>
    protected override void OnExit(Player player)
    {
        player.playerEvents.OnStompEnding?.Invoke();
    }

    /// <summary>
    /// 每帧更新重击状态逻辑
    /// - 空中计时，达到设定时间后开始下落
    /// - 施加向下的重击力
    /// - 处理落地事件及落地停留时间
    /// - 落地后可触发小跳反弹
    /// </summary>
    protected override void OnStep(Player player)
    {
        if (!m_falling)
        {
            // 空中计时
            m_airTimer += Time.deltaTime;

            // 空中蓄力时间结束，开始下落
            if (m_airTimer >= player.stats.current.stompAirTime)
            {
                m_falling = true;
                player.playerEvents.OnStompFalling.Invoke();
            }
        }
        else
        {
            // 下落阶段施加向下力
            player.verticalVelocity += Vector3.down * player.stats.current.stompDownwardForce;
        }

        // 玩家落地处理
        if (player.isGrounded)
        {
            if (!m_landed)
            {
                // 第一次落地触发事件
                m_landed = true;
                player.playerEvents.OnStompLanding?.Invoke();
            }

            if (m_groundTimer >= player.stats.current.stompGroundTime)
            {
                // 落地停留时间结束 → 小跳反弹并切换到下落状态
                player.verticalVelocity = Vector3.up * player.stats.current.stompGroundLeapHeight;
                player.states.Change<FallPlayerState>();
            }
            else
            {
                // 继续增加落地计时
                m_groundTimer += Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 碰撞处理
    /// - 重击状态下不处理碰撞
    /// </summary>
    public override void OnContact(Player player, Collider other)
    {
    }
}
