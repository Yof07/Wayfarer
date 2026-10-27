using System.Collections;
using UnityEngine;

// 要求此物体必须有 Collider 组件
[RequireComponent(typeof(Collider))]
// 在 Unity 的 AddComponent 菜单中显示路径
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Collectable")]
public class Collectable : MonoBehaviour
{
    [Header("General Settings")] // 常规设置
    public bool collectOnContact = true;   // 是否碰到玩家就直接收集
    public int times = 1;                  // 收集时调用次数（可以多次触发奖励/效果）
    public float ghostingDuration = 0.5f;  // “幽灵时间”，刚生成时短暂不可收集
    public GameObject display;             // 用于展示的模型/物体
    public AudioClip clip;                 // 收集音效
    public ParticleSystem particle;        // 收集时播放的粒子效果

    [Header("Visibility Settings")] // 可见性设置
    public bool hidden;                    // 是否初始隐藏（收集时才显示）
    public float quickShowHeight = 2f;     // 隐藏物体被展示时的上升高度
    public float quickShowDuration = 0.25f;// 上升过程持续时间
    public float hideDuration = 0.5f;      // 展示后再隐藏的等待时间

    [Header("Life Time")] // 生命周期
    public bool hasLifeTime;               // 是否有存活时间
    public float lifeTimeDuration = 5f;    // 存活时间（超过后自动消失）

    [Header("Physics Settings")] // 物理相关设置
    public bool usePhysics;                // 是否启用物理弹跳效果
    public float minForceToStopPhysics = 3f; // 小于此速度时停止物理效果
    public float collisionRadius = 0.5f;   // 碰撞检测的球体半径
    public float gravity = 15f;            // 重力加速度
    public float bounciness = 0.98f;       // 弹性系数
    public float maxBounceYVelocity = 10f; // 垂直方向最大反弹速度
    public bool randomizeInitialDirection = true; // 是否随机初始发射方向
    public Vector3 initialVelocity = new Vector3(0, 12, 0); // 初始速度
    public AudioClip collisionClip;        // 碰撞时的音效


    [Space(15)]

    // 内部状态
    protected bool m_vanished;             // 是否已经消失
    protected bool m_ghosting = true;      // 是否处于幽灵时间（不可收集）
    protected float m_elapsedLifeTime;     // 已经过的生命周期时间
    protected float m_elapsedGhostingTime; // 已经过的幽灵时间
    protected Vector3 m_velocity;          // 当前速度（物理模式下使用）

    // 常量（用于随机旋转初始方向）
    protected const int k_verticalMinRotation = 0;
    protected const int k_verticalMaxRotation = 30;
    protected const int k_horizontalMinRotation = 0;
    protected const int k_horizontalMaxRotation = 360;

    /// <summary>
    /// 当物品被收集时调用的事件。
    /// </summary>
    public PlayerEvent onCollect;

    // 组件缓存
    protected Collider m_collider;
    protected AudioSource m_audio;

