using UnityEngine;

public abstract class EntityBase : MonoBehaviour
{
    // 忽略碰撞器缩放的实体位置，如角色的脚底而非角色胶囊体的中心
    public Vector3 unsizedPosition => transform.position;
    
    public bool isGrounded { get; protected set; } = true;        // 是否在地面上
    
    public CharacterController controller { get; protected set; }   // 角色控制器组件
    
    public float originalHeight { get; protected set; }             // 初始碰撞器高度
    
    public float lastGroundTime { get; protected set; }             // 上一次处于地面时间
    
    public float groundAngle { get; protected set; }               // 当前地面角度

    public Vector3 groundNormal { get; protected set; }            // 当前地面法线
    
    public Vector3 localSlopeDirection { get; protected set; }     // 当前地面的局部斜坡方向
    
    public EntityEvents entityEvents;
    
    public RaycastHit groundHit;                                   // 当前地面检测的碰撞信息
    
    public float height => controller.height;                      // 碰撞器当前高度
    
    public float radius => controller.radius;                      // 碰撞器半径
    
    public Vector3 center => controller.center;                    // 碰撞器中心点
    
    // 实体当前位置（角色控制器中心加位置）
    public Vector3 position => transform.position + center;
    
    // 脚步位置（底部位置，考虑了stepOffset）
    public Vector3 stepPosition => position - transform.up * (height * 0.5f - controller.stepOffset);
    
    protected readonly float m_groundOffset = 0.1f;              // 地面检测偏移
    
    // 判断实体是否在斜坡上
    public virtual bool OnSlopingGround()
    {
        return false;
    }

    // 球形检测（无返回检测信息，只返回是否检测到碰撞）
    public virtual bool SphereCast(Vector3 direction, float distance, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        // 调用带有返回检测信息的方法，并忽略返回的hit信息
        return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
    }
    
    // 球形检测（返回检测信息，检测是否与其他物体发生碰撞）
    public virtual bool SphereCast(Vector3 direction, float distance,
        out RaycastHit hit, int layer = Physics.DefaultRaycastLayers,
        QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
    {
        // 计算球形检测的有效距离，确保球形的检测范围符合预期
        var castDistance = Mathf.Abs(distance - radius);

        // 使用物理引擎进行球形碰撞检测
        return Physics.SphereCast(position, radius, direction,
            out hit, castDistance, layer, queryTriggerInteraction);
    }
    
    // 判断一个点是否在实体脚步位置下方（用于踩踏检测）
    public virtual bool IsPointUnderStep(Vector3 point) => stepPosition.y > point.y;

    // 受到伤害（空实现，子类按需重写）
    public virtual void ApplyDamage(int damage, Vector3 origin) { }

    // 调整角色控制器碰撞器高度
    public virtual void ResizeCollider(float height)
    {
        // 计算新的高度和当前高度的差值
        var delta = height - this.height;
        // 修改角色控制器的高度
        controller.height = height;
        // 调整角色控制器的中心位置，使其根据高度变化自动平移
        controller.center += Vector3.up * delta * 0.5f;
    }
}

// 泛型版本实体类，T继承自Entity<T>
public abstract class Entity<T> : EntityBase where T : Entity<T>
{
    // 状态管理器
    public EntityStateManager<T> states { get; protected set; } 
    
    public Vector3 velocity { get; set; }                          // 当前速度
    
    public float accelerationMultiplier { get; set; } = 1f; // 加速度倍率

    public float gravityMultiplier { get; set; } = 1f; // 重力倍率

    public float topSpeedMultiplier { get; set; } = 1f; // 最高速度倍率

    public float turningDragMultiplier { get; set; } = 1f; // 转向阻力倍率

    public float decelerationMultiplier { get; set; } = 1f; // 减速度倍率
    
    // 当前水平速度（XZ平面速度）
    public Vector3 lateralVelocity
    {
        get { return new Vector3(velocity.x, 0, velocity.z); }
        set { velocity = new Vector3(value.x, velocity.y, value.z); }
    }

