using System;
using UnityEngine;

/// <summary>
/// 表示一个游戏关卡的数据结构。
/// 用于记录关卡的状态（是否解锁、收集的金币数量、通关时间、星星收集情况等）。
/// 该类主要用于存储与读取关卡数据，以及格式化关卡时间显示。
/// </summary>
[Serializable] // 允许此类被序列化（可在Inspector中显示，也方便保存到文件）
public class GameLevel
{
    /// <summary>
    /// 是否锁定此关卡。
    /// true 表示锁定（无法进入），false 表示解锁（可进入）。
    /// </summary>
    public bool locked;

    /// <summary>
    /// 关卡所对应的场景名称（Unity Scene 名称）。
    /// 用于场景切换时加载。
    /// </summary>
    public string scene;

    /// <summary>
    /// 关卡的显示名称。
    /// 例如：“第一关 - 森林冒险”。
    /// </summary>
    public string name;

    /// <summary>
    /// 关卡描述信息。
    /// 可用于UI中展示关卡的背景故事或提示。
    /// </summary>
    public string description;

    /// <summary>
    /// 关卡预览图片（Sprite）。
    /// 可用于UI中展示关卡缩略图。
    /// </summary>
    public Sprite image;

    /// <summary>
    /// 已收集的金币数量。
    /// 可通过属性进行读取或写入。
    /// </summary>
    public int coins { get; set; }

    /// <summary>
    /// 该关卡的通关时间（单位：秒）。
    /// 记录最快通关时间。
    /// </summary>
    public float time { get; set; }

    /// <summary>
    /// 星星收集状态数组。
    /// 每个布尔值表示该星星是否已收集（true = 已收集）。
    /// 默认长度为 StarsPerLevel（通常为3颗星）。
    /// </summary>
    public bool[] stars { get; set; } = new bool[StarsPerLevel];

    /// <summary>
    /// 每个关卡的星星总数（固定值）。
    /// 通常为3颗星。
    /// </summary>
    public static readonly int StarsPerLevel = 3;

    /// <summary>
    /// 从给定的 LevelData 加载此关卡的状态。
    /// 用于数据反序列化或读取存档。
    /// </summary>
    /// <param name="data">包含关卡状态的 LevelData 对象。</param>
    public virtual void LoadState(LevelData data)
    {
        locked = data.locked;  // 是否锁定
        coins = data.coins;    // 收集金币数
        time = data.time;      // 最佳通关时间
        stars = data.stars;    // 星星收集状态
    }

    /// <summary>
    /// 将当前 GameLevel 对象转换为 LevelData 对象。
    /// 用于存档或数据保存。
    /// </summary>
    /// <returns>包含当前关卡状态的 LevelData 对象。</returns>
    public virtual LevelData ToData()
    {
        return new LevelData()
        {
            locked = this.locked,
            coins = this.coins,
            time = this.time,
            stars = this.stars
        };
    }

    /// <summary>
    /// 将给定的时间（秒）格式化为 00'00"00 的字符串格式。
    /// 示例：65.23 秒 -> "1'05"23"（1分5秒23毫秒）
    /// </summary>
    /// <param name="time">需要格式化的时间（单位：秒）。</param>
    /// <returns>格式化后的时间字符串，例如 "02'34"56"。</returns>
    public static string FormattedTime(float time)
    {
        // 计算分钟数
        var minutes = Mathf.FloorToInt(time / 60f);

        // 计算剩余的秒数（去掉分钟后的部分）
        var seconds = Mathf.FloorToInt(time % 60f);

        // 计算毫秒（保留两位，取百分位）
        var milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        // 返回格式化后的字符串
        return minutes.ToString("0") + "'" + seconds.ToString("00") + "\"" + milliseconds.ToString("00");
    }
}
