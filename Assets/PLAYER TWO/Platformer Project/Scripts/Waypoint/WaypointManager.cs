using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 路点管理器，用于控制对象沿多个路点移动
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Waypoint/Waypoint Manager")] // 编辑器菜单路径
public class WaypointManager : MonoBehaviour
{
    [Header("路点设置")]
    public WaypointMode mode;        // 路点切换模式：PingPong、Loop 或 Once
    public float waitTime;           // 到达路点后的等待时间
    public List<Transform> waypoints; // 路点列表

    protected Transform m_current;   // 当前路点

    protected bool m_pong;           // PingPong 模式下的方向标记
    protected bool m_changing;       // 是否正在切换路点


    /// <summary>
    /// 切换到下一个路点，依据模式不同行为不同
    /// </summary>
    public virtual void Next()
    {
        if (m_changing)
        {
            // 正在切换中时，不执行
            return;
        }

        if (mode == WaypointMode.PingPong)
        {
            // PingPong 模式：往返移动
            if (!m_pong)
            {
                // 如果未反向，判断是否到达末路点
                m_pong = (index + 1 == waypoints.Count);
            }
            else
            {
                // 如果已反向，判断是否到达起始路点
                m_pong = (index - 1 >= 0);
            }

            // 根据方向计算下一个路点索引
            var next = !m_pong ? index + 1 : index - 1;
            StartCoroutine(Change(next));
        }
        else if (mode == WaypointMode.Loop)
        {
            // Loop 模式：循环移动
            if (index + 1 < waypoints.Count)
            {
                StartCoroutine(Change(index + 1));
            }
            else
            {
                // 到达末路点时从头开始
                StartCoroutine(Change(0));
            }
        }
        else if (mode == WaypointMode.Once)
        {
            // Once 模式：到达最后一个路点后停止
            if (index + 1 < waypoints.Count)
            {
                StartCoroutine(Change(index + 1));
            }
        }
    }

    /// <summary>
    /// 切换路点协程，等待一段时间后更新当前路点
    /// </summary>
    /// <param name="to">目标路点索引</param>
    protected virtual IEnumerator Change(int to)
    {
        m_changing = true;                 // 标记正在切换
        yield return new WaitForSeconds(waitTime); // 等待指定时间
        current = waypoints[to];           // 更新当前路点
        m_changing = false;                // 切换完成
    }

    /// <summary>
    /// 当前路点在列表中的索引
    /// </summary>
    public int index => waypoints.IndexOf(current);

    /// <summary>
    /// 当前路点实例
    /// </summary>
    public Transform current
    {
        get
        {
            if (!m_current)
            {
                // 如果当前路点为空，默认为第一个路点
                m_current = waypoints[0];
            }

            return m_current;
        }
        protected set { m_current = value; }
    }
}
