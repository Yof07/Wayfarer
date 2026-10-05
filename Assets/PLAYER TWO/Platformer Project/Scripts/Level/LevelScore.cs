using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Score")]
public class LevelScore : Singleton<LevelScore>
{
    /// <summary>
    /// 当金币数量发生变化时触发的事件，传递当前金币数量。
    /// </summary>
    public UnityEvent<int> OnCoinsSet;

    /// <summary>
    /// 当已收集的星星数组发生变化时触发的事件，传递当前星星状态数组。
    /// </summary>
    public UnityEvent<bool[]> OnStarsSet;

    /// <summary>
    /// 当关卡数据完全加载后触发的事件。
    /// </summary>
    public UnityEvent OnScoreLoaded;

    /// <summary>
    /// 当前关卡已收集的金币数量。
    /// </summary>
    public int coins
    {
        get { return m_coins; }
        set
        {
            m_coins = value;
            OnCoinsSet?.Invoke(m_coins); // 更新金币时触发事件通知
        }
    }

    /// <summary>
    /// 当前关卡收集的星星数组（返回克隆，防止外部修改原数据）。
    /// </summary>
    public bool[] stars => (bool[])m_stars.Clone();

    /// <summary>
    /// 当前关卡开始以来经过的时间。
    /// </summary>
    public float time { get; protected set; }

    /// <summary>
    /// 控制时间计数器是否暂停更新，默认暂停。
    /// </summary>
    public bool stopTime { get; set; } = true;

    // 当前金币数的私有字段
    protected int m_coins;
    // 当前星星收集状态的私有数组
    protected bool[] m_stars = new bool[GameLevel.StarsPerLevel];

    // 游戏实例引用
    protected Game m_game;
    // 当前关卡实例引用
    protected GameLevel m_level;

    /// <summary>
    /// 重置关卡分数到默认状态（时间归零，金币归零，星星状态从当前关卡复制）。
    /// </summary>
    public virtual void Reset()
    {
        time = 0;
        coins = 0;

        if (m_level != null)
        {
            m_stars = (bool[])m_level.stars.Clone();
        }
    }

    /// <summary>
    /// 收集指定索引的星星，更新状态并触发事件通知。
    /// </summary>
    /// <param name="index">要收集的星星索引。</param>
    public virtual void CollectStar(int index)
    {
        m_stars[index] = true;
        OnStarsSet?.Invoke(m_stars);
    }

    /// <summary>
    /// 将当前关卡分数数据复制回关卡实例，并请求游戏保存数据。
    /// 会更新时间（如果当前时间更短或未设置），金币（若当前金币更多）和星星状态。
    /// </summary>
    public virtual void Consolidate()
    {
        if (m_level != null)
        {
            // 若关卡时间未设置或当前用时更短，更新关卡时间
            if (m_level.time == 0 || time < m_level.time)
            {
                m_level.time = time;
            }

            // 若当前金币更多，更新关卡金币
            if (coins > m_level.coins)
            {
                m_level.coins = coins;
            }

            // 更新关卡星星状态
            m_level.stars = (bool[])stars.Clone();

            // 请求游戏保存数据
            m_game.RequestSaving();
        }
    }

    /// <summary>
    /// 初始化方法，获取游戏和当前关卡实例，复制当前关卡星星状态，触发分数加载完成事件。
    /// </summary>
    protected virtual void Start()
    {
        m_game = Game.instance;
        m_level = m_game?.GetCurrentLevel();

        if (m_level != null)
        {
            m_stars = (bool[])m_level.stars.Clone();
        }

        OnScoreLoaded?.Invoke();
    }

    /// <summary>
    /// 每帧调用，若时间计数未暂停，则累加经过的时间。
    /// </summary>
    protected virtual void Update()
    {
        if (!stopTime)
        {
            time += Time.deltaTime;
        }
    }
}
