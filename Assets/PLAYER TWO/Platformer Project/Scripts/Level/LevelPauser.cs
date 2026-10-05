using UnityEngine;
using UnityEngine.Events;

// 游戏关卡暂停控制器（单例模式）
[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Pauser")]
public class LevelPauser : Singleton<LevelPauser>
{
    /// <summary>
    /// 当关卡被暂停时触发的事件
    /// </summary>
    public UnityEvent OnPause;

    /// <summary>
    /// 当关卡被取消暂停时触发的事件
    /// </summary>
    public UnityEvent OnUnpause;

    // 暂停界面的 UI 动画控制器
    public UIAnimator pauseScreen;

    /// <summary>
    /// 是否允许暂停关卡
    /// </summary>
    public bool canPause { get; set; }

    /// <summary>
    /// 当前关卡是否处于暂停状态
    /// </summary>
    public bool paused { get; protected set; }

    /// <summary>
    /// 根据传入的值设置暂停状态
    /// </summary>
    /// <param name="value">要设置的暂停状态（true = 暂停, false = 取消暂停）</param>
    public virtual void Pause(bool value)
    {
        // 如果状态没有变化，则不执行
        if (paused != value)
        {
            // 当前是未暂停状态 -> 需要暂停
            if (!paused)
            {
                // 仅在允许暂停时执行
                if (canPause)
                {
                    Game.LockCursor(false);   // 解锁鼠标
                    paused = true;           // 设置为暂停
                    Time.timeScale = 0;      // 游戏时间停止
                    pauseScreen.SetActive(true); // 显示暂停界面
                    pauseScreen?.Show();     // 播放显示动画
                    OnPause?.Invoke();       // 触发暂停事件
                }
            }
            // 当前是暂停状态 -> 需要取消暂停
            else
            {
                Game.LockCursor();          // 锁定鼠标
                paused = false;             // 设置为非暂停
                Time.timeScale = 1;         // 游戏时间恢复
                pauseScreen?.Hide();        // 播放隐藏动画
                OnUnpause?.Invoke();        // 触发取消暂停事件
            }
        }
    }
}
