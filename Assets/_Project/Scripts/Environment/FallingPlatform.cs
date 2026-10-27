using System.Collections;
using UnityEngine;

// 挂载在物体上时要求必须有 Collider 组件
[RequireComponent(typeof(Collider))]
// 在 Unity 菜单中的显示路径
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Falling Platform")]
public class FallingPlatform : MonoBehaviour, IEntityContact
{

    // 平台是否会在掉落后自动复位
    public bool autoReset = true;
    // 玩家踩上去后，延迟多久开始掉落
    public float fallDelay = 2f;
    // 平台掉落后，过多久重新复位
    public float resetDelay = 5f;
    // 平台下落的重力速度（越大掉得越快）
    public float fallGravity = 40f;

    [Header("Shake Setting")] // 在 Inspector 面板中显示标题
    public bool shake = true;  // 是否在掉落前进行“抖动”效果
    public float speed = 45f;  // 抖动的频率
    public float height = 0.1f; // 抖动的幅度（上下偏移量）

    // 平台自身的碰撞器
    protected Collider m_collider;
    // 平台的初始位置（用于复位时还原）
    protected Vector3 m_initialPosition;
    /// <summary>
    /// 平台是否已被激活（玩家踩上触发计时）
    /// </summary>
    public bool activated { get; protected set; }
    /// <summary>
    /// 平台是否处于掉落状态
    /// </summary>
    public bool falling { get; protected set; }
    // 存放重叠检测到的碰撞体
    protected Collider[] m_overlaps = new Collider[32];


    /// <summary>
    /// 让平台掉落
    /// </summary>
    public virtual void Fall()
    {
        falling = true;             // 标记为掉落状态
        m_collider.isTrigger = true; // 变为触发器，避免阻挡其他物体
    }

    /// <summary>
    /// 平台掉落前的协程逻辑
    /// </summary>
    protected IEnumerator Routine()
    {
        var timer = fallDelay; // 倒计时

        while (timer >= 0)
        {
            // 在倒计时后半段时播放“抖动效果”
            if (shake && (timer <= fallDelay / 2f))
            {
                var shake = Mathf.Sin(Time.time * speed) * height;
                transform.position = m_initialPosition + Vector3.up * shake;
            }

            timer -= Time.deltaTime; // 递减计时
            yield return null;       // 等待下一帧
        }

        // 倒计时结束后掉落
        Fall();

        // 如果启用自动复位
        if (autoReset)
        {
            yield return new WaitForSeconds(resetDelay); // 等待复位时间
            Restart(); // 恢复平台
        }
    }

    /// <summary>
    /// Unity 生命周期：每帧更新
    /// </summary>
    protected virtual void Update()
    {
        if (falling)
        {
            // 模拟重力下落效果
            transform.position += fallGravity * Vector3.down * Time.deltaTime;
        }
    }

    /// <summary>
    /// 复位平台到最初状态
    /// </summary>
    public virtual void Restart()
    {
        activated = falling = false;        // 取消激活和掉落状态
        transform.position = m_initialPosition; // 回到初始位置
        m_collider.isTrigger = false;       // 重新变为实体碰撞器
        OffsetPlayer();                     // 确保玩家不会被卡在平台里面
    }

    /// <summary>
    /// 防止复位时玩家“卡进”平台，做位置修正
    /// </summary>
    protected virtual void OffsetPlayer()
    {
        var center = m_collider.bounds.center;   // 平台中心
        var extents = m_collider.bounds.extents; // 平台范围
        var maxY = m_collider.bounds.max.y;      // 平台顶部的 y 值

        // 检测平台范围内的所有碰撞体
        var overlaps = Physics.OverlapBoxNonAlloc(center, extents, m_overlaps);

        for (int i = 0; i < overlaps; i++)
        {
            // 只处理玩家
            if (!m_overlaps[i].CompareTag(GameTags.Player))
                continue;

            // 玩家和平台顶部的距离
            var distance = maxY - m_overlaps[i].transform.position.y;
            // 玩家高度
            var height = m_overlaps[i].GetComponent<Player>().height;
            // 计算向上的偏移量（确保玩家在平台之上）
            var offset = Vector3.up * (distance + height * 0.5f);

            // 修正玩家位置
            m_overlaps[i].transform.position += offset;
        }
    }

    /// <summary>
    /// 当有实体（EntityBase）接触到平台时触发
    /// </summary>
    public void OnEntityContact(EntityBase entity)
    {
        // 只有玩家并且站在平台上方时才触发
        if (entity is Player && entity.IsPointUnderStep(m_collider.bounds.max))
        {
            if (!activated) // 防止重复触发
            {
                activated = true; // 标记已被激活
                StartCoroutine(Routine()); // 开始掉落计时协程
            }
        }
    }

    /// <summary>
    /// Unity 生命周期：游戏开始时初始化
    /// </summary>
    protected virtual void Start()
    {
        m_collider = GetComponent<Collider>(); // 获取平台的碰撞器
        m_initialPosition = transform.position; // 记录初始位置
        tag = GameTags.Platform; // 设置标签为 Platform（用于识别）
    }
}
