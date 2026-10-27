using UnityEngine;

// 依赖要求：挂载该脚本的物体必须有 Collider 组件
[RequireComponent(typeof(Collider))]
// 在 Unity 组件菜单中为该类添加路径，方便在编辑器中添加
[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity Hitbox")]
public class EntityHitbox : MonoBehaviour
{

    [Header("Attack Settings")] // 在 Inspector 面板中分组显示：攻击相关设置
    public bool breakObjects;   // 是否可以击碎可破坏物体
    public int damage = 1;      // 攻击造成的伤害值

    [Header("Rebound Settings")] // 分组显示：反弹相关设置
    public bool rebound;        // 是否启用反弹效果
    public float reboundMinForce = 10f; // 反弹最小力度
    public float reboundMaxForce = 25f; // 反弹最大力度

    [Header("Push Back Settings")] // 分组显示：击退相关设置
    public bool pushBack;        // 是否启用击退效果
    public float pushBackMinMagnitude = 5f;  // 击退最小速度
    public float pushBackMaxMagnitude = 10f; // 击退最大速度

    protected EntityBase m_entity; // 缓存自身实体的引用（EntityBase 脚本）
    protected Collider m_collider; // 缓存自身碰撞体的引用

    /// <summary>
    /// Unity 生命周期方法：脚本启用时调用一次
    /// </summary>
    protected virtual void Start()
    {
        InitializeEntity();  // 初始化实体引用
        InitializeCollider(); // 初始化碰撞器
    }

    /// <summary>
    /// 初始化实体引用（寻找父物体上的 EntityBase）
    /// </summary>
    protected virtual void InitializeEntity()
    {
        if (!m_entity) // 如果还没缓存实体
        {
            // 从父对象中获取 EntityBase（假设 hitbox 可能是角色的子物体）
            m_entity = GetComponentInParent<EntityBase>();
        }
    }

    /// <summary>
    /// 初始化碰撞器
    /// </summary>
    protected virtual void InitializeCollider()
    {
        // 获取 Collider 组件
        m_collider = GetComponent<Collider>();
        // 将碰撞器设置为触发器模式（不产生物理碰撞，仅检测触发事件）
        m_collider.isTrigger = true;
    }

    /// <summary>
    /// Unity 生命周期方法：触发碰撞时调用
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);       // 基本碰撞处理
        HandleCustomCollision(other); // 子类扩展处理
    }

    /// <summary>
    /// 留给子类自定义的额外碰撞处理逻辑
    /// </summary>
    protected virtual void HandleCustomCollision(Collider other) { }

    /// <summary>
    /// 处理与其他碰撞体的触发碰撞逻辑
    /// </summary>
    protected virtual void HandleCollision(Collider other)
    {
        // 排除与自身控制器的碰撞（防止自己打到自己）
        if (other != m_entity.controller)
        {
            // 如果对方是一个实体（EntityBase）
            if (other.TryGetComponent(out EntityBase target))
            {
                HandleEntityAttack(target); // 对实体造成伤害
                HandleRebound();            // 执行反弹效果
                HandlePushBack();           // 执行击退效果
            }
            // 如果对方是可破坏物体
            else if (other.TryGetComponent(out Breakable breakable))
            {
                HandleBreakableObject(breakable); // 尝试破坏它
            }
        }
    }

    /// <summary>
    /// 处理可破坏物体的逻辑
    /// </summary>
    protected virtual void HandleBreakableObject(Breakable breakable)
    {
        // 如果允许破坏物体
        if (breakObjects)
        {
            breakable.Break(); // 调用 Break 方法销毁或打碎物体
        }
    }

    /// <summary>
    /// 对其他实体造成伤害
    /// </summary>
    protected virtual void HandleEntityAttack(EntityBase other)
    {
        // 调用对方的 ApplyDamage 方法，传入伤害值和攻击位置
        other.ApplyDamage(damage, transform.position);
    }

    /// <summary>
    /// 处理反弹逻辑
    /// </summary>
    protected virtual void HandleRebound()
    {
        if (rebound)
        {
            // 根据当前垂直速度的反方向作为反弹力度（只取 y 方向）
            var force = -m_entity.velocity.y;
            // 限制反弹力度在设定范围内
            force = Mathf.Clamp(force, reboundMinForce, reboundMaxForce);
            // 设置垂直速度，让实体向上反弹
            m_entity.verticalVelocity = Vector3.up * force;
        }
    }

    /// <summary>
    /// 处理击退逻辑
    /// </summary>
    protected virtual void HandlePushBack()
    {
        if (pushBack)
        {
            // 根据当前横向速度大小计算击退力度
            var force = m_entity.lateralVelocity.magnitude;
            // 限制击退力度
            force = Mathf.Clamp(force, pushBackMinMagnitude, pushBackMaxMagnitude);
            // 设置横向速度，使实体往后退
            m_entity.lateralVelocity = -transform.forward * force;
        }
    }
}
