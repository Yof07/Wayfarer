using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 游戏核心管理类，单例模式。
/// 负责游戏的整体状态管理，如关卡列表、重试次数、保存与加载等。
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game")]
public class Game : Singleton<Game>
{
    /// <summary>
    /// 当重试次数改变时触发，带有当前重试次数参数。
    /// </summary>
    public UnityEvent<int> OnRetriesSet;

    /// <summary>
    /// 当请求保存游戏时触发。
    /// </summary>
    public UnityEvent OnSavingRequested;

    /// <summary>
    /// 初始重试次数，游戏开始时赋值给 retries。
    /// </summary>
    public int initialRetries = 3;

    /// <summary>
    /// 游戏包含的所有关卡列表。
    /// </summary>
    public List<GameLevel> levels;

    /// <summary>
    /// 当前剩余的重试次数，封装字段。
    /// </summary>
    protected int m_retries;

    /// <summary>
    /// 当前游戏数据索引，标识加载或保存的存档槽。
    /// </summary>
    protected int m_dataIndex;

    /// <summary>
    /// 游戏存档创建时间。
    /// </summary>
    protected DateTime m_createdAt;

    /// <summary>
    /// 游戏存档最后更新时间。
    /// </summary>
    protected DateTime m_updatedAt;

    /// <summary>
    /// 当前游戏剩余的重试次数属性，设置时会触发 OnRetriesSet 事件。
    /// </summary>
    public int retries
    {
        get { return m_retries; }
        set
        {
            m_retries = value;
            // 通知监听者重试次数已改变
            OnRetriesSet?.Invoke(m_retries);
        }
    }

    /// <summary>
    /// 设置鼠标指针的锁定和显示状态。
    /// 仅在 Standalone 和 WebGL 平台生效。
    /// </summary>
    /// <param name="value">为 true 时隐藏并锁定光标，为 false 时显示并释放锁定。</param>
    public static void LockCursor(bool value = true)
    {
#if UNITY_STANDALONE || UNITY_WEBGL
        Cursor.visible = value;
        Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
    }

    /// <summary>
    /// 根据给定的存档索引和数据，加载游戏状态。
    /// </summary>
    /// <param name="index">存档索引。</param>
    /// <param name="data">存档数据对象。</param>
    public virtual void LoadState(int index, GameData data)
    {
        m_dataIndex = index;
        m_retries = data.retries;
        m_createdAt = DateTime.Parse(data.createdAt);
        m_updatedAt = DateTime.Parse(data.updatedAt);

        // 依次将各关卡加载对应的存档数据
        for (int i = 0; i < data.levels.Length; i++)
        {
            levels[i].LoadState(data.levels[i]);
        }
    }

    /// <summary>
    /// 获取当前游戏所有关卡数据的数组，用于保存。
    /// </summary>
    /// <returns>关卡数据数组。</returns>
    public virtual LevelData[] LevelsData()
    {
        // 通过 LINQ 将每个关卡转换成 LevelData 类型数组
        return levels.Select(level => level.ToData()).ToArray();
    }

    /// <summary>
    /// 获取当前场景对应的游戏关卡实例，如果当前场景不是关卡，返回 null。
    /// </summary>
    /// <returns>当前关卡对象或 null。</returns>
    public virtual GameLevel GetCurrentLevel()
    {
        var scene = GameLoader.instance.currentScene;
        return levels.Find((level) => level.scene == scene);
    }

    /// <summary>
    /// 获取当前场景对应的关卡在关卡列表中的索引。
    /// </summary>
    /// <returns>当前关卡索引，找不到返回 -1。</returns>
    public virtual int GetCurrentLevelIndex()
    {
        var scene = GameLoader.instance.currentScene;
        return levels.FindIndex((level) => level.scene == scene);
    }

    /// <summary>
    /// 请求保存当前游戏数据，调用保存系统并触发保存事件。
    /// </summary>
    public virtual void RequestSaving()
    {
        GameSaver.instance.Save(ToData(), m_dataIndex);
        OnSavingRequested?.Invoke();
    }

    /// <summary>
    /// 解锁下一个关卡，使其可进入。
    /// </summary>
    public virtual void UnlockNextLevel()
    {
        var index = GetCurrentLevelIndex() + 1;

        if (index >= 0 && index < levels.Count)
        {
            levels[index].locked = false;
        }
    }

    /// <summary>
    /// 将当前游戏状态转换为可保存的 GameData 对象。
    /// </summary>
    /// <returns>表示当前游戏状态的数据对象。</returns>
    public virtual GameData ToData()
    {
        return new GameData()
        {
            retries = m_retries,
            levels = LevelsData(),
            createdAt = m_createdAt.ToString(),
            updatedAt = DateTime.UtcNow.ToString()
        };
    }

    /// <summary>
    /// Unity 生命周期 Awake，初始化单例和默认重试次数，防止游戏对象销毁。
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        retries = initialRetries;
        DontDestroyOnLoad(gameObject);
    }
}
