using UnityEngine;
using UnityEngine.Events;

// 要求该对象必须挂载 Collider 和 Rigidbody 组件
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Pickable")]
public class Pickable : MonoBehaviour, IEntityContact
{
    [Header("General Settings")]
    public Vector3 offset;                 // 物体被拾取后在玩家手中的位置偏移
    public float releaseOffset = 0.5f;     // 物体被释放时向前偏移的距离

    [Header("Respawn Settings")]
    public bool autoRespawn;               // 是否开启自动重生
    public bool respawnOnHitHazards;       // 碰到危险物（Hazard）是否重生
    public float respawnHeightLimit = -100;// 超过某个高度（掉落过深）是否重生

    [Header("Attack Settings")]
    public bool attackEnemies = true;      // 是否可以攻击敌人
    public int damage = 1;                 // 对敌人造成的伤害值
    public float minDamageSpeed = 5f;      // 物体速度超过这个阈值时才会造成伤害

    [Space(15)]

    /// <summary>
    /// 当物体被拾取时触发
    /// </summary>
    public UnityEvent onPicked;

    /// <summary>
    /// 当物体被释放时触发
    /// </summary>
    public UnityEvent onReleased;

    /// <summary>
    /// 当物体被重生时触发
    /// </summary>
    public UnityEvent onRespawn;

    protected Collider m_collider;              // 缓存物体的碰撞体
    protected Rigidbody m_rigidBody;            // 缓存物体的刚体

    protected Vector3 m_initialPosition;        // 初始位置（用于重生）
    protected Quaternion m_initialRotation;     // 初始旋转（用于重生）
    protected Transform m_initialParent;        // 初始父物体（用于重生时还原层级）

    protected RigidbodyInterpolation m_interpolation; // 保存插值模式（被拾取时关闭）

    public bool beingHold { get; protected set; } // 是否当前正被玩家持有

    /// <summary>
    /// 拾取物体
    /// </summary>
    /// <param name="slot">玩家的拾取槽（Transform）</param>
    public virtual void PickUp(Transform slot)
    {
        if (!beingHold)
        {
            beingHold = true;
            transform.parent = slot;                          // 物体挂到拾取槽下
            transform.localPosition = Vector3.zero + offset;  // 设置偏移位置
            transform.localRotation = Quaternion.identity;    // 重置旋转
            m_rigidBody.isKinematic = true;                   // 关闭物理模拟
            m_collider.isTrigger = true;                      // 设为触发器避免碰撞
            m_interpolation = m_rigidBody.interpolation;      // 保存插值模式
            m_rigidBody.interpolation = RigidbodyInterpolation.None; // 临时关闭插值
            onPicked?.Invoke();                               // 触发拾取事件
        }
    }

    /// <summary>
    /// 释放物体
    /// </summary>
    /// <param name="direction">释放方向</param>
    /// <param name="force">释放的力度</param>
    public virtual void Release(Vector3 direction, float force)
    {
        if (beingHold)
        {
            transform.parent = null;                          // 脱离玩家父物体
            transform.position += direction * releaseOffset;  // 稍微往释放方向偏移
            m_collider.isTrigger = m_rigidBody.isKinematic = beingHold = false; // 恢复物理
            m_rigidBody.interpolation = m_interpolation;      // 恢复插值模式
            m_rigidBody.velocity = direction * force;         // 赋予速度
            onReleased?.Invoke();                             // 触发释放事件
        }
    }

    /// <summary>
    /// 当物体与实体接触时触发
    /// </summary>
    public void OnEntityContact(EntityBase entity)
    {
        // 如果允许攻击敌人，并且接触对象是敌人，且物体速度大于阈值 -> 造成伤害
        if (attackEnemies && entity is Enemy &&
            m_rigidBody.velocity.magnitude > minDamageSpeed)
        {
            entity.ApplyDamage(damage, transform.position);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_initialPosition = transform.localPosition;   // 记录初始本地坐标
        m_initialRotation = transform.localRotation;   // 记录初始本地旋转
        m_initialParent = transform.parent;            // 记录初始父对象
    }

    /// <summary>
    /// 每帧检测是否需要因为掉落过深而自动重生
    /// </summary>
    protected virtual void Update()
    {
        if (autoRespawn && transform.position.y <= respawnHeightLimit)
        {
            Respawn();
        }
    }

    /// <summary>
    /// 检查是否需要因为碰到危险物体而重生
    /// </summary>
    protected virtual void EvaluateHazardRespawn(Collider other)
    {
        if (autoRespawn && respawnOnHitHazards && other.CompareTag(GameTags.Hazard))
        {
            Respawn();
        }
    }

    /// <summary>
    /// 重生物体到初始位置
    /// </summary>
    public virtual void Respawn()
    {
        m_rigidBody.velocity = Vector3.zero;                       // 清除速度
        transform.parent = m_initialParent;                        // 恢复初始父物体
        transform.SetPositionAndRotation(m_initialPosition, m_initialRotation); // 恢复位置和旋转
        m_rigidBody.isKinematic = m_collider.isTrigger = beingHold = false;     // 恢复物理属性
        onRespawn?.Invoke();                                       // 触发重生事件
    }

    /// <summary>
    /// 碰到触发器时调用
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other) =>
        EvaluateHazardRespawn(other);

    /// <summary>
    /// 碰到碰撞体时调用
    /// </summary>
    protected virtual void OnCollisionEnter(Collision collision) =>
        EvaluateHazardRespawn(collision.collider);
}
