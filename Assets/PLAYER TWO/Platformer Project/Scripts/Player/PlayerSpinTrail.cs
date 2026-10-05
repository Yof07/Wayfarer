using UnityEngine;

/// <summary>
/// 玩家旋转攻击时的拖尾特效组件。
/// 当玩家处于 SpinPlayerState（旋转状态）时，
/// 在玩家手部挂点（hand）位置生成拖尾特效。
/// </summary>
[RequireComponent(typeof(TrailRenderer))] // 强制要求该物体上必须有 TrailRenderer 组件，否则 Unity 会自动添加
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Spin Trail")] // 在 Unity 编辑器的菜单中添加该组件入口
public class PlayerSpinTrail : MonoBehaviour
{
    /// <summary>
    /// 玩家手部的 Transform，用来作为拖尾的挂点。
    /// </summary>
    public Transform hand;

    /// <summary>
    /// 引用到玩家 Player 脚本对象。
    /// </summary>
    protected Player m_player;

    /// <summary>
    /// 拖尾渲染组件 TrailRenderer。
    /// </summary>
    protected TrailRenderer m_trail;

    /// <summary>
    /// 初始化拖尾组件，默认禁用。
    /// </summary>
    protected virtual void InitializeTrail()
    {
        m_trail = GetComponent<TrailRenderer>(); // 获取拖尾组件
        m_trail.enabled = false;                 // 默认关闭拖尾效果
    }

    /// <summary>
    /// 初始化拖尾物体的父子关系。
    /// 把本对象绑定到玩家手部 Transform 上，并重置本地坐标。
    /// </summary>
    protected virtual void InitializeTransform()
    {
        transform.parent = hand;                  // 设置父节点为玩家手
        transform.localPosition = Vector3.zero;   // 重置位置到父节点的中心
    }

    /// <summary>
    /// 初始化玩家引用，并监听玩家状态切换事件。
    /// </summary>
    protected virtual void InitializePlayer()
    {
        m_player = GetComponentInParent<Player>(); // 在父物体上找到 Player 组件
        // 监听玩家状态切换事件，每次切换都会调用 HandleActive()
        m_player.states.events.onChange.AddListener(HandleActive);
    }

    /// <summary>
    /// 响应玩家状态变化。
    /// 如果玩家当前状态是 SpinPlayerState，则启用拖尾；
    /// 否则禁用拖尾。
    /// </summary>
    protected virtual void HandleActive()
    {
        if (m_player.states.IsCurrentOfType(typeof(SpinPlayerState)))
        {
            m_trail.enabled = true;  // 进入旋转状态 → 开启拖尾
        }
        else
        {
            m_trail.enabled = false; // 离开旋转状态 → 关闭拖尾
        }
    }

    /// <summary>
    /// Unity 生命周期方法 Start，在游戏开始时调用。
    /// 顺序执行初始化流程。
    /// </summary>
    protected virtual void Start()
    {
        InitializeTrail();     // 初始化拖尾组件
        InitializeTransform(); // 设置父子关系与位置
        InitializePlayer();    // 绑定玩家与事件监听
    }
}
