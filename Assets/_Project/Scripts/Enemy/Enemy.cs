using UnityEngine;

// 限制此组件必须挂载的依赖组件
[RequireComponent(typeof(EnemyStatsManager))] // 敌人的属性管理器
[RequireComponent(typeof(EnemyStateManager))] // 敌人的状态机管理器
[RequireComponent(typeof(WaypointManager))] // 路径点管理器（巡逻用）
[RequireComponent(typeof(Health))] // 血量组件
[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy")] // 添加到组件菜单
public class Enemy : Entity<Enemy>
{

    // 敌人相关的事件（受伤、死亡、发现玩家等）
    public EnemyEvents enemyEvents;
    // 用于存储视野检测的碰撞体缓存（减少 GC）
    protected Collider[] m_sightOverlaps = new Collider[1024];
    // 用于存储接触攻击检测的碰撞体缓存
    protected Collider[] m_contactAttackOverlaps = new Collider[1024];

    /// <summary>
    /// 血量组件实例
    /// </summary>
    public Health health { get; protected set; }

    /// <summary>
    /// 当前被敌人发现的玩家实例
    /// </summary>
    public Player player { get; protected set; }

    /// <summary>
    /// 敌人属性管理器实例
    /// </summary>
    public EnemyStatsManager stats { get; protected set; }

    /// <summary>
    /// 路径点管理器实例
    /// </summary>
    public WaypointManager waypoints { get; protected set; }

    protected virtual void InitializeStatsManager() => stats = GetComponent<EnemyStatsManager>();
    protected virtual void InitializeWaypointsManager() => waypoints = GetComponent<WaypointManager>();
    protected virtual void InitializeHealth() => health = GetComponent<Health>();
    protected virtual void InitializeTag() => tag = GameTags.Enemy;

    /// <summary>
    /// 组件初始化
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        InitializeTag();              // 设置标签
        InitializeStatsManager();     // 初始化属性管理器
        InitializeWaypointsManager(); // 初始化路径点管理器
        InitializeHealth();           // 初始化血量组件
    }

    /// <summary>
    /// 每帧更新逻辑
    /// </summary>
    protected override void OnUpdate()
    {
        HandleSight();   // 检测玩家
        ContactAttack(); // 检测接触攻击
    }

    /// <summary>
    /// 接触攻击逻辑（如果敌人支持近身碰撞攻击）
    /// </summary>
    public virtual void ContactAttack()
    {
        if (stats.current.canAttackOnContact)
        {
            // 检测指定范围内的实体
            var overlaps = OverlapEntity(m_contactAttackOverlaps, stats.current.contactOffset);

            for (int i = 0; i < overlaps; i++)
            {
                // 如果是玩家
                if (m_contactAttackOverlaps[i].CompareTag(GameTags.Player) &&
                    m_contactAttackOverlaps[i].TryGetComponent<Player>(out var player))
                {
                    // 计算脚下位置（防止玩家从上方踩到）
                    // controller.bounds.max：这是敌人的碰撞盒（Collider）的最高点坐标，通常是敌人头顶的位置。
                    // Vector3.down * stats.current.contactSteppingTolerance：沿着 Y 轴向下偏移一个很小的距离（contactSteppingTolerance），用来做容差范围。
                    // 合起来，stepping 代表敌人“头顶往下一点点”的位置，作为判断玩家是否站在敌人头上的参考点。
                    var stepping = controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;

                    //避免玩家从敌人上方踩踏时，被敌人错误判定为接触攻击
                    if (!player.IsPointUnderStep(stepping))
                    {
                        // 如果开启击退效果
                        if (stats.current.contactPushback)
                        {
                            lateralVelocity = -transform.forward * stats.current.contactPushBackForce;
                        }

                        // 对玩家造成伤害
                        player.ApplyDamage(stats.current.contactDamage, transform.position);
                        enemyEvents.OnPlayerContact?.Invoke(); // 触发接触事件
                    }
                }
            }
        }
    }

    /// <summary>
    /// 敌人视野检测与玩家发现逻辑
    /// </summary>
    protected virtual void HandleSight()
    {
        if (!player) // 没有锁定玩家
        {
            // 检测视野范围内的碰撞体
            var overlaps = Physics.OverlapSphereNonAlloc(position, stats.current.spotRange, m_sightOverlaps);

            for (int i = 0; i < overlaps; i++)
            {
                // 如果是玩家
                if (m_sightOverlaps[i].CompareTag(GameTags.Player))
                {
                    if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
                    {
                        this.player = player; // 记录玩家引用
                        enemyEvents.OnPlayerSpotted?.Invoke(); // 触发发现玩家事件
                        return;
                    }
                }
            }
        }
        else // 已经锁定了玩家
        {
            var distance = Vector3.Distance(position, player.position);

            // 如果玩家死亡或超出视野范围
            if ((player.health.current == 0) || (distance > stats.current.viewRange))
            {
                player = null; // 解除锁定
                enemyEvents.OnPlayerScaped?.Invoke(); // 触发玩家逃脱事件
            }
        }
    }

    /// <summary>
    /// 对敌人造成伤害，并根据血量状态触发相应事件
    /// </summary>
    /// <param name="amount">伤害值</param>
    /// <param name="origin">伤害来源位置</param>
    public override void ApplyDamage(int amount, Vector3 origin)
    {
        if (!health.isEmpty && !health.recovering) // 确保敌人还有血且不在恢复中
        {
            health.Damage(amount); // 扣血
            enemyEvents.OnDamage?.Invoke(); // 触发受伤事件

            if (health.isEmpty) // 血量耗尽
            {
                controller.enabled = false; // 禁用控制器（停止移动）
                enemyEvents.OnDie?.Invoke(); // 触发死亡事件
            }
        }
    }

    /// <summary>
    /// 按摩擦力平滑减速
    /// </summary>
    public virtual void Friction() => Decelerate(stats.current.friction);

    /// <summary>
    /// 按指定加速度和最高速度加速
    /// </summary>
    public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) =>
        Accelerate(direction, stats.current.turningDrag, acceleration, topSpeed);


    /// <summary>
    /// 按减速度平滑减速（横向速度归零）
    /// </summary>
    public virtual void Decelerate() => Decelerate(stats.current.deceleration);

    /// <summary>
    /// 平滑地朝向指定方向旋转
    /// </summary>
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);


    /// <summary>
    /// 应用重力
    /// </summary>
    public virtual void Gravity() => Gravity(stats.current.gravity);

    /// <summary>
    /// 当在地面上时，施加向下的吸附力（防止悬空）
    /// </summary>
    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);
}
