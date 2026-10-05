using UnityEngine;

// 要求该组件必须依附在一个带有 Collider 的物体上
[RequireComponent(typeof(Collider))]
// 在 Unity 编辑器的组件菜单中显示该脚本的位置
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Gravity Field")]
public class GravityField : MonoBehaviour
{
    // 施加给玩家的重力场“力”的大小
    public float force = 75f;

    // 缓存当前物体的 Collider 组件
    protected Collider m_collider;


    /// <summary>
    /// 当其他物体进入并停留在该触发器区域时会调用。
    /// 如果检测到是玩家，则对其施加一个向上的力。
    /// </summary>
    /// <param name="other">进入触发器的物体的 Collider</param>
    protected virtual void OnTriggerStay(Collider other)
    {
        // 确认物体带有 "Player" 标签
        if (other.CompareTag(GameTags.Player))
        {
            // 尝试获取 Player 组件（即玩家控制脚本）
            if (other.TryGetComponent<Player>(out var player))
            {
                // 如果玩家处于地面状态，则清空竖直速度
                // 这样避免角色因地面检测而被拉住，确保能被场的力抬起
                if (player.isGrounded)
                {
                    player.verticalVelocity = Vector3.zero;
                }

                // 给玩家的速度施加一个“沿着当前物体的 up 方向”的力
                // Time.deltaTime 确保力是逐帧平滑的
                player.velocity += transform.up * force * Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// 初始化 Collider，将其设置为触发器（Trigger），
    /// 这样物体不会发生物理碰撞，而是仅检测进入/停留/退出的触发事件。
    /// </summary>
    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;
    }
}
