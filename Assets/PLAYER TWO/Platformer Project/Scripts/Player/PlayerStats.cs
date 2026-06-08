using UnityEngine;

public class PlayerStats : EntityStats<PlayerStats>
{
    
    //==============================【基础属性】==============================//
    [Header("General Stats")]
    public float pushForce = 4f;           // 推动物体的力量
    public float snapForce = 15f;          // 将角色贴合到地面的吸附力
    public float slideForce = 10f;         // 下坡滑动的额外推力
    public float rotationSpeed = 970f;     // 玩家角色旋转速度（度/秒）
    public float gravity = 38f;            // 普通重力加速度
    public float fallGravity = 65f;        // 下落时额外重力加速度
    public float gravityTopSpeed = 50f;    // 重力作用下的最大下落速度
    
    //==============================【运动属性】==============================//
    [Header("Motion Stats")]
    public bool applySlopeFactor = true;   // 是否考虑坡度因子
    public float acceleration = 13f;       // 加速度
    public float deceleration = 28f;       // 减速度
    public float friction = 28f;           // 地面摩擦力
    public float slopeFriction = 18f;      // 坡面摩擦力
    public float topSpeed = 6f;            // 最高速度
    public float turningDrag = 28f;        // 转向时的阻力
    public float airAcceleration = 32f;    // 空中加速度
    public float brakeThreshold = -0.8f;   // 刹车判定阈值
    public float slopeUpwardForce = 25f;   // 上坡时的额外推力
    public float slopeDownwardForce = 28f; // 下坡时的额外推力
    
    //==============================【跳跃】==============================//
    [Header("Jump Stats")]
    public int multiJumps = 1;                 // 允许的额外跳跃次数（多段跳）
    public float coyoteJumpThreshold = 0.15f;  // 土狼跳判定时间（离地后还能跳的时间窗口）
    public float maxJumpHeight = 17f;          // 最大跳跃高度
    public float minJumpHeight = 10f;          // 最小跳跃高度（轻按跳）
    
    //==============================【受伤反应】==============================//
    [Header("Hurt Stats")]
    public float hurtUpwardForce = 10f;        // 受伤时向上的力
    public float hurtBackwardsForce = 5f;      // 受伤时向后的力
    
    //==============================【下蹲】==============================//
    [Header("Crouch Stats")]
    public float crouchHeight = 1f;            // 下蹲时角色高度
    public float crouchFriction = 10f;         // 下蹲时摩擦力
    
    //==============================【匍匐爬行】==============================//
    [Header("Crawling Stats")]
    public float crawlingAcceleration = 8f;    // 爬行加速度
    public float crawlingFriction = 32f;       // 爬行摩擦力
    public float crawlingTopSpeed = 2.5f;      // 爬行最高速度
    public float crawlingTurningSpeed = 3f;    // 爬行转向速度
    
    //==============================【后空翻】==============================//
    [Header("Backflip Stats")]
    public bool canBackflip = true;            // 是否能后空翻
    public bool backflipLockMovement = true;   // 后空翻时是否锁定移动
    public float backflipAirAcceleration = 12f;// 空中加速度
    public float backflipTurningDrag = 2.5f;   // 转向阻力
    public float backflipTopSpeed = 7.5f;      // 最高速度
    public float backflipJumpHeight = 23f;     // 跳跃高度
    public float backflipGravity = 35f;        // 重力
    public float backflipBackwardForce = 4f;   // 向后推力
    public float backflipBackwardTurnForce = 8f;// 向后转向力
    
    //==============================【冲刺/疾跑】==============================//
    [Header("Dash Stats")]
    public bool canAirDash = true;             // 是否能空中冲刺
    public bool canGroundDash = true;          // 是否能地面冲刺
    public float dashForce = 25f;              // 冲刺推力
    public float dashDuration = 0.3f;          // 冲刺持续时间
    public float groundDashCoolDown = 0.5f;    // 地面冲刺冷却时间
    public float allowedAirDashes = 1;         // 允许的空中冲刺次数
    
    //==============================【踩踏攻击】==============================//
    [Header("Stomp Attack Stats")]
    public bool canStompAttack = true;         // 是否能进行踩踏攻击
    public float stompDownwardForce = 20f;     // 踩踏时向下的力
    public float stompAirTime = 0.8f;          // 空中踩踏时间
    public float stompGroundTime = 0.5f;       // 落地后的硬直时间
    public float stompGroundLeapHeight = 10f;  // 踩踏落地后的反弹高度
    
    //==============================【旋转攻击】==============================//
    [Header("Spin Stats")]
    public bool canSpin = true;                // 是否可以旋转攻击
    public bool canAirSpin = true;             // 是否可以空中旋转攻击
    public float spinDuration = 0.5f;          // 旋转攻击持续时间
    public float airSpinUpwardForce = 10f;     // 空中旋转攻击时的上升力
    public int allowedAirSpins = 1;            // 允许的空中旋转次数
    
    //==============================【拾取与投掷】==============================//
    [Header("Pick'n Throw Stats")]
    public bool canPickUp = true;          // 是否允许拾取物体
    public bool canPickUpOnAir = false;    // 是否可以在空中拾取
    public bool canJumpWhileHolding = true;// 拿着物体时是否可以跳跃
    public float pickDistance = 0.5f;      // 拾取判定距离
    public float throwVelocityMultiplier = 1.5f; // 投掷时的速度倍率
    
