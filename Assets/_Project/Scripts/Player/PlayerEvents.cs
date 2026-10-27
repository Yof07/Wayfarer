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
    /// 当玩家受到伤害时调用。
    /// </summary>
    public UnityEvent OnHurt;

    /// <summary>
    /// 当玩家死亡时调用。
    /// </summary>
    public UnityEvent OnDie;

    /// <summary>
    /// 当玩家使用旋转攻击时调用。
    /// </summary>
    public UnityEvent OnSpin;

    /// <summary>
    /// 当玩家拾取物体时调用。
    /// </summary>
    public UnityEvent OnPickUp;

    /// <summary>
    /// 当玩家投掷物体时调用。
    /// </summary>
    public UnityEvent OnThrow;

    /// <summary>
    /// 当玩家开始使用踩踏攻击时调用。
    /// </summary>
    public UnityEvent OnStompStarted;

    /// <summary>
    /// 当玩家在踩踏攻击中开始下落时调用。
    /// </summary>
    public UnityEvent OnStompFalling;

    /// <summary>
    /// 当玩家从踩踏攻击中落地时调用。
    /// </summary>
    public UnityEvent OnStompLanding;

    /// <summary>
    /// 当玩家结束踩踏攻击时调用。
    /// </summary>
    public UnityEvent OnStompEnding;

    /// <summary>
    /// 当玩家抓住边缘时调用。
    /// </summary>
    public UnityEvent OnLedgeGrabbed;

    /// <summary>
    /// 当玩家攀爬上平台边缘时调用。
    /// </summary>
    public UnityEvent OnLedgeClimbing;

    /// <summary>
    /// 当玩家执行空中俯冲时调用。
    /// </summary>
    public UnityEvent OnAirDive;

    /// <summary>
    /// 当玩家执行后空翻时调用。
    /// </summary>
    public UnityEvent OnBackflip;

    /// <summary>
    /// 当玩家开始滑翔时调用。
    /// </summary>
    public UnityEvent OnGlidingStart;

    /// <summary>
    /// 当玩家停止滑翔时调用。
    /// </summary>
    public UnityEvent OnGlidingStop;

    /// <summary>
    /// 当玩家开始冲刺时调用。
    /// </summary>
    public UnityEvent OnDashStarted;

    /// <summary>
    /// 当玩家结束冲刺时调用。
    /// </summary>
    public UnityEvent OnDashEnded;
}