    // 当前垂直速度（Y轴速度）
    public Vector3 verticalVelocity
    {
        get { return new Vector3(0, velocity.y, 0); }
        set { velocity = new Vector3(velocity.x, value.y, velocity.z); }
    }
    
    // 初始化状态管理器
    protected virtual void InitializeStateManager() => states = GetComponent<EntityStateManager<T>>();
    
    // 初始化角色控制器组件（CharacterController）
    // 负责角色的基本移动、碰撞等物理交互
    protected virtual void InitializeController()
    {
        // 获取当前物体上的 CharacterController 组件
        controller = GetComponent<CharacterController>();
        // 如果没有，就动态添加一个 CharacterController
        if (!controller)
        {
            controller = gameObject.AddComponent<CharacterController>();
        }

        // skinWidth 表示碰撞器表面到实际碰撞检测边界的距离（防止卡住用的小偏移）
        controller.skinWidth = 0.005f;
        // minMoveDistance 为最小移动距离（设为 0 表示即使移动非常小也会被检测到）
        controller.minMoveDistance = 0;
        // 记录角色控制器的初始高度（用于后续复位或高度调整）
        originalHeight = controller.height;
    }
    
    protected virtual void Awake()
    {
        InitializeController();
        InitializeStateManager();
    }
    
    // 根据输入的方向平滑加速移动
    public virtual void Accelerate(Vector3 direction, float turningDrag, float acceleration, float topSpeed)
    {
        // 判断方向是否有效（不为零向量）
        if (direction.sqrMagnitude > 0)
        {
            // 计算当前速度在目标方向上的投影速度（标量）
            var speed = Vector3.Dot(direction, lateralVelocity);
            // 计算当前速度在目标方向上的向量部分
            var velocity = direction * speed;
            // 计算当前速度中垂直于目标方向的部分（转向速度）
            var turningVelocity = lateralVelocity - velocity;
            // 计算转向阻力对应的速度变化量（根据转向阻力系数和时间增量）
            var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
            // 计算最大允许速度（考虑速度倍率）
            var targetTopSpeed = topSpeed * topSpeedMultiplier;

            // 如果当前速度未达最大速度，或当前速度与目标方向相反，则加速
            if (lateralVelocity.magnitude < targetTopSpeed || speed < 0)
            {
                // 增加速度，受加速度倍率和时间影响
                speed += acceleration * accelerationMultiplier * Time.deltaTime;
                // 限制速度在[-最大速度, 最大速度]范围内
                speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
            }

            // 重新计算目标方向速度向量
            velocity = direction * speed;
            // 将转向速度平滑减小到0，实现自然转向过渡
            turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
            // 更新横向速度为目标方向速度与转向速度之和
            lateralVelocity = velocity + turningVelocity;
        }
    }
    
    // 处理角色控制器的移动
    protected virtual void HandleController()
    {
        if (controller.enabled)
        {
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        transform.position += velocity * Time.deltaTime;
    }
    
    // 通过状态机来调用当前状态的步进逻辑
    protected virtual void HandleStates() => states.Step();
    
    // 处理角色与地面的检测与相关逻辑
    protected virtual void HandleGround()
    {
        // 如果角色在轨道模式（onRails）下，不做地面检测
        // if (onRails) return;

        // 距离计算：角色半高 + 地面检测的额外偏移量
        var distance = (height * 0.5f) + m_groundOffset;

        // 向下发射球体射线检测地面，并且角色的垂直速度 ≤ 0（下落或静止状态）
        if (SphereCast(Vector3.down, distance, out var hit) && verticalVelocity.y <= 0)
        {
            // 如果之前不在地面状态
            if (!isGrounded)
            {
                // 判断是否满足落地条件
                if (EvaluateLanding(hit))
                {
                    // 进入落地逻辑
                    EnterGround(hit);
                }
            } // 已经在地面状态
            else if (IsPointUnderStep(hit.point))
            {
                // 更新地面信息（比如接触点、法线等）
                UpdateGround(hit);
            }
        }
        else
        {
            // 射线未检测到地面，则视为离开地面
            ExitGround();
        }
        
    }
    
    // 离开地面状态（角色刚刚离开地面时调用）
    protected virtual void ExitGround()
    {
        // 只有当前在地面状态时才执行
        if (isGrounded)
        {
            // 标记角色不在地面
            isGrounded = false;
            // 解除与地面的父子关系（如果站在移动平台上，需要解绑）
            transform.parent = null;
            // 记录离开地面的时间（可能用于跳跃缓冲或着陆判断）
            lastGroundTime = Time.time;
            // 限制垂直速度：如果正在向下运动，不改变；如果有向上的速度，则保留（防止离地瞬间速度异常）
            verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);
            // 触发“离开地面”的事件（例如播放起跳动画）
            entityEvents.OnGroundExit?.Invoke();
        }
    }

