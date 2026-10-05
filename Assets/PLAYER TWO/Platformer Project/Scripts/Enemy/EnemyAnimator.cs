using UnityEngine;

[RequireComponent(typeof(Enemy))]  // 依赖 Enemy 组件，确保同物体上有 Enemy
[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy Animator")]  // 在菜单中显示该脚本的位置
public class EnemyAnimator : MonoBehaviour
{
    public Animator animator;  // 绑定的 Animator 组件

    [Header("Parameters Names")]  // 动画参数名称，方便在 Inspector 中配置
    public string stateName = "State";              // 当前状态参数名
    public string lastStateName = "Last State";     // 上一个状态参数名
    public string lateralSpeedName = "Lateral Speed";  // 横向速度参数名
    public string verticalSpeedName = "Vertical Speed"; // 纵向速度参数名
    public string healthName = "Health";             // 生命值参数名
    public string isGroundedName = "Is Grounded";    // 是否着地参数名
    public string onStateChangedName = "On State Changed"; // 状态切换触发器名

    // 动画参数对应的 Hash 值，提升设置参数效率
    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_healthHash;
    protected int m_isGroundedHash;
    protected int m_onStateChangedHash;

    protected Enemy m_enemy;  // 敌人组件引用

    // 初始化敌人组件
    protected virtual void InitializeEnemy() => m_enemy = GetComponent<Enemy>();

    // 计算动画参数名对应的哈希值
    protected virtual void InitializeParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
    }

    // 监听敌人状态切换事件，触发动画中的 On State Changed 触发器
    protected virtual void InitializeAnimatorTriggers() =>
        m_enemy.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));

    // 初始化阶段调用以上函数
    protected virtual void Start()
    {
        InitializeEnemy();
        InitializeParametersHash();
        InitializeAnimatorTriggers();
    }

    // LateUpdate 中更新动画参数
    protected virtual void LateUpdate()
    {
        // 获取敌人当前的横向速度（大小）和纵向速度（y 分量）
        var lateralSpeed = m_enemy.lateralVelocity.magnitude;
        var verticalSpeed = m_enemy.verticalVelocity.y;

        // 将敌人的当前状态索引传给动画控制器
        animator.SetInteger(m_stateHash, m_enemy.states.index);
        // 上一个状态索引
        animator.SetInteger(m_lastStateHash, m_enemy.states.lastIndex);
        // 横向速度参数
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        // 纵向速度参数
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        // 当前生命值
        animator.SetInteger(m_healthHash, m_enemy.health.current);
        // 是否着地布尔值
        animator.SetBool(m_isGroundedHash, m_enemy.isGrounded);
    }
}
