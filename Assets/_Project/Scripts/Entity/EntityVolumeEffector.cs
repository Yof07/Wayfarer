
using UnityEngine;

/// <summary>
/// 实体区域影响器组件，用于在特定体积范围内影响实体的运动属性。
/// 通过触发器碰撞体检测实体进入和离开，根据设定参数调整实体的运动状态。
/// </summary>
[RequireComponent(typeof(Collider))] // 强制要求该组件所在的GameObject必须有Collider组件
[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity Volume Effector")] // 在组件菜单中的路径
public class EntityVolumeEffector : MonoBehaviour
{

    /// <summary>
    /// 进入区域时，实体速度的乘法转换因子。
    /// </summary>
    public float velocityConversion = 1f;

    /// <summary>
    /// 进入区域时，实体加速度的乘法因子。
    /// </summary>
    public float accelerationMultiplier = 1f;

    /// <summary>
    /// 进入区域时，实体最大速度的乘法因子。
    /// </summary>
    public float topSpeedMultiplier = 1f;

    /// <summary>
    /// 进入区域时，实体减速度的乘法因子。
    /// </summary>
    public float decelerationMultiplier = 1f;

    /// <summary>
    /// 进入区域时，实体转向阻力的乘法因子。
    /// </summary>
    public float turningDragMultiplier = 1f;

    /// <summary>
    /// 进入区域时，实体重力影响的乘法因子。
    /// </summary>
    public float gravityMultiplier = 1f;

    /// <summary>
    /// 缓存的Collider组件引用，用于设置触发器属性。
    /// </summary>
    protected Collider m_collider;


    /// <summary>
    /// 当其他碰撞体进入触发器时调用。
    /// 如果碰撞体挂载了 EntityBase 组件，则根据设定参数调整实体的运动属性。
    /// </summary>
    /// <param name="other">进入触发器的碰撞体。</param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        // 尝试获取碰撞体上的 EntityBase 组件
        if (other.TryGetComponent(out EntityBase entity))
        {
            // 通过乘法因子修改实体当前的速度
            entity.velocity *= velocityConversion;
            // 设置实体各类运动属性的倍率，影响后续运动行为
            entity.accelerationMultiplier = accelerationMultiplier;
            entity.topSpeedMultiplier = topSpeedMultiplier;
            entity.decelerationMultiplier = decelerationMultiplier;
            entity.turningDragMultiplier = turningDragMultiplier;
            entity.gravityMultiplier = gravityMultiplier;
        }
    }

    /// <summary>
    /// 当其他碰撞体离开触发器时调用。
    /// 如果碰撞体挂载了 EntityBase 组件，则重置实体的运动属性倍率为默认值 1。
    /// </summary>
    /// <param name="other">离开触发器的碰撞体。</param>
    protected virtual void OnTriggerExit(Collider other)
    {
        // 尝试获取碰撞体上的 EntityBase 组件
        if (other.TryGetComponent(out EntityBase entity))
        {
            // 将所有运动属性倍率重置为默认值，恢复实体正常行为
            entity.accelerationMultiplier = 1f;
            entity.topSpeedMultiplier = 1f;
            entity.decelerationMultiplier = 1f;
            entity.turningDragMultiplier = 1f;
            entity.gravityMultiplier = 1f;
        }
    }

    /// <summary>
    /// Unity生命周期方法，初始化时获取Collider组件并设置为触发器。
    /// </summary>
    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        // 将Collider设置为触发器，以便检测实体进入和退出事件，而不产生物理碰撞
        m_collider.isTrigger = true;
    }
}
