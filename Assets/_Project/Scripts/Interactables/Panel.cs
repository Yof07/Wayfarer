using UnityEngine;
using UnityEngine.Events;

// 组件要求：必须挂载 Collider 和 AudioSource
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Panel")]
public class Panel : MonoBehaviour, IEntityContact
{
    // 是否在触发后自动切换状态（松开后是否自动关闭）
    public bool autoToggle;
    
    // 本身的 Collider
    protected Collider m_collider;
    
    // 音频播放组件
    protected AudioSource m_audio;
    
    /// <summary>
    /// 当前 Panel 是否处于激活状态
    /// </summary>
    public bool activated { get; protected set; }
    
    
    // 激活时播放的音效
    public AudioClip activateClip;

    // 取消激活时播放的音效
    public AudioClip deactivateClip;
    
    /// <summary>
    /// 当 Panel 被激活时触发的事件（可在 Inspector 中绑定）
    /// </summary>
    public UnityEvent OnActivate;

    /// <summary>
    /// 当 Panel 被取消激活时触发的事件
    /// </summary>
    public UnityEvent OnDeactivate;
    
    // 是否要求踩踏才能触发（需要实体从上方踩下）
    public bool requireStomp;

    // 是否只允许玩家触发（而不是敌人或其他物体）
    public bool requirePlayer;
    
    // 被实体（如玩家）激活时的 Collider
    protected Collider m_entityActivator;

    // 被非玩家/实体（如箱子、敌人）激活时的 Collider
    protected Collider m_otherActivator;
    
    /// <summary>
    /// 取消激活面板
    /// </summary>
    public virtual void Deactivate()
    {
        // 只有在激活状态时才能取消
        if (activated)
        {
            // 播放取消音效
            if (deactivateClip)
            {
                m_audio.PlayOneShot(deactivateClip);
            }

            // 设置状态为未激活，并触发事件
            activated = false;
            OnDeactivate?.Invoke();
        }
    }
    
    /// <summary>
    /// 当实体（如玩家或敌人）接触 Panel 时调用
    /// </summary>
    public void OnEntityContact(EntityBase entity)
    {
        // 条件：实体必须是下落或静止在上面（y 速度 <= 0），
        // 且接触点在 Panel 顶部
        if (entity.velocity.y <= 0 && entity.IsPointUnderStep(m_collider.bounds.max))
        {
            // 如果要求是玩家，则必须是 Player
            // 如果要求踩踏，则必须是处于 Stomp 状态的 Player
            if ((!requirePlayer || entity is Player) &&
                (!requireStomp || (entity as Player).states.IsCurrentOfType(typeof(StompPlayerState))))
            {
                // 记录该实体的 Collider 作为触发器
                m_entityActivator = entity.controller;
            }
        }
    }
    
    /// <summary>
    /// 当非实体物体与面板保持碰撞时调用
    /// </summary>
    protected virtual void OnCollisionStay(Collision collision)
    {
        // 如果没有特别要求玩家或踩踏，那么允许普通物体触发
        // 但忽略玩家本身（防止重复逻辑）
        if (!(requirePlayer || requireStomp) && !collision.collider.CompareTag(GameTags.Player))
        {
            m_otherActivator = collision.collider;
        }
    }

    protected virtual void Update()
    {
        // 只要有触发器存在（实体或其他物体）
        if (m_entityActivator || m_otherActivator)
        {
            // 计算碰撞体的范围
            var center = m_collider.bounds.center;
            var contactOffset = Physics.defaultContactOffset + 0.1f; // 避免浮点误差
            var size = m_collider.bounds.size + Vector3.up * contactOffset;
            var bounds = new Bounds(center, size);
            
            // 检测实体是否还在接触区域内
            var intersectsEntity = m_entityActivator && bounds.Intersects(m_entityActivator.bounds);

            // 检测非实体物体是否还在接触区域内
            var intersectsOther = m_otherActivator && bounds.Intersects(m_otherActivator.bounds);
            
            // 如果仍然有接触，保持激活
            if (intersectsEntity || intersectsOther)
            {
                Activate();
            }
            else
            {
                // 如果不再接触，清空触发器引用
                m_entityActivator = intersectsEntity ? m_entityActivator : null;
                m_otherActivator = intersectsOther ? m_otherActivator : null;

                // 如果启用自动切换，松开后自动关闭
                if (autoToggle)
                {
                    Deactivate();
                }
            }
        }
    }

    /// <summary>
    /// 激活面板
    /// </summary>
    public virtual void Activate()
    {
        // 只有在未激活时才能激活
        if (!activated)
        {
            // 播放激活音效
            if (activateClip)
            {
                m_audio.PlayOneShot(activateClip);
            }

            // 设置状态为激活，并触发事件
            activated = true;
            OnActivate?.Invoke();
        }
    }
    
    protected virtual void Start()
    {
        // 设置 Tag，便于识别
        gameObject.tag = GameTags.Panel;

        // 获取自身组件
        m_collider = GetComponent<Collider>();
        m_audio = GetComponent<AudioSource>();
    }
    
}