using UnityEngine;

/// <summary>
/// 星星收集物（Star）
/// 玩家碰到后会被收集，并记录到关卡分数系统中。
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Star")] // 在 Unity 菜单中显示
public class Star : Collectable
{
    /// <summary>
    /// 星星的索引（对应关卡中的第几个星星）。
    /// </summary>
    public int index;

    /// <summary>
    /// 当前关卡的分数管理器（单例）。
    /// </summary>
    protected LevelScore m_score => LevelScore.instance;

    /// <summary>
    /// 禁用当前星星（隐藏）。
    /// </summary>
    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 玩家收集星星时触发。
    /// </summary>
    /// <param name="player">收集星星的玩家。</param>
    public override void Collect(Player player)
    {
        // 通知关卡分数系统：收集了 index 对应的星星
        m_score.CollectStar(index);

        // 调用父类的 Collect（播放音效/特效等）
        base.Collect(player);
    }

    /// <summary>
    /// 初始化方法。
    /// </summary>
    protected override void Awake()
    {
        // 先执行 Collectable 的初始化逻辑
        base.Awake();

        // 当分数数据加载完成时，检查该星星是否已经被收集
        m_score.OnScoreLoaded.AddListener(() =>
        {
            // 如果该星星在存档里已被收集，则禁用它（不再显示）
            if (m_score.stars[index])
            {
                Disable();
            }
        });
    }
}
