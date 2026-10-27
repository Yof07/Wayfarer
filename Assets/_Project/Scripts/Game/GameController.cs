using UnityEngine;

/// <summary>
/// 游戏控制器
/// 提供访问全局游戏状态（Game 单例）的便捷入口，
/// 主要供事件系统调用，例如收集生命道具时增加剩余生命。
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Controller")] // 在 Unity 菜单中显示
public class GameController : MonoBehaviour
{
    /// <summary>
    /// 全局游戏状态（Game 单例）的快捷访问。
    /// </summary>
    protected Game m_game => Game.instance;

    /// <summary>
    /// 场景加载器（GameLoader 单例）的快捷访问。
    /// </summary>
    protected GameLoader m_loader => GameLoader.instance;

    /// <summary>
    /// 增加剩余生命（重试次数）。
    /// </summary>
    /// <param name="amount">增加的数量</param>
    public virtual void AddRetries(int amount) => m_game.retries += amount;

    /// <summary>
    /// 加载指定名称的场景（供 UI 按钮调用）。
    /// </summary>
    /// <param name="scene">目标场景名称</param>
    public virtual void LoadScene(string scene) => m_loader.Load(scene);
}
