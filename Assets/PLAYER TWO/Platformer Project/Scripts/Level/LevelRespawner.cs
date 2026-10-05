using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Respawner")]
public class LevelRespawner : Singleton<LevelRespawner>
{
    /// <summary>
    /// 在复活流程结束后调用的事件。
    /// </summary>
    public UnityEvent OnRespawn;

    /// <summary>
    /// 在游戏结束（Game Over）流程结束后调用的事件。
    /// </summary>
    public UnityEvent OnGameOver;

    // 复活淡出延迟时间
    public float respawnFadeOutDelay = 1f;
    // 复活淡入延迟时间
    public float respawnFadeInDelay = 0.5f;
    // 游戏结束淡出延迟时间
    public float gameOverFadeOutDelay = 5f;
    // 重启淡出延迟时间
    public float restartFadeOutDelay = 0.5f;

    // 存储场景中的所有玩家摄像机列表
    protected List<PlayerCamera> m_cameras;

    // 快捷访问当前关卡实例
    protected Level m_level => Level.instance;
    // 快捷访问当前关卡分数管理实例
    protected LevelScore m_score => LevelScore.instance;
    // 快捷访问暂停管理实例
    protected LevelPauser m_pauser => LevelPauser.instance;
    // 快捷访问游戏管理实例
    protected Game m_game => Game.instance;
    // 快捷访问画面淡入淡出管理实例
    protected Fader m_fader => Fader.instance;

    /// <summary>
    /// 复活的协程流程。
    /// 如果 consumeRetries 为真，则扣除一次重试机会。
    /// 重置玩家状态，重置分数，重置摄像机，触发复活事件，播放淡入动画，恢复玩家控制和允许暂停。
    /// </summary>
    protected virtual IEnumerator RespawnRoutine(bool consumeRetries)
    {
        if (consumeRetries)
        {
            m_game.retries--;
        }

        m_level.player.Respawn();
        m_score.coins = 0;
        ResetCameras();
        OnRespawn?.Invoke();

        yield return new WaitForSeconds(respawnFadeInDelay);

        m_fader.FadeIn(() =>
        {
            m_pauser.canPause = true;
            m_level.player.inputs.enabled = true;
        });
    }

    /// <summary>
    /// 游戏结束的协程流程。
    /// 停止计时，等待游戏结束淡出延迟，重新加载场景，触发游戏结束事件。
    /// </summary>
    protected virtual IEnumerator GameOverRoutine()
    {
        m_score.stopTime = true;
        yield return new WaitForSeconds(gameOverFadeOutDelay);
        GameLoader.instance.Reload();
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// 重新开始关卡的协程流程。
    /// 取消暂停，禁止暂停，禁用玩家输入，等待重启淡出延迟，重新加载当前场景。
    /// </summary>
    protected virtual IEnumerator RestartRoutine()
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.inputs.enabled = false;
        yield return new WaitForSeconds(restartFadeOutDelay);
        GameLoader.instance.Reload();
    }

    /// <summary>
    /// 复活或游戏结束的统一流程。
    /// 如果消耗重试次数且重试次数为0，则进入游戏结束流程。
    /// 否则先等待复活淡出延迟，然后播放淡出动画，之后进入复活流程。
    /// </summary>
    protected virtual IEnumerator Routine(bool consumeRetries)
    {
        m_pauser.Pause(false);
        m_pauser.canPause = false;
        m_level.player.inputs.enabled = false;

        if (consumeRetries && m_game.retries == 0)
        {
            StartCoroutine(GameOverRoutine());
            yield break;
        }

        yield return new WaitForSeconds(respawnFadeOutDelay);

        m_fader.FadeOut(() => StartCoroutine(RespawnRoutine(consumeRetries)));
    }

    /// <summary>
    /// 重置所有摄像机状态，调用每个摄像机的 Reset 方法。
    /// </summary>
    protected virtual void ResetCameras()
    {
        foreach (var camera in m_cameras)
        {
            camera.Reset();
        }
    }

    /// <summary>
    /// 根据重试次数调用复活流程或游戏结束流程。
    /// 参数 consumeRetries 指示是否扣除重试次数。
    /// </summary>
    public virtual void Respawn(bool consumeRetries)
    {
        StopAllCoroutines();
        StartCoroutine(Routine(consumeRetries));
    }

    /// <summary>
    /// 重新开始当前关卡，重新加载当前场景。
    /// </summary>
    public virtual void Restart()
    {
        StopAllCoroutines();
        StartCoroutine(RestartRoutine());
    }

    /// <summary>
    /// 初始化方法，查找场景中所有玩家摄像机，监听玩家死亡事件自动触发复活。
    /// </summary>
    protected virtual void Start()
    {
        m_cameras = new List<PlayerCamera>(FindObjectsOfType<PlayerCamera>());
        m_level.player.playerEvents.OnDie.AddListener(() => Respawn(true));
    }
}
