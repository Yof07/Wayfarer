
using UnityEngine;

public class Player : Entity<Player>
{
    public PlayerEvents playerEvents; // 玩家事件（受伤、死亡、拾取物品等触发的事件）
    
    /// <summary> 玩家输入管理器实例 </summary>
    public PlayerInputManager inputs { get; protected set; }
    
    /// <summary> 玩家数值管理器实例 </summary>
    public PlayerStatsManager stats { get; protected set; }

    /// <summary> 生命值实例 </summary>
    public Health health { get; protected set; }
    
    /// <summary> 玩家已跳跃的次数（用于多段跳） </summary>
    public int jumpCounter { get; protected set; }
    
    /// <summary> 玩家是否正在持有物品 </summary>
    public bool holding { get; protected set; }
    
    protected override void Awake()
    {
        base.Awake();
        InitializeInputs();
        InitializeStats();
        InitializeHealth();
        InitializeTag();
        
        // 监听落地事件，重置跳跃次数
        entityEvents.OnGroundEnter.AddListener(ResetJumps);
    }
    
    // 初始化输入
    protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();
    
    // 初始化数值
    protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();

    // 初始化生命
    protected virtual void InitializeHealth() => health = GetComponent<Health>();

    // 初始化标签（标记为 Player）
    protected virtual void InitializeTag() => tag = GameTags.Player;

    /// <summary> 玩家是否可以站立（通过 SphereCast 检测头顶是否有障碍物） </summary>
    public virtual bool canStandUp => !SphereCast(Vector3.up, originalHeight);

    /// <summary>
    /// 对玩家造成伤害（带击退与受伤反应）
    /// </summary>
    public override void ApplyDamage(int amount, Vector3 origin)
    {
        if (health && !health.isEmpty && !health.recovering)
        {
            health.Damage(amount);

            var damageDir = origin - transform.position;
            damageDir.y = 0;
            damageDir = damageDir.normalized;
            FaceDirectionSmooth(damageDir);

            lateralVelocity = -transform.forward * stats.current.hurtBackwardsForce;
            verticalVelocity = Vector3.up * stats.current.hurtUpwardForce;
            states.Change<HurtPlayerState>();

            playerEvents?.OnHurt?.Invoke();
        }
    }
    
    /// <summary>
    /// 在指定方向上平滑移动玩家（加速度控制）
    /// </summary>
    public virtual void Accelerate(Vector3 direction)
    {
        // 根据是否按下 Run 键、是否在地面，决定不同的转向阻尼与加速度
        // var turningDrag = isGrounded && inputs.GetRun() ? stats.current.runningTurningDrag : stats.current.turningDrag;
        // var acceleration = isGrounded && inputs.GetRun() ? stats.current.runningAcceleration : stats.current.acceleration;
        // var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration; // 空中与地面不同
        // var topSpeed = inputs.GetRun() ? stats.current.runningTopSpeed : stats.current.topSpeed;
        
        // 根据是否按下 Run 键、是否在地面，决定不同的转向阻尼与加速度
        var turningDrag = stats.current.turningDrag;
        var acceleration = stats.current.acceleration;
        var finalAcceleration = acceleration;
        var topSpeed = stats.current.topSpeed;
        
        // 调用底层 Accelerate(方向, 转向阻尼, 加速度, 最大速度)
        Accelerate(direction, turningDrag, finalAcceleration, topSpeed);

    }

    /// <summary>
    /// 在指定方向上平滑移动玩家（匍匐状态的参数）
    /// </summary>
    public virtual void CrawlingAccelerate(Vector3 direction) =>
        Accelerate(direction, stats.current.crawlingTurningSpeed, stats.current.crawlingAcceleration,
            stats.current.crawlingTopSpeed);
    
    /// <summary>
    /// 平滑减速（使用 deceleration 参数）
    /// </summary>
    public virtual void Decelerate() => Decelerate(stats.current.deceleration);
    
    /// <summary>
    /// 平滑减速（使用摩擦力参数）
    /// </summary>
    public virtual void Friction()
    {
        if (OnSlopingGround())
            Decelerate(stats.current.slopeFriction); // 在斜坡上使用斜坡摩擦
        else
            Decelerate(stats.current.friction); // 普通摩擦
    }

    /// <summary>
    /// 根据相机方向输入来平滑移动玩家
    /// </summary>
    public virtual void AccelerateToInputDirection()
    {
        var inputDirection = inputs.GetMovementCameraDirection();
        Accelerate(inputDirection);
    }
    
    /// <summary>
    /// 平滑朝向某个方向旋转（陆地旋转速度）
    /// </summary>
    public virtual void FaceDirectionSmooth(Vector3 direction) => FaceDirection(direction, stats.current.rotationSpeed);

    /// <summary>
    /// 施加重力，使玩家下落
    /// </summary>
    public virtual void Gravity()
    {
        if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
        {
            var speed = verticalVelocity.y;
            // 上升时用普通重力，下落时用更强的下落重力
            var force = verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
            speed -= force * gravityMultiplier * Time.deltaTime;

            // 限制最大下落速度
            speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
            verticalVelocity = new Vector3(0, speed, 0);
        }
    }

    /// <summary>
    /// 通过 snap 力量强制把玩家贴到地面上
    /// </summary>
    public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

    /// <summary>
    /// 如果玩家不在地面上，切换到下落状态
    /// </summary>
    public virtual void Fall()
    {
        if (!isGrounded)
        {
            states.Change<FallPlayerState>();
        }
    }

    /// <summary>
    /// 重置跳跃计数（回到 0，常用于落地时）
    /// </summary>
    public virtual void ResetJumps() => jumpCounter = 0;
    
    /// <summary>
    /// 执行跳跃逻辑（包括多段跳、土狼跳、持物判定）
    /// </summary>
    public virtual void Jump()
    {
        // 是否可以进行二段 / 多段跳
        var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
        // 土狼跳判定（离地一小段时间内仍然可以跳）
        var canCoyoteJump = (jumpCounter == 0) && (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
        
        // 地面 / 轨道 / 多段跳 / 土狼跳条件满足时才允许跳跃
        if ((isGrounded || canMultiJump || canCoyoteJump))
        {
            if (inputs.GetJumpDown()) // 按下跳跃键
            {
                Jump(stats.current.maxJumpHeight);
            }
        }

        // 松开跳跃键时，如果还在上升，限制为最小跳跃高度（实现“按得短跳得低”的效果）,早松手就早限制
        if (inputs.GetJumpUp() && (jumpCounter > 0) && (verticalVelocity.y > stats.current.minJumpHeight))
        {
            verticalVelocity = Vector3.up * stats.current.minJumpHeight;
        }
    }
    
    /// <summary>
    /// 执行一个标准的向上跳跃
    /// </summary>
    public virtual void Jump(float height)
    {
        jumpCounter++; // 增加跳跃计数
        verticalVelocity = Vector3.up * height; // 设置垂直速度
        states.Change<FallPlayerState>(); // 切换为下落状态（跳起后最终会落下）
        playerEvents.OnJump?.Invoke(); // 触发跳跃事件
    }
}
