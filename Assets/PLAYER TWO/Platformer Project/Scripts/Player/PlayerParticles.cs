using UnityEngine;

/// <summary>
/// 玩家粒子效果控制器
/// - 管理玩家在不同状态下（行走、落地、受伤、冲刺、滑轨等）的粒子特效播放。
/// - 通过监听 Player 和 Entity 的事件来触发对应的粒子效果。
/// </summary>
[RequireComponent(typeof(Player))] // 要求挂载的物体必须有 Player 组件
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Particles")]
public class PlayerParticles : MonoBehaviour
{
    [Header("速度阈值设置")]
    public float walkDustMinSpeed = 3.5f;      // 行走扬尘效果的最小速度阈值
    public float landingParticleMinSpeed = 5f; // 触发落地粒子特效的最小纵向速度阈值

    [Header("粒子特效引用")]
    public ParticleSystem walkDust;     // 行走扬尘
    public ParticleSystem landDust;     // 落地尘土
    public ParticleSystem hurtDust;     // 受伤尘土
    public ParticleSystem dashDust;     // 冲刺初始特效
    public ParticleSystem speedTrails;  // 冲刺速度残影
    public ParticleSystem grindTrails;  // 滑轨火花特效

    protected Player m_player; // 当前绑定的 Player 引用

    /// <summary>
    /// 初始化，绑定玩家事件回调。
    /// </summary>
    protected virtual void Start()
    {
        m_player = GetComponent<Player>();

        // 绑定落地事件 -> 播放落地尘土
        m_player.entityEvents.OnGroundEnter.AddListener(HandleLandParticle);
        // 绑定受伤事件 -> 播放受伤粒子
        m_player.playerEvents.OnHurt.AddListener(HandleHurtParticle);
        // 绑定冲刺开始事件 -> 播放冲刺特效
        m_player.playerEvents.OnDashStarted.AddListener(OnDashStarted);
        // 绑定冲刺结束事件 -> 停止速度残影并清理
        m_player.playerEvents.OnDashEnded.AddListener(() => Stop(speedTrails, true));
    }

    /// <summary>
    /// 每帧更新（LateUpdate 后）
    /// - 检查并处理行走尘土与滑轨火花粒子。
    /// </summary>
    protected virtual void Update()
    {
        HandleWalkParticle();
        HandleRailParticle();
    }

    /// <summary>
    /// 处理滑轨粒子效果
    /// - 在滑轨时播放火花残影，否则停止并清理。
    /// </summary>
    protected virtual void HandleRailParticle()
    {
        if (m_player.onRails)
            Play(grindTrails);
        else
            Stop(grindTrails, true);
    }

    /// <summary>
    /// 处理行走时的尘土粒子
    /// - 条件：必须在地面、非铁轨、非水面
    /// - 当水平速度大于阈值时触发，否则停止。
    /// </summary>
    protected virtual void HandleWalkParticle()
    {
        if (m_player.isGrounded && !m_player.onRails && !m_player.onWater)
        {
            if (m_player.lateralVelocity.magnitude > walkDustMinSpeed)
            {
                Play(walkDust);
            }
            else
            {
                Stop(walkDust);
            }
        }
        else
        {
            Stop(walkDust);
        }
    }

    /// <summary>
    /// 处理落地粒子效果
    /// - 条件：非水面落地，且纵向速度大于阈值。
    /// </summary>
    protected virtual void HandleLandParticle()
    {
        if (!m_player.onWater &&
            Mathf.Abs(m_player.velocity.y) >= landingParticleMinSpeed)
        {
            Play(landDust);
        }
    }

    /// <summary>
    /// 处理受伤粒子效果（如被攻击击中时触发）。
    /// </summary>
    protected virtual void HandleHurtParticle() => Play(hurtDust);


    /// <summary>
    /// 冲刺开始时触发的特效
    /// - 播放冲刺爆发和速度残影。
    /// </summary>
    protected virtual void OnDashStarted()
    {
        Play(dashDust);
        Play(speedTrails);
    }

    /// <summary>
    /// 停止指定的粒子效果
    /// - 可选择是否清理已有粒子（true 表示立即清除，false 表示自然消散）。
    /// </summary>
    public virtual void Stop(ParticleSystem particle, bool clear = false)
    {
        if (particle.isPlaying)
        {
            // 根据 clear 参数决定是只停止发射还是清空已存在的粒子
            var mode = clear ? ParticleSystemStopBehavior.StopEmittingAndClear :
                ParticleSystemStopBehavior.StopEmitting;
            particle.Stop(true, mode);
        }
    }

    /// <summary>
    /// 播放指定的粒子效果
    /// - 如果粒子当前未播放，则调用 Play()。
    /// </summary>
    public virtual void Play(ParticleSystem particle)
    {
        if (!particle.isPlaying)
        {
            particle.Play();
        }
    }
}
