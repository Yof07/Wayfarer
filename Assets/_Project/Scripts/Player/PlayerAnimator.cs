using System.Collections.Generic;
using UnityEngine;


// 挂载当前组件的对象，必须也挂载 Player 组件
[RequireComponent(typeof(Player))]
// 在 Unity 的 "Add Component" 菜单里显示该脚本的路径
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Animator")]
public class PlayerAnimator : MonoBehaviour
{
    /// <summary>
    /// 定义一个强制过渡的类，用于指定从某个玩家状态退出时，
    /// 强制跳转到 Animator 控制器中的某个动画。
    /// </summary>
    [System.Serializable]
    public class ForcedTransition
    {
        [Tooltip("玩家状态机中 'fromStateId' 的状态结束时，强制跳转到'toAnimationStat'指定的动画")]
        public int fromStateId;

        [Tooltip("目标动画所在的 Animator 层索引。默认0表示 Base Layer")]
        public int animationLayer;

        [Tooltip("要强制播放的动画状态名")]
        public string toAnimationState;
    }
    
    [Header("Parameters Names")] // Animator 参数的变量名，集中配置（可在 Inspector 修改）
    public string stateName = "State";                      // 当前状态
    public string lastStateName = "Last State";             // 上一个状态
    public string lateralSpeedName = "Lateral Speed";       // 横向速度
    public string verticalSpeedName = "Vertical Speed";     // 纵向速度
    public string lateralAnimationSpeedName = "Lateral Animation Speed"; // 横向动画播放速度
    public string healthName = "Health";                    // 血量
    public string jumpCounterName = "Jump Counter";         // 跳跃计数
    public string isGroundedName = "Is Grounded";           // 是否落地
    public string isHoldingName = "Is Holding";             // 是否正在抓取物品
    public string onStateChangedName = "On State Changed";  // 状态切换触发器
    
    [Header("Settings")]
    public float minLateralAnimationSpeed = 0.5f; // 横向动画播放的最小速度，防止太慢
    public List<ForcedTransition> forcedTransitions; // 强制过渡的列表
    
    // 角色 Animator 组件（动画控制器）
    public Animator animator;
    
    // Animator 参数的 Hash 值，避免的是每帧重复处理字符串和查找参数名
    protected int m_stateHash;
    protected int m_lastStateHash;
    protected int m_lateralSpeedHash;
    protected int m_verticalSpeedHash;
    protected int m_lateralAnimationSpeedHash;
    protected int m_healthHash;
    protected int m_jumpCounterHash;
    protected int m_isGroundedHash;
    protected int m_isHoldingHash;
    protected int m_onStateChangedHash;
    
    // 强制过渡的映射表（键和值的首字段相同，通过字典来快速查找过渡）
    protected Dictionary<int, ForcedTransition> m_forcedTransitions;

    // 引用玩家对象
    protected Player m_player;

    /// <summary>
    /// 脚本启动时初始化所有逻辑
    /// </summary>
    protected virtual void Start()
    {
        InitializePlayer();
        InitializeForcedTransitions();
        InitializeParametersHash();
        InitializeAnimatorTriggers();
    }
    
    /// <summary>
    /// 在 LateUpdate 中更新 Animator 参数，
    /// 保证动画在物理和输入计算完成后才同步
    /// </summary>
    protected virtual void LateUpdate() => HandleAnimatorParameters();
    
    /// <summary>
    /// 每帧更新 Animator 参数，使动画与玩家实际状态同步
    /// </summary>
    protected virtual void HandleAnimatorParameters()
    {
        var lateralSpeed = m_player.lateralVelocity.magnitude; // 横向速度
        var verticalSpeed = m_player.verticalVelocity.y;       // 纵向速度
        // 横向动画播放速度 = 横向速度 / 最大速度，保证最小速度不低于 minLateralAnimationSpeed
        var lateralAnimationSpeed = Mathf.Max(minLateralAnimationSpeed, lateralSpeed / m_player.stats.current.topSpeed);

        // 设置 Animator 参数
        animator.SetInteger(m_stateHash, m_player.states.index);
        animator.SetInteger(m_lastStateHash, m_player.states.lastIndex);
        animator.SetFloat(m_lateralSpeedHash, lateralSpeed);
        animator.SetFloat(m_verticalSpeedHash, verticalSpeed);
        animator.SetFloat(m_lateralAnimationSpeedHash, lateralAnimationSpeed);
        animator.SetInteger(m_jumpCounterHash, m_player.jumpCounter);
        animator.SetBool(m_isGroundedHash, m_player.isGrounded);
        animator.SetBool(m_isHoldingHash, m_player.holding);
    }
    
    /// <summary>
    /// 初始化 Player 引用，并监听状态切换事件
    /// </summary>
    protected virtual void InitializePlayer()
    {
        m_player = GetComponent<Player>();
        // 绑定回调
        m_player.states.events.onChange.AddListener(HandleForcedTransitions);
    }
    
    /// <summary>
    /// 初始化强制过渡字典，避免重复 Key
    /// </summary>
    protected virtual void InitializeForcedTransitions()
    {
        m_forcedTransitions = new Dictionary<int, ForcedTransition>();

        foreach (var transition in forcedTransitions)
        {
            if (!m_forcedTransitions.ContainsKey(transition.fromStateId))
            {
                m_forcedTransitions.Add(transition.fromStateId, transition);
            }
        }
    }
    
    /// <summary>
    /// 初始化 Animator 的触发器，当状态切换时触发动画事件
    /// </summary>
    protected virtual void InitializeAnimatorTriggers()
    {
        // 绑定回调(有状态发生变化就触发一次 m_onStateChangedHash 所代表的触发器)
        m_player.states.events.onChange.AddListener(() => animator.SetTrigger(m_onStateChangedHash));   // ()=>类似lambda
    }
    
    /// <summary>
    /// 执行强制过渡逻辑：
    /// 如果上一个状态匹配强制过渡表，则播放对应的动画
    /// </summary>
    protected virtual void HandleForcedTransitions()
    {
        var lastStateIndex = m_player.states.lastIndex;

        if (m_forcedTransitions.ContainsKey(lastStateIndex))
        {
            var layer = m_forcedTransitions[lastStateIndex].animationLayer;
            animator.Play(m_forcedTransitions[lastStateIndex].toAnimationState, layer);
        }
    }
    
    /// <summary>
    /// 把参数名转换为 Hash，提高性能
    /// </summary>
    protected virtual void InitializeParametersHash()
    {
        m_stateHash = Animator.StringToHash(stateName);
        m_lastStateHash = Animator.StringToHash(lastStateName);
        m_lateralSpeedHash = Animator.StringToHash(lateralSpeedName);
        m_verticalSpeedHash = Animator.StringToHash(verticalSpeedName);
        m_lateralAnimationSpeedHash = Animator.StringToHash(lateralAnimationSpeedName);
        m_healthHash = Animator.StringToHash(healthName);
        m_jumpCounterHash = Animator.StringToHash(jumpCounterName);
        m_isGroundedHash = Animator.StringToHash(isGroundedName);
        m_isHoldingHash = Animator.StringToHash(isHoldingName);
        m_onStateChangedHash = Animator.StringToHash(onStateChangedName);
    }
}