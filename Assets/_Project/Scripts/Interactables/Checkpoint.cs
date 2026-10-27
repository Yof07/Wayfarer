using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))] // 该组件依赖Collider组件
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Checkpoint")] // 组件菜单路径
public class Checkpoint : MonoBehaviour
{
    /// <summary>
    /// 玩家重生点的位置和旋转。
    /// </summary>
    public Transform respawn;

    /// <summary>
    /// 激活检查点时播放的音效。
    /// </summary>
    public AudioClip clip;

    /// <summary>
    /// 当检查点被激活时触发的事件。
    /// </summary>
    public UnityEvent OnActivate;

    // 本组件的Collider引用
    protected Collider m_collider;

    // 本组件的AudioSource引用
    protected AudioSource m_audio;

    /// <summary>
    /// 判断检查点是否已被激活。
    /// </summary>
    public bool activated { get; protected set; }


    /// <summary>
    /// 激活检查点，并设置玩家的重生点位置和旋转。
    /// </summary>
    /// <param name="player">要设置重生点的玩家对象。</param>
    public virtual void Activate(Player player)
    {
        if (!activated)
        {
            activated = true;  // 标记检查点为激活状态
            m_audio.PlayOneShot(clip);  // 播放激活音效
            player.SetRespawn(respawn.position, respawn.rotation);  // 设置玩家重生点
            OnActivate?.Invoke();  // 触发激活事件
        }
    }

    /// <summary>
    /// 当其他碰撞体进入触发器时调用。
    /// </summary>
    /// <param name="other">进入触发器的碰撞体。</param>
    protected virtual void OnTriggerEnter(Collider other)
    {
        // 如果检查点未激活，且碰撞体是玩家
        if (!activated && other.CompareTag(GameTags.Player))
        {
            // 尝试获取玩家组件
            if (other.TryGetComponent<Player>(out var player))
            {
                Activate(player);  // 激活检查点
            }
        }
    }

    /// <summary>
    /// 初始化时调用，获取或添加AudioSource，配置Collider为触发器。
    /// </summary>
    protected virtual void Awake()
    {
        // 尝试获取AudioSource组件，没有则添加一个
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true;  // 设置为触发器模式
    }
}
