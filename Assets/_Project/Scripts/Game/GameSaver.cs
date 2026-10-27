using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

/// <summary>
/// 游戏存档管理器（单例模式）。
/// 负责将游戏数据保存到不同的存储介质（文件或 PlayerPrefs），
/// 并支持读取、删除和批量获取存档。
/// 支持三种存档方式：
/// - Binary（二进制文件存储，体积小，无法直接读取）
/// - JSON（文本可读，方便调试）
/// - PlayerPrefs（存储在注册表或本地配置文件中）
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Saver")]
public class GameSaver : Singleton<GameSaver>
{
    /// <summary>
    /// 存档模式。
    /// </summary>
    public enum Mode { Binary, JSON, PlayerPrefs }

    /// <summary>
    /// 当前使用的存档模式。
    /// 默认使用 JSON：可直接查看和编辑，且不依赖已被 .NET 标记为废弃的 BinaryFormatter。
    /// Binary 模式仅在 Editor / Mono 运行时可用，在 IL2CPP、WebGL 等 AOT 平台上会失效。
    /// </summary>
    public Mode mode = Mode.JSON;

    /// <summary>
    /// 存档文件的基础文件名（不包含扩展名和槽位号）。
    /// </summary>
    public string fileName = "save";

    /// <summary>
    /// 二进制文件的扩展名（默认为 .data）。
    /// </summary>
    public string binaryFileExtension = "data";

    /// <summary>
    /// 存档槽位总数。
    /// 游戏可同时存在的最大存档数。
    /// </summary>
    protected static readonly int TotalSlots = 5;

    /// <summary>
    /// 保存游戏数据到指定的存档槽位。
    /// 根据当前 mode 选择保存方式。
    /// </summary>
    /// <param name="data">要保存的游戏数据对象。</param>
    /// <param name="index">存档槽位编号（0~TotalSlots-1）。</param>
    public virtual void Save(GameData data, int index)
    {
        switch (mode)
        {
            default:
            case Mode.Binary:
                SaveBinary(data, index);
                break;
            case Mode.JSON:
                SaveJSON(data, index);
                break;
            case Mode.PlayerPrefs:
                SavePlayerPrefs(data, index);
                break;
        }
    }

    /// <summary>
    /// 从指定的存档槽位读取游戏数据。
    /// 如果存档不存在，返回 null。
    /// </summary>
    /// <param name="index">存档槽位编号。</param>
    public virtual GameData Load(int index)
    {
        switch (mode)
        {
            default:
            case Mode.Binary:
                return LoadBinary(index);
            case Mode.JSON:
                return LoadJSON(index);
            case Mode.PlayerPrefs:
                return LoadPlayerPrefs(index);
        }
    }

    /// <summary>
    /// 删除指定存档槽位的数据。
    /// </summary>
    /// <param name="index">存档槽位编号。</param>
    public virtual void Delete(int index)
    {
        switch (mode)
        {
            default:
            case Mode.Binary:
            case Mode.JSON:
                DeleteFile(index);
                break;
            case Mode.PlayerPrefs:
                DeletePlayerPrefs(index);
                break;
        }
    }

    /// <summary>
    /// 读取所有存档槽位的游戏数据。
    /// 返回一个长度为 TotalSlots 的数组。
    /// </summary>
    public virtual GameData[] LoadList()
    {
        var list = new GameData[TotalSlots];

        for (int i = 0; i < TotalSlots; i++)
        {
            var data = Load(i);

            if (data != null)
            {
                list[i] = data;
            }
        }

        return list;
    }

    #region Binary 存档方式
    /// <summary>
    /// 以二进制格式保存数据到文件。
    /// 这种方式存储空间小，但不方便直接查看。
    /// 注意：BinaryFormatter 已被 .NET 标记为废弃，存在反序列化安全问题，
    /// 并且在 IL2CPP / WebGL 等 AOT 平台上不可用，因此默认不使用该模式。
    /// 如需真正的跨平台二进制存档，应改用 JsonUtility + 自写二进制读写，或 MessagePack 等第三方序列化。
    /// </summary>
    protected virtual void SaveBinary(GameData data, int index)
    {
        var path = GetFilePath(index);
        var formatter = new BinaryFormatter();
        var stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, data);
        stream.Close();
    }

    /// <summary>
    /// 从二进制文件读取游戏数据。
    /// </summary>
    protected virtual GameData LoadBinary(int index)
    {
        var path = GetFilePath(index);

        if (File.Exists(path))
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream(path, FileMode.Open);
            var data = formatter.Deserialize(stream);
            stream.Close();
            return data as GameData;
        }

        return null;
    }
    #endregion

    #region JSON 存档方式
    /// <summary>
    /// 以 JSON 格式保存数据到文件。
    /// 这种方式可直接查看和编辑，方便调试。
    /// </summary>
    protected virtual void SaveJSON(GameData data, int index)
    {
        var json = data.ToJson();
        var path = GetFilePath(index);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// 从 JSON 文件读取游戏数据。
    /// </summary>
    protected virtual GameData LoadJSON(int index)
    {
        var path = GetFilePath(index);

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            return GameData.FromJson(json);
        }

        return null;
    }
    #endregion

    #region 文件删除
    /// <summary>
    /// 删除指定槽位的存档文件。
    /// </summary>
    protected virtual void DeleteFile(int index)
    {
        var path = GetFilePath(index);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    #endregion

    #region PlayerPrefs 存档方式
    /// <summary>
    /// 将数据保存到 PlayerPrefs（JSON 格式字符串）。
    /// </summary>
    protected virtual void SavePlayerPrefs(GameData data, int index)
    {
        var json = data.ToJson();
        var key = index.ToString();
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save(); // 立即写入磁盘，避免异常退出时丢失存档
    }

    /// <summary>
    /// 从 PlayerPrefs 读取数据。
    /// </summary>
    protected virtual GameData LoadPlayerPrefs(int index)
    {
        var key = index.ToString();

        if (PlayerPrefs.HasKey(key))
        {
            var json = PlayerPrefs.GetString(key);
            return GameData.FromJson(json);
        }

        return null;
    }

    /// <summary>
    /// 删除 PlayerPrefs 中的存档数据。
    /// </summary>
    protected virtual void DeletePlayerPrefs(int index)
    {
        var key = index.ToString();

        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
    #endregion

    #region 文件路径
    /// <summary>
    /// 获取指定存档槽位的文件路径。
    /// 根据当前 mode 自动选择扩展名（.json 或自定义二进制扩展名）。
    /// 存储路径使用 Application.persistentDataPath（持久化目录）。
    /// </summary>
    protected virtual string GetFilePath(int index)
    {
        var extension = mode == Mode.JSON ? "json" : binaryFileExtension;
        return Application.persistentDataPath + "/" + fileName + "_" + index + "." + extension;
    }
    #endregion
}