    // 初始化音频组件
    protected virtual void InitializeAudio()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }

    // 初始化碰撞体（设置为触发器）
    protected virtual void InitializeCollider()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }

    // 初始化位置与旋转（物体独立存在，旋转归零）
    protected virtual void InitializeTransform()
    {
        transform.parent = null;
        transform.rotation = Quaternion.identity;
    }

    // 初始化显示状态
    protected virtual void InitializeDisplay()
    {
        display?.SetActive(!hidden);
    }

    // 初始化速度（可随机方向）
    protected virtual void InitializeVelocity()
    {
        var direction = initialVelocity.normalized;
        var force = initialVelocity.magnitude;

        if (randomizeInitialDirection)
        {
            var randomZ = Random.Range(k_verticalMinRotation, k_verticalMaxRotation);
            var randomY = Random.Range(k_horizontalMinRotation, k_horizontalMaxRotation);
            direction = Quaternion.Euler(0, 0, randomZ) * direction;
            direction = Quaternion.Euler(0, randomY, 0) * direction;
        }

        m_velocity = direction * force;
    }

    /// <summary>
    /// 消失：隐藏显示物体与碰撞体。
    /// </summary>
    public virtual void Vanish()
    {
        if (!m_vanished)
        {
            m_vanished = true;
            m_elapsedLifeTime = 0;
            display.SetActive(false);
            m_collider.enabled = false;
        }
    }

    /// <summary>
    /// 快速展示协程：隐藏状态下展示 → 升起 → 等待 → 回到原位 → 消失
    /// </summary>
    protected virtual IEnumerator QuickShowRoutine()
    {
        var elapsedTime = 0f;
        var initialPosition = transform.position;
        var targetPosition = initialPosition + Vector3.up * quickShowHeight;

        display.SetActive(true);
        m_collider.enabled = false;

        while (elapsedTime < quickShowDuration)
        {
            var t = elapsedTime / quickShowDuration;
            transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        yield return new WaitForSeconds(hideDuration);
        transform.position = initialPosition;
        Vanish();
    }

    /// <summary>
    /// 收集协程：播放音效，触发事件，可多次执行。
    /// </summary>
    protected virtual IEnumerator CollectRoutine(Player player)
    {
        for (int i = 0; i < times; i++)
        {
            m_audio.Stop();
            m_audio.PlayOneShot(clip);
            onCollect.Invoke(player);
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 触发收集逻辑。
    /// </summary>
    public virtual void Collect(Player player)
    {
        if (!m_vanished && !m_ghosting)
        {
            if (!hidden)
            {
                Vanish();

                if (particle != null)
                {
                    particle.Play();
                }
            }
            else
            {
                StartCoroutine(QuickShowRoutine());
            }

            StartCoroutine(CollectRoutine(player));
        }
    }


    // 触发器检测：如果设置了 collectOnContact，玩家碰到就收集
    protected virtual void OnTriggerStay(Collider other)
    {
        if (collectOnContact && other.CompareTag(GameTags.Player))
        {
            if (other.TryGetComponent<Player>(out var player))
            {
                Collect(player);
            }
        }
    }

    // 幽灵时间处理
    protected virtual void HandleGhosting()
    {
        if (m_ghosting)
        {
            m_elapsedGhostingTime += Time.deltaTime;

            if (m_elapsedGhostingTime >= ghostingDuration)
            {
                m_elapsedGhostingTime = 0;
                m_ghosting = false;
            }
        }
    }

    // 生命周期处理
    protected virtual void HandleLifeTime()
    {
        if (hasLifeTime)
        {
            m_elapsedLifeTime += Time.deltaTime;

            if (m_elapsedLifeTime >= lifeTimeDuration)
            {
                Vanish();
            }
        }
    }

    // 物理移动处理（重力）
    protected virtual void HandleMovement()
    {
        m_velocity.y -= gravity * Time.deltaTime;
    }

    // 碰撞检测与反弹
    protected virtual void HandleSweep()
    {
        var direction = m_velocity.normalized;
        var magnitude = m_velocity.magnitude;
        var distance = magnitude * Time.deltaTime;
        if (Physics.SphereCast(transform.position, collisionRadius, direction,
                out var hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag(GameTags.Player))
            {
                // 反弹方向 = 入射方向在法线上的反射
                var bounceDirection = Vector3.Reflect(direction, hit.normal);
                m_velocity = bounceDirection * magnitude * bounciness;
                m_velocity.y = Mathf.Min(m_velocity.y, maxBounceYVelocity);
                m_audio.Stop();
                m_audio.PlayOneShot(collisionClip);

                // 速度过小则停止物理模式
                if (m_velocity.y <= minForceToStopPhysics)
                    usePhysics = false;
            }
        }

        transform.position += m_velocity * Time.deltaTime;
    }

    protected virtual void Update()
    {
        if (!m_vanished)
        {
            HandleGhosting();
            HandleLifeTime();

            if (usePhysics)
            {
                HandleMovement();
                HandleSweep();
            }
        }
    }


    // Unity 生命周期函数
    protected virtual void Awake()
    {
        InitializeAudio();
        InitializeCollider();
        InitializeTransform();
        InitializeDisplay();
        InitializeVelocity();
    }
}
