
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    // 输入动作资源（在 Input System 中配置的 InputActionAsset）
    public InputActionAsset actions;
    
    // 输入动作缓存
    protected InputAction m_movement;
    protected InputAction m_look;
    protected InputAction m_jump;
    protected InputAction m_crouch;
    protected InputAction m_dash;
    protected InputAction m_stomp;
    protected InputAction m_spin;
    protected InputAction m_airDive;
    protected InputAction m_dive;
    protected InputAction m_glide;
    protected InputAction m_grindBrake;
    protected InputAction m_releaseLedge;
    protected InputAction m_pause;
    protected InputAction m_run;
    protected InputAction m_pickAndDrop;
    
    // 主摄像机引用，用于计算相对移动方向
    protected Camera m_camera;
    // 常量：鼠标设备名称
    protected const string k_mouseDeviceName = "Mouse";
    
    // 用于锁定移动方向的时间戳（当前时间小于解锁时间时，禁止移动输入）
    protected float m_movementDirectionUnlockTime;
    
    // 最近一次按下跳跃的时间，用于跳跃缓冲
    protected float? m_lastJumpTime;
    // 常量：跳跃缓冲时长（单位：秒）
    protected const float k_jumpBuffer = 0.15f;
    
    protected virtual void Awake() => CacheActions();

    protected virtual void Start()
    {
        m_camera = Camera.main;
        actions.Enable();
    }

    protected virtual void Update()
    {
        // 记录跳跃按下时间，用于实现跳跃缓冲
        if (m_jump.WasPressedThisFrame())
        {
            m_lastJumpTime = Time.time;
        }
    }
    
    protected virtual void OnEnable() => actions?.Enable();
    protected virtual void OnDisable() => actions?.Disable();

    protected virtual void CacheActions()
    {
        m_movement = actions["Movement"];
        m_look =  actions["Look"];
        m_jump = actions["Jump"];
        m_crouch = actions["Crouch"];
        m_dash = actions["Dash"];
        m_stomp = actions["Stomp"];
        m_spin = actions["Spin"];
        m_airDive = actions["AirDive"];
        m_dive = actions["Dive"];
        m_glide = actions["Glide"];
        m_grindBrake = actions["Grind Brake"];
        m_releaseLedge = actions["ReleaseLedge"];
        m_pause = actions["Pause"];
        m_run = actions["Run"];
        m_pickAndDrop = actions["PickAndDrop"];
    }
    
    /// <summary>
    /// 获取观察方向输入（鼠标时直接返回，手柄时使用死区修正）
    /// </summary>
    public virtual Vector3 GetLookDirection()
    {
        var value = m_look.ReadValue<Vector2>();

        if (IsLookingWithMouse())
        {
            return new Vector3(value.x, 0, value.y);
        }

        return GetAxisWithCrossDeadZone(value);
    }
    
    /// <summary>
    /// 判断是否通过鼠标进行观察输入
    /// </summary>
    public virtual bool IsLookingWithMouse()
    {
        if (m_look.activeControl == null)
        {
            return false;
        }
        return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
    }

    /// <summary>
    /// 临时锁定移动方向输入
    /// </summary>
    /// <param name="duration">锁定时长（秒）</param>
    public virtual void LockMovementDirection(float duration = 0.25f)
    {
        m_movementDirectionUnlockTime = Time.time + duration;
    }
    
    /// <summary>
    /// 获取移动方向输入（带十字型死区判断）
    /// 如果在锁定时间内，则返回 Vector3.zero
    /// </summary>
    public virtual Vector3 GetMovementDirection()
    {
        if (Time.time < m_movementDirectionUnlockTime) return Vector3.zero;

        var value = m_movement.ReadValue<Vector2>();
        return GetAxisWithCrossDeadZone(value);
    }
    
    /// <summary>
    /// 根据十字形死区修正输入值（Input System 默认是圆形死区）
    /// </summary>
    /// <param name="axis">输入轴</param>
    public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
    {
        var deadzone = InputSystem.settings.defaultDeadzoneMin; // 死区值，小于这个值就被当作0
        axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadzone(axis.x, deadzone) : 0;
        axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadzone(axis.y, deadzone) : 0;
        return new Vector3(axis.x, 0, axis.y);
    }
    
    /// <summary>
    /// 把超过死区的输入值重新映射，让它从 0 开始增长到 1，即把区间从[-deadzone, 0] [deadzone, 1] 拉伸为[-1, 0] [0, 1]
    /// 拉伸是为了有正常的过渡，刚离开死区，角色速度不应该突然从 0 跳到 deadzone 对应的速度
    /// 拉伸是为了解决因为deadzone的防误触而带来的不平滑的问题
    /// </summary>
    // protected float RemapToDeadzone(float value,float deadzone)=>(value - deadzone) / (1-deadzone);
    protected float RemapToDeadzone(float value, float deadzone) => (value - (value > 0 ? deadzone : -deadzone)) / (1 - deadzone);
    
    /// <summary>
    /// 获取相机方向下的移动向量  
    /// 将输入方向映射到相机朝向（Y轴旋转）下
    /// 就是玩家的实际移动方向是由 输入方向(wasd或左摇杆来改变) 和 摄像机的方向(鼠标或右摇杆来改变)
    /// </summary>
    public virtual Vector3 GetMovementCameraDirection()
    {
        // 1. 获取移动方向（通常是玩家输入的水平/垂直方向，比如 WSAD 或摇杆）
        var direction = GetMovementDirection();

        // 2. 如果有输入（不是零向量）
        if (direction.sqrMagnitude > 0)
        {
            // 3. 构建一个旋转：根据摄像机的 Y 轴角度（水平朝向）
            // Quaternion.AngleAxis(angle, axis) 表示绕某个轴旋转一个角度
            // 即摄像机当前的旋转角度
            var rotation = Quaternion.AngleAxis(m_camera.transform.eulerAngles.y, Vector3.up);

            // 4. 把原始输入方向旋转到摄像机的朝向下
            direction = rotation * direction;

            // 5. 归一化，保持方向向量的长度为 1（只保留方向）
            direction = direction.normalized;
        }

        // 6. 返回最终的世界空间移动方向
        return direction;
    }
    
    /// <summary>
    /// 判断是否触发跳跃（支持跳跃缓冲）
    /// </summary>
    public virtual bool GetJumpDown()
    {
        if (m_lastJumpTime != null &&
            Time.time - m_lastJumpTime < k_jumpBuffer)
        {
            m_lastJumpTime = null;
            return true;
        }
        return false;
    }

    public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();
    public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();
    public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();
    public virtual bool GetSpinDown() => m_spin.WasPressedThisFrame();
    public virtual bool GetAirDiveDown() => m_airDive.WasPressedThisFrame();
    public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();
    public virtual bool GetDive() => m_dive.IsPressed();
    public virtual bool GetGlide() => m_glide.IsPressed();
    public virtual bool GetGrindBrake() => m_grindBrake.IsPressed();
    public virtual bool GetReleaseLedgeDown() => m_releaseLedge.WasPressedThisFrame();
    public virtual bool GetPauseDown() => m_pause.WasPressedThisFrame();

    // 获取奔跑输入是否处于按住状态
    public virtual bool GetRun() => m_run.IsPressed();

    // 获取奔跑输入是否在本帧松开
    public virtual bool GetRunUp() => m_run.WasReleasedThisFrame();

    // 获取下蹲/爬行输入是否处于按住状态
    public virtual bool GetCrouchAndCraw() => m_crouch.IsPressed();
}
