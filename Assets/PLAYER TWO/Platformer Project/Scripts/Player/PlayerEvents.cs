using System;
using UnityEngine.Events;

[Serializable]
public class PlayerEvents
{
    /// <summary>
    /// 当玩家跳跃时调用。
    /// </summary>
    public UnityEvent OnJump;

    /// <summary>
    /// 当玩家受伤时调用。
    /// </summary>
    public UnityEvent OnHurt;
}
