using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Starter")]
public class LevelStarter : Singleton<LevelStarter>
{

    /// <summary>
    /// 当关卡开始流程完成后触发的事件。
    /// </summary>
    public UnityEvent OnStart;

    /// <summary>
    /// 启用玩家控制的延迟时间（秒）。
    /// </summary>
    public float enablePlayerDelay = 1f;

    protected LevelPauser m_pauser => LevelPauser.instance;

    /// <summary>
    /// MonoBehaviour的启动方法，自动开始关卡启动协程。
    /// </summary>
    protected virtual void Start()
    {
        StartCoroutine(Routine());
    }

    /// <summary>
    /// 关卡开始的协程流程：
    /// 1. 锁定鼠标光标
    /// 2. 禁用玩家控制和输入
    /// 3. 等待指定的延迟时间
    /// 4. 开始计时（关卡时间计数开始）
    /// 5. 启用玩家控制和输入
    /// 6. 允许关卡暂停
    /// 7. 触发关卡开始事件
    /// </summary>
    protected virtual IEnumerator Routine()
    {
        Game.LockCursor();                              // 锁定鼠标光标（隐藏并锁定在窗口中央）
        yield return new WaitForSeconds(enablePlayerDelay); // 延迟等待，通常用于加载动画或准备阶段
        m_pauser.canPause = true;                       // 允许游戏暂停
        OnStart?.Invoke();                             // 触发关卡开始事件，通知其他系统
    }
}
