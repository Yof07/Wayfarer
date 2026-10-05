using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏 HUD（Heads-Up Display）管理类
/// 显示玩家信息，如重试次数、金币、生命值、计时器和关卡星级
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/UI/HUD")]
public class HUD : MonoBehaviour
{
    // 字符串格式，用于显示重试次数、金币和生命值
    public string retriesFormat = "00";
    public string coinsFormat = "000";
    public string healthFormat = "0";

    [Header("UI Elements")]
    // UI 文本组件
    public Text retries;          // 显示重试次数
    public Text coins;            // 显示金币数量
    public Text health;           // 显示玩家生命值
    public Text timer;            // 显示关卡计时
    public Image[] starsImages;   // 显示关卡获得的星级图标

    // 内部引用
    protected Game m_game;        // 游戏管理实例
    protected LevelScore m_score; // 当前关卡分数实例
    protected Player m_player;    // 玩家实例

    protected float timerStep;                 // 计时器累加值，用于刷新 UI
    protected static float timerRefreshRate = .1f; // 计时器刷新间隔（秒）

    /// <summary>
    /// 更新金币显示
    /// </summary>
    protected virtual void UpdateCoins(int value)
    {
        coins.text = value.ToString(coinsFormat); // 格式化显示
    }

    /// <summary>
    /// 更新重试次数显示
    /// </summary>
    protected virtual void UpdateRetries(int value)
    {
        retries.text = value.ToString(retriesFormat);
    }

    /// <summary>
    /// 更新玩家生命值显示
    /// </summary>
    protected virtual void UpdateHealth()
    {
        health.text = m_player.health.current.ToString(healthFormat);
    }

    /// <summary>
    /// 根据布尔数组更新星级显示状态
    /// </summary>
    protected virtual void UpdateStars(bool[] value)
    {
        for (int i = 0; i < starsImages.Length; i++)
        {
            starsImages[i].enabled = value[i]; // true 显示，false 隐藏
        }
    }

    /// <summary>
    /// 更新关卡计时器显示
    /// </summary>
    protected virtual void UpdateTimer()
    {
        timerStep += Time.deltaTime; // 累加时间

        if (timerStep >= timerRefreshRate) // 达到刷新间隔
        {
            var time = m_score.time;
            timer.text = GameLevel.FormattedTime(m_score.time); // 格式化时间显示
            timerStep = 0;
        }
    }

    /// <summary>
    /// 强制刷新 HUD 所有显示
    /// </summary>
    public virtual void Refresh()
    {
        UpdateCoins(m_score.coins);        // 刷新金币
        UpdateRetries(m_game.retries);      // 刷新重试次数
        UpdateHealth();                     // 刷新生命值
        UpdateStars(m_score.stars);         // 刷新星级
    }

    /// <summary>
    /// 初始化 HUD，绑定事件监听
    /// </summary>
    protected virtual void Awake()
    {
        m_game = Game.instance;           // 获取游戏实例
        m_score = LevelScore.instance;    // 获取关卡分数实例
        m_player = FindObjectOfType<Player>(); // 查找场景中的 Player 实例

        // 关卡分数加载完成后绑定 UI 更新事件
        m_score.OnScoreLoaded.AddListener(() =>
        {
            m_score.OnCoinsSet.AddListener(UpdateCoins);       // 金币更新事件
            m_score.OnStarsSet.AddListener(UpdateStars);       // 星级更新事件
            m_game.OnRetriesSet.AddListener(UpdateRetries);    // 重试次数更新事件
            m_player.health.onChange.AddListener(UpdateHealth);// 玩家生命值变化事件
            Refresh();                                        // 初始化刷新 HUD
        });
    }

    /// <summary>
    /// 每帧更新计时器
    /// </summary>
    protected virtual void Update() => UpdateTimer();
}
