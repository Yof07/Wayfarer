using UnityEngine;

// 要求当前物体上必须有 Player 组件
[RequireComponent(typeof(Player))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Lean")]
public class PlayerLean : MonoBehaviour
{
    [Header("倾斜设置")]
    public Transform target;              // 要倾斜的目标（通常是玩家的模型）
    public float maxTiltAngle = 15;       // 最大倾斜角度
    public float tiltSmoothTime = 0.2f;   // 倾斜的平滑过渡时间

    protected Player m_player;            // 玩家组件引用
    protected Quaternion m_initialRotation; // 初始旋转（未使用，可扩展）

    protected float m_velocity;           // 用于 SmoothDamp 的角速度缓存

    /// <summary>
    /// 判断玩家是否可以进行倾斜效果。
    /// </summary>
    /// <returns>如果当前状态是行走、游泳或滑翔，则返回 true。</returns>
    public virtual bool CanLean()
    {
        var walking = m_player.states.IsCurrentOfType(typeof(WalkPlayerState));    // 玩家是否在行走状态
        var swimming = m_player.states.IsCurrentOfType(typeof(SwimPlayerState));    // 玩家是否在游泳状态
        var gliding = m_player.states.IsCurrentOfType(typeof(GlidingPlayerState));  // 玩家是否在滑翔状态
        return walking || swimming || gliding;
    }

    /// <summary>
    /// 初始化，获取 Player 组件。
    /// </summary>
    protected virtual void Awake()
    {
        m_player = GetComponent<Player>();
    }

    /// <summary>
    /// 在 LateUpdate 中更新倾斜角度，使角色根据移动方向与输入方向的偏差进行左右倾斜。
    /// </summary>
    protected virtual void LateUpdate()
    {
        // 输入方向（相机相对方向）
        var inputDirection = m_player.inputs.GetMovementCameraDirection();
        // 玩家当前的移动方向（水平速度）
        var moveDirection = m_player.lateralVelocity.normalized;

        // 计算输入方向与移动方向之间的夹角（带符号，区分左右）
        var angle = Vector3.SignedAngle(inputDirection, moveDirection, Vector3.up);

        // 如果允许倾斜，则夹角被限制在 [-maxTiltAngle, maxTiltAngle] 范围内
        var amount = CanLean() ? Mathf.Clamp(angle, -maxTiltAngle, maxTiltAngle) : 0;

        // 获取目标局部欧拉角
        var rotation = target.localEulerAngles;

        // 平滑过渡到目标倾斜角度
        rotation.z = Mathf.SmoothDampAngle(rotation.z, amount, ref m_velocity, tiltSmoothTime);

        // 应用新的旋转
        target.localEulerAngles = rotation;
    }
}
