using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Volume（区域体积触发器）
/// 用于检测物体进入或离开某个区域时触发事件，并可播放对应的音效。
/// </summary>
[RequireComponent(typeof(Collider))] // 确保物体上有 Collider 组件
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Volume")] // 在 Unity 菜单中显示
public class Volume : MonoBehaviour
{

    /// <summary>
    /// 当有物体进入触发区域时调用的事件。
    /// </summary>
    public UnityEvent onEnter;

    /// <summary>
    /// 当有物体离开触发区域时调用的事件。
    /// </summary>
    public UnityEvent onExit;

    /// <summary>
    /// 进入区域时播放的音效。
    /// </summary>
    public AudioClip enterClip;

    /// <summary>
    /// 离开区域时播放的音效。
    /// </summary>
    public AudioClip exitClip;

    /// <summary>
    /// 用于播放音效的音源。
    /// </summary>
    protected AudioSource m_audio;

    /// <summary>
    /// 当前物体上的触发器碰撞体。
    /// </summary>
    protected Collider m_collider;

    /// <summary>
    /// 在游戏开始时初始化 Collider 和 AudioSource。
    /// </summary>
    protected virtual void Start()
    {
        InitializeCollider();
        InitializeAudioSource();
    }

    /// <summary>
    /// 初始化 Collider，并将其设为触发器（Trigger）。
    /// </summary>
    protected virtual void InitializeCollider()
    {
        m_collider = GetComponent<Collider>();
        m_collider.isTrigger = true; // 设置为触发器模式
    }

    /// <summary>
    /// 初始化 AudioSource，如果没有则自动添加。
    /// </summary>
    protected virtual void InitializeAudioSource()
    {
        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        // 设置音源的空间混合（0 = 2D, 1 = 3D, 这里 0.5 表示半 2D 半 3D）
        m_audio.spatialBlend = 0.5f;
    }

    /// <summary>
    /// 当其他物体进入触发区域时调用。
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        // 检查进入物体的边界点是否完全在本区域内
        if (!m_collider.bounds.Contains(other.bounds.max) ||
            !m_collider.bounds.Contains(other.bounds.min))
        {
            // 播放进入音效
            m_audio.PlayOneShot(enterClip);

            // 触发进入事件
            onEnter?.Invoke();
        }
    }

    /// <summary>
    /// 当其他物体离开触发区域时调用。
    /// </summary>
    protected virtual void OnTriggerExit(Collider other)
    {
        // 检查物体的位置是否不在区域内
        if (!m_collider.bounds.Contains(other.transform.position))
        {
            // 播放离开音效
            m_audio.PlayOneShot(exitClip);

            // 触发离开事件
            onExit?.Invoke();
        }
    }

}