    //==============================【空中俯冲】==============================//
    [Header("Air Dive Stats")]
    public bool canAirDive = true;             // 是否能进行空中俯冲
    public bool applyDiveSlopeFactor = true;   // 是否考虑坡度因子
    public float airDiveForwardForce = 16f;    // 俯冲向前力
    public float airDiveFriction = 32f;        // 俯冲摩擦力
    public float airDiveSlopeFriction = 12f;   // 坡地摩擦
    public float airDiveSlopeUpwardForce = 35f;// 上坡时的额外推力
    public float airDiveSlopeDownwardForce = 40f;// 下坡时的额外推力
    public float airDiveGroundLeapHeight = 10f;// 俯冲落地后的跳跃高度
    public float airDiveRotationSpeed = 45f;   // 俯冲旋转速度
    
    //==============================【奔跑】==============================//
    [Header("Running Stats")]
    public float runningAcceleration = 16f;   // 奔跑加速度
    public float runningTopSpeed = 7.5f;      // 奔跑最高速度
    public float runningTurningDrag = 14f;    // 奔跑转向阻力
    
    //==============================【游泳】==============================//
    [Header("Swimming Stats")]
    public float waterConversion = 0.35f;      // 水中移动系数
    public float waterRotationSpeed = 360f;    // 水中旋转速度
    public float waterUpwardsForce = 8f;       // 浮力
    public float waterJumpHeight = 15f;        // 水中跳高度
    public float waterTurningDrag = 2.5f;      // 水中转向阻力
    public float swimAcceleration = 4f;        // 游泳加速度
    public float swimDeceleration = 3f;        // 游泳减速度
    public float swimTopSpeed = 4f;            // 游泳最高速度
    public float swimDiveForce = 15f;          // 潜水下潜力
    
    //==============================【滑翔】==============================//
    [Header("Gliding Stats")]
    public bool canGlide = true;               // 是否能滑翔
    public float glidingGravity = 10f;         // 滑翔时的重力
    public float glidingMaxFallSpeed = 2f;     // 最大下落速度
    public float glidingTurningDrag = 8f;      // 转向阻力
    
    //==============================【墙壁滑行 & 蹬墙跳】==============================//
    [Header("Wall Drag Stats")]
    public bool canWallDrag = true;            // 是否可以贴墙滑行
    public bool wallJumpLockMovement = true;   // 蹬墙跳后是否锁定移动
    public LayerMask wallDragLayers;           // 可以进行墙滑的层
    public Vector3 wallDragSkinOffset;         // 判定偏移
    public float wallDragGravity = 12f;        // 墙滑时的重力
    public float wallJumpDistance = 8f;        // 蹬墙跳水平距离
    public float wallJumpHeight = 15f;         // 蹬墙跳垂直高度
    
    //==============================【爬杆】==============================//
    [Header("Pole Climb Stats")]
    public bool canPoleClimb = true;           // 是否能爬杆
    public Vector3 poleClimbSkinOffset;        // 碰撞偏移
    public float climbUpSpeed = 3f;            // 向上爬速度
    public float climbDownSpeed = 8f;          // 向下爬速度
    public float climbRotationSpeed = 2f;      // 转动速度
    public float poleJumpDistance = 8f;        // 杆子跳水平距离
    public float poleJumpHeight = 15f;         // 杆子跳高度
    
    //==============================【铁轨滑行（Grinding）】==============================//
    [Header("Rail Grinding Stats")]
    public bool useCustomCollision = true;     // 是否使用自定义碰撞检测
    public float grindRadiusOffset = 0.26f;    // 贴合轨道的半径偏移
    public float minGrindInitialSpeed = 10f;   // 开始滑轨的最小初速度
    public float minGrindSpeed = 5f;           // 最小滑行速度
    public float grindTopSpeed = 25f;          // 最大滑行速度
    public float grindDownSlopeForce = 40f;    // 下坡时推力
    public float grindUpSlopeForce = 30f;      // 上坡时推力

    //==============================【铁轨刹车】==============================//
    [Header("Rail Grinding Brake")]
    public bool canGrindBrake = true;          // 是否能在滑轨时刹车
    public float grindBrakeDeceleration = 10;  // 刹车减速度

    //==============================【铁轨冲刺】==============================//
    [Header("Rail Grinding Dash Stats")]
    public bool canGrindDash = true;           // 是否能在滑轨时冲刺
    public bool applyGrindingSlopeFactor = true;// 是否考虑坡度因子
    public float grindDashCoolDown = 0.5f;     // 冲刺冷却时间
    public float grindDashForce = 25f;         // 冲刺推力
    
    //==============================【悬挂在边缘】==============================//
    [Header("Ledge Hanging Stats")]
    public bool canLedgeHang = true;           // 是否可以悬挂
    public LayerMask ledgeHangingLayers;       // 悬挂检测层
    public Vector3 ledgeHangingSkinOffset;     // 碰撞偏移
    public float ledgeMaxForwardDistance = 0.25f; // 前向最大检测距离
    public float ledgeMaxDownwardDistance = 0.25f;// 下向最大检测距离
    public float ledgeSideMaxDistance = 0.5f;     // 侧向最大检测距离
    public float ledgeSideHeightOffset = 0.15f;   // 侧边高度偏移
    public float ledgeSideCollisionRadius = 0.25f;// 侧边检测半径
    public float ledgeMovementSpeed = 1.5f;       // 悬挂时移动速度
    
    //==============================【攀爬边缘】==============================//
    [Header("Ledge Climbing Stats")]
    public bool canClimbLedges = true;         // 是否可以攀爬边缘
    public LayerMask ledgeClimbingLayers;      // 攀爬检测层
    public Vector3 ledgeClimbingSkinOffset;    // 碰撞偏移
    public float ledgeClimbingDuration = 1f;   // 攀爬动作时长
}