using System;
using System.Linq;

/// <summary>
/// 关卡数据类
/// 用于保存单个关卡的进度信息，可序列化以便存档或在 Inspector 中显示。
/// </summary>
[Serializable]
public class LevelData
{
    /// <summary>
    /// 关卡是否被锁定
    /// true 表示该关卡尚未解锁，玩家无法进入
    /// </summary>
    public bool locked;

    /// <summary>
    /// 在该关卡中收集到的金币数量
    /// </summary>
    public int coins;

    /// <summary>
    /// 通关耗时（单位：秒）
    /// </summary>
    public float time;

    /// <summary>
    /// 星星收集状态数组
    /// 数组长度由 GameLevel.StarsPerLevel 决定
    /// true 表示对应位置的星星已收集
    /// </summary>
    public bool[] stars = new bool[GameLevel.StarsPerLevel];

    /// <summary>
    /// 获取已收集的星星数量
    /// </summary>
    public int CollectedStars()
    {
        // 统计 stars 数组中值为 true 的个数
        return stars.Where((star) => star).Count();
    }
}
