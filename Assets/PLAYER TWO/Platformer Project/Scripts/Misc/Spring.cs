using UnityEngine;

/// <summary>
/// 弹簧组件（Spring）
/// 玩家踩上去时会被弹起，并附带音效。
/// 实现 IEntityContact 接口，用于和实体接触时触发逻辑。
/// </summary>
[RequireComponent(typeof(Collider))] // 要求物体上必须有 Collider 组件
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Spring")] // 添加到 Unity 的组件菜单中
public class Spring : MonoBehaviour, IEntityContact
{
    /// <summary>
    /// 弹簧施加的向上力大小。
    /// </summary>
    public float force = 25f;

    /// <summary>
    /// 弹簧触发时播放的音效。
    /// </summary>
    public AudioClip clip;

    /// <summary>
    /// 音效播放器。
    /// </summary>
    protected AudioSource m_audio;

    /// <summary>
    /// 碰撞体，用于检测玩家是否接触到弹簧。
    /// </summary>
    protected Collider m_collider;


    /// <summary>
    /// 对指定玩家施加弹簧的向上力。
    /// </summary>
    /// <param name="player">要施加力的玩家对象。</param>
    public void ApplyForce(Player player)
    {
        // 仅当玩家当前竖直速度向下（y <= 0）时才触发弹跳
        if (player.verticalVelocity.y <= 0)
        {
            // 播放弹簧音效
            m_audio.PlayOneShot(clip);

            // 设置玩家的竖直速度为向上的力
            player.verticalVelocity = Vector3.up * force;
        }
    }


    /// <summary>
    /// 当有实体接触到弹簧时触发。
    /// IEntityContact 接口的实现。
    /// </summary>
    /// <param name="entity">接触弹簧的实体对象。</param>
    public void OnEntityContact(EntityBase entity)
    {
        // 检查实体是否从弹簧的上方踩下（碰撞点在碰撞体顶部附近）
        // 并且该实体是 Player 且玩家还活着
        if (entity.IsPointUnderStep(m_collider.bounds.max) &&
            entity is Player player && player.isAlive)
        {
            // 施加弹簧力
            ApplyForce(player);

            // 重置玩家空中状态（恢复一次跳跃、重置旋转和冲刺）
            player.SetJumps(1);
            player.ResetAirSpins();
            player.ResetAirDash();

            // 强制切换到“下落状态”
            player.states.Change<FallPlayerState>();
        }
    }

    /// <summary>
    /// 初始化，设置标签、获取组件。
    /// </summary>
    protected virtual void Start()
    {
        // 设置标签为 Spring
        tag = GameTags.Spring;

        // 获取碰撞体
        m_collider = GetComponent<Collider>();

        // 获取音效组件，如果没有则自动添加一个
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }
    }
}
