using UnityEngine;
using UnityEngine.Events;

// 要求必须挂载 BoxCollider 组件
[RequireComponent(typeof(BoxCollider))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Item Box")]
public class ItemBox : MonoBehaviour, IEntityContact
{
    // 道具箱中包含的可收集物品
    public Collectable[] collectables;

    // 道具箱外观渲染器（用于显示材质变化）
    public MeshRenderer itemBoxRenderer;

    // 道具箱被取空后显示的材质
    public Material emptyItemBoxMaterial;

    [Space(15)]
    // 当道具被收集时触发的事件
    public UnityEvent onCollect;

    // 当道具箱被禁用时触发的事件
    public UnityEvent onDisable;

    // 当前道具索引（指向 collectables 数组中的元素）
    protected int m_index;

    // 是否启用（道具箱是否还能使用）
    protected bool m_enabled = true;

    // 道具箱的初始缩放值
    protected Vector3 m_initialScale;

    // BoxCollider 引用
    protected BoxCollider m_collider;

    /// <summary>
    /// 初始化道具箱中的所有 Collectable。
    /// 如果物品不是隐藏的 -> 先禁用物体，等待触发后再显示。
    /// 如果物品是隐藏的 -> 禁止自动收集，需要手动触发 Collect()。
    /// </summary>
    protected virtual void InitializeCollectables()
    {
        foreach (var collectable in collectables)
        {
            if (!collectable.hidden)
            {
                // 非隐藏道具，先设置为不可见
                collectable.gameObject.SetActive(false);
            }
            else
            {
                // 隐藏道具，关闭“接触即收集”功能，等待手动触发
                collectable.collectOnContact = false;
            }
        }
    }

    /// <summary>
    /// 玩家收集道具箱内的物品。
    /// </summary>
    /// <param name="player">触发收集的玩家对象</param>
    public virtual void Collect(Player player)
    {
        if (m_enabled)
        {
            // 还有未收集的物品
            if (m_index < collectables.Length)
            {
                // 如果该物品是隐藏的 -> 直接调用 Collect()
                if (collectables[m_index].hidden)
                {
                    collectables[m_index].Collect(player);
                }
                else
                {
                    // 否则 -> 激活该物体显示在场景中
                    collectables[m_index].gameObject.SetActive(true);
                }

                // 递增索引，防止越界
                m_index = Mathf.Clamp(m_index + 1, 0, collectables.Length);

                // 触发收集事件（比如音效/特效）
                onCollect?.Invoke();
            }

            // 如果所有物品都被取完 -> 禁用道具箱
            if (m_index == collectables.Length)
            {
                Disable();
            }
        }
    }

    /// <summary>
    /// 禁用道具箱（设置为空材质，并触发事件）。
    /// </summary>
    public virtual void Disable()
    {
        if (m_enabled)
        {
            m_enabled = false;

            // 修改渲染材质为“空箱材质”
            itemBoxRenderer.sharedMaterial = emptyItemBoxMaterial;

            // 触发禁用事件（比如播放空箱音效）
            onDisable?.Invoke();
        }
    }

    /// <summary>
    /// 初始化 BoxCollider、道具箱缩放值，并初始化 Collectables。
    /// </summary>
    protected virtual void Start()
    {
        m_collider = GetComponent<BoxCollider>();
        m_initialScale = transform.localScale;
        InitializeCollectables();
    }

    /// <summary>
    /// 当玩家与道具箱发生碰撞时调用。
    /// 条件：
    /// 1. 必须是 Player
    /// 2. 玩家必须从下方向上碰到箱子（velocity.y > 0 且 玩家 y < 箱子底部 y）
    /// 满足条件则触发 Collect()
    /// </summary>
    public void OnEntityContact(EntityBase entity)
    {
        if (entity is Player player)
        {
            if (entity.velocity.y > 0 &&
                entity.position.y < m_collider.bounds.min.y)
            {
                Collect(player);
            }
        }
    }
}
