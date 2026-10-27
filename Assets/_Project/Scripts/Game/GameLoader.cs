using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 游戏加载器（单例模式）。
/// 用于加载和切换场景，提供加载过程的 UI 反馈，以及触发加载开始与结束事件。
/// 支持设置加载界面、加载延迟、进度获取等功能。
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Loader")]
public class GameLoader : Singleton<GameLoader>
{
    /// <summary>
    /// 当任何加载过程开始时触发的事件。
    /// 可在 Inspector 中绑定方法，例如播放加载动画、暂停游戏等。
    /// </summary>
    public UnityEvent OnLoadStart;

    /// <summary>
    /// 当任何加载过程结束时触发的事件。
    /// 可绑定方法，例如关闭加载动画、恢复游戏等。
    /// </summary>
    public UnityEvent OnLoadFinish;

    /// <summary>
    /// 加载界面 UI 控制器（UIAnimator 用于控制显示/隐藏动画）。
    /// </summary>
    public UIAnimator loadingScreen;

    [Header("Minimum Time")]
    /// <summary>
    /// 场景加载开始前的延迟时间（单位：秒）。
    /// 主要用于在加载界面显示前留出缓冲，让过渡更自然。
    /// </summary>
    public float startDelay = 1f;

    /// <summary>
    /// 场景加载完成后的延迟时间（单位：秒）。
    /// 主要用于加载完成后停留加载界面，避免闪屏。
    /// </summary>
    public float finishDelay = 1f;

    /// <summary>
    /// 当前是否正在加载中。
    /// true 表示正在进行场景加载过程。
    /// </summary>
    public bool isLoading { get; protected set; }

    /// <summary>
    /// 当前的加载进度（0~1）。
    /// 由 SceneManager.LoadSceneAsync 提供。
    /// </summary>
    public float loadingProgress { get; protected set; }

    /// <summary>
    /// 当前场景的名称。
    /// 通过 SceneManager.GetActiveScene().name 获取。
    /// </summary>
    public string currentScene => SceneManager.GetActiveScene().name;

    /// <summary>
    /// 重新加载当前场景。
    /// </summary>
    public virtual void Reload()
    {
        StartCoroutine(LoadRoutine(currentScene));
    }

    /// <summary>
    /// 加载指定名称的场景。
    /// 会在以下条件满足时执行：
    /// - 当前没有正在加载的场景。
    /// - 要加载的场景与当前场景不同。
    /// </summary>
    /// <param name="scene">要加载的场景名称。</param>
    public virtual void Load(string scene)
    {
        if (!isLoading && (currentScene != scene))
        {
            StartCoroutine(LoadRoutine(scene));
        }
    }

    /// <summary>
    /// 场景加载的协程流程。
    /// 包含加载前延迟、加载过程进度记录、加载完成延迟、UI 动画显示等步骤。
    /// </summary>
    /// <param name="scene">要加载的场景名称。</param>
    protected virtual IEnumerator LoadRoutine(string scene)
    {
        // 触发加载开始事件（外部 UI 或逻辑可以监听）
        OnLoadStart?.Invoke();

        // 标记为加载中
        isLoading = true;

        // 激活加载界面并显示动画
        loadingScreen.SetActive(true);
        loadingScreen.Show();

        // 加载前延迟
        yield return new WaitForSeconds(startDelay);

        // 异步加载场景
        var operation = SceneManager.LoadSceneAsync(scene);
        loadingProgress = 0;

        // 在场景加载过程中不断更新进度
        while (!operation.isDone)
        {
            loadingProgress = operation.progress; // progress 取值范围通常是 0~0.9，完成时才为 1
            yield return null; // 等待下一帧
        }

        // 加载完成，强制进度为 1
        loadingProgress = 1;

        // 加载完成后的延迟
        yield return new WaitForSeconds(finishDelay);

        // 标记加载结束
        isLoading = false;

        // 隐藏加载界面
        loadingScreen.Hide();

        // 触发加载结束事件
        OnLoadFinish?.Invoke();
    }
}