    // 更新地面相关数据（每帧更新站立地面的法线、坡度等信息）
    protected virtual void UpdateGround(RaycastHit hit)
    {
        // 只有当前处于地面状态时才执行
        if (isGrounded)
        {
            // 更新地面射线检测信息
            groundHit = hit;
            // 记录地面法线（用于计算坡度方向）
            groundNormal = groundHit.normal;
            // 计算当前地面的坡度角（与世界Y轴的夹角）
            groundAngle = Vector3.Angle(Vector3.up, groundHit.normal);
            // 计算本地的坡度方向（水平投影后的法线方向）
            localSlopeDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
            // 如果地面是平台（tag = Platform），让角色成为平台的子物体，跟随平台移动
            // 否则取消父子关系
            transform.parent = hit.collider.CompareTag(GameTags.Platform) ? hit.transform : null;
        }
    }

    // 将角色吸附到地面（防止悬空）
    public virtual void SnapToGround(float force)
    {
        // 只有接触地面，且垂直速度是向下的（y <= 0）才生效
        if (isGrounded && (verticalVelocity.y <= 0))
        {
            // 将垂直速度设置为一个恒定向下的力（防止离地浮空）
            verticalVelocity = Vector3.down * force;
        }
    }

    // 让角色按一定旋转速度朝向某个方向（平滑转向）
    public virtual void FaceDirection(Vector3 direction, float degreesPerSecond)
    {
        // 必须是有效的方向
        if (direction != Vector3.zero)
        {
            // 当前旋转
            var rotation = transform.rotation;
            // 本帧允许的最大旋转角度（受 Time.deltaTime 影响）
            var rotationDelta = degreesPerSecond * Time.deltaTime;
            // 目标旋转
            var target = Quaternion.LookRotation(direction, Vector3.up);
            // 按最大旋转速度逐渐逼近目标旋转
            transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
        }
    }
    
    // 平滑减速，速度逐渐趋近于 0（水平速度减速）
    public virtual void Decelerate(float deceleration)
    {
        // 计算本帧的减速度（decelerationMultiplier 可用于调节全局减速效果）
        var delta = deceleration * decelerationMultiplier * Time.deltaTime;
        // 将 lateralVelocity（水平速度向量）逐渐插值到 Vector3.zero（完全停止）
        // 第三个参数是本帧允许的最大速度变化量
        lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
    }
    
    // 评估是否符合着陆条件
    protected virtual bool EvaluateLanding(RaycastHit hit)
    {
        //slopeLimit是坡度的最大限制角度
        return IsPointUnderStep(hit.point) && Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit;
    }
    
    // 进入地面状态（角色刚刚落地时调用）
    protected virtual void EnterGround(RaycastHit hit)
    {
        // 只有当前不是地面状态时才执行（防止重复触发）
        if (!isGrounded)
        {
            // 记录当前地面的射线检测信息（位置、法线等）
            groundHit = hit;
            // 标记角色已经在地面上
            isGrounded = true;
            // 触发“进入地面”的事件（例如播放落地动画、音效）
            entityEvents.OnGroundEnter?.Invoke();
        }
    }

    protected virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var contacts = hit.collider.GetComponents<IEntityContact>();

        foreach (var contact in contacts)
        {
            contact.OnEntityContact(this);
        }
    }

    protected virtual void Update()
    {
        if (controller.enabled)
        {
            HandleStates();
            HandleController();
            HandleGround();
        }

    }
}
    
