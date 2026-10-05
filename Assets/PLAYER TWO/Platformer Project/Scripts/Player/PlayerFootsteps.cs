using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Footsteps")]
public class PlayerFootsteps : MonoBehaviour
{
    [System.Serializable]
    public class Surface
    {
        /// <summary>
        /// 表示地面类型的标签
        /// </summary>
        public string tag;

        /// <summary>
        /// 玩家在该地面上的脚步声音数组
        /// </summary>
        public AudioClip[] footsteps;

        /// <summary>
        /// 玩家在该地面上的落地声音数组
        /// </summary>
        public AudioClip[] landings;
    }

    /// <summary>
    /// 各种地面类型及其对应的音效配置
    /// </summary>
    public Surface[] surfaces;

    /// <summary>
    /// 默认脚步声（当未匹配到地面类型时使用）
    /// </summary>
    public AudioClip[] defaultFootsteps;

    /// <summary>
    /// 默认落地声（当未匹配到地面类型时使用）
    /// </summary>
    public AudioClip[] defaultLandings;

    /// <summary>
    /// 玩家引用
    /// </summary>
    protected Player m_player;

    /// <summary>
    /// 播放音效的组件
    /// </summary>
    protected AudioSource m_audio;

    [Header("General Settings")]
    /// <summary>
    /// 玩家每走多少距离触发一次脚步声
    /// </summary>
    public float stepOffset = 1.25f;

    /// <summary>
    /// 脚步声音量
    /// </summary>
    public float footstepVolume = 0.5f;

    /// <summary>
    /// 上一次的水平位置（用于计算移动距离）
    /// </summary>
    protected Vector3 m_lastLateralPosition;

    /// <summary>
    /// 存储不同地面类型的脚步声映射
    /// </summary>
    protected Dictionary<string, AudioClip[]> m_footsteps = new Dictionary<string, AudioClip[]>();

    /// <summary>
    /// 存储不同地面类型的落地声映射
    /// </summary>
    protected Dictionary<string, AudioClip[]> m_landings = new Dictionary<string, AudioClip[]>();

    /// <summary>
    /// 初始化：绑定玩家事件，设置音效组件，加载地面音效数据
    /// </summary>
    protected virtual void Start()
    {
        m_player = GetComponent<Player>();
        m_player.entityEvents.OnGroundEnter.AddListener(Landing);

        if (!TryGetComponent(out m_audio))
        {
            m_audio = gameObject.AddComponent<AudioSource>();
        }

        foreach (var surface in surfaces)
        {
            m_footsteps.Add(surface.tag, surface.footsteps);
            m_landings.Add(surface.tag, surface.landings);
        }
    }

    /// <summary>
    /// 每帧检测玩家是否在行走，并在移动达到步长时播放脚步声
    /// </summary>
    protected virtual void Update()
    {
        if (m_player.isGrounded && m_player.states.IsCurrentOfType(typeof(WalkPlayerState)))
        {
            var position = transform.position;
            var lateralPosition = new Vector3(position.x, 0, position.z);
            var distance = (m_lastLateralPosition - lateralPosition).magnitude;

            if (distance >= stepOffset)
            {
                if (m_footsteps.ContainsKey(m_player.groundHit.collider.tag))
                {
                    PlayRandomClip(m_footsteps[m_player.groundHit.collider.tag]);
                }
                else
                {
                    PlayRandomClip(defaultFootsteps);
                }

                m_lastLateralPosition = lateralPosition;
            }
        }
    }

    /// <summary>
    /// 播放落地音效（根据地面类型选择对应的音效）
    /// </summary>
    protected virtual void Landing()
    {
        if (!m_player.onWater)
        {
            if (m_landings.ContainsKey(m_player.groundHit.collider.tag))
            {
                PlayRandomClip(m_landings[m_player.groundHit.collider.tag]);
            }
            else
            {
                PlayRandomClip(defaultLandings);
            }
        }
    }

    /// <summary>
    /// 从给定的音效数组中随机播放一个音效
    /// </summary>
    protected virtual void PlayRandomClip(AudioClip[] clips)
    {
        if (clips.Length > 0)
        {
            var index = Random.Range(0, clips.Length);
            m_audio.PlayOneShot(clips[index], footstepVolume);
        }
    }
}
