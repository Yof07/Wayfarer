using UnityEngine;

// 将该脚本添加到 Unity 的菜单栏，方便开发者在 Inspector 中快速挂载
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Controller")]
public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// 增加玩家生命值 1 点。
    /// </summary>
    /// <param name="player">玩家对象实例。</param>
    public void AddHealth(Player player) => AddHealth(player, 1);

    /// <summary>
    /// 按指定数值增加玩家生命值。
    /// </summary>
    /// <param name="player">玩家对象实例。</param>
    /// <param name="amount">要增加的生命值数量。</param>
    public void AddHealth(Player player, int amount) => player.health.Increase(amount);
}