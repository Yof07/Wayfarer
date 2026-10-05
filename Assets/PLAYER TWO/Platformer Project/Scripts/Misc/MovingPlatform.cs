using UnityEngine;

// 要求物体必须挂载 WaypointManager 组件（用于管理路径点）
// 要求物体必须挂载 Collider 组件（平台要有碰撞体才能让玩家站上去）
[RequireComponent(typeof(WaypointManager))]
[RequireComponent(typeof(Collider))]
// 在 Unity 的 Inspector 组件菜单中显示
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Moving Platform")]
public class MovingPlatform : MonoBehaviour
{
    [Header("移动设置")]
    // 平台移动速度（单位：米/秒）
    public float speed = 3f;

    // 公开只读属性，表示该平台的路径点管理器
    public WaypointManager waypoints { get; protected set; }

    /// <summary>
    /// 每帧更新：
    /// 平台会不断朝当前目标路径点移动，
    /// 到达目标后自动切换到下一个路径点。
    /// </summary>
    protected virtual void Update()
    {
        // 当前的平台位置
        var position = transform.position;

        // 当前目标路径点位置
        var target = waypoints.current.position;

        // 平滑移动：以 speed 的速度，逐步靠近目标点
        position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);

        // 更新平台位置
        transform.position = position;

        // 如果平台到达了目标路径点
        if (Vector3.Distance(transform.position, target) == 0)
        {
            // 切换到下一个路径点
            waypoints.Next();
        }
    }

    /// <summary>
    /// 初始化：
    /// - 设置平台的 Tag（用于标识为平台）
    /// - 获取并缓存 WaypointManager 组件
    /// </summary>
    protected virtual void Awake()
    {
        // 给平台打上指定的标签，方便检测
        tag = GameTags.Platform;
        // 获取路径点管理器（Waypoints）
        waypoints = GetComponent<WaypointManager>();
    }
}
