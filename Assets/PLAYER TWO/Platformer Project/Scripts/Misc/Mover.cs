using System.Collections;
using UnityEngine;

public class Mover : MonoBehaviour
{
    [Header("位移设置")]
    // 偏移量，物体将移动到初始位置 + offset 的位置
    public Vector3 offset;

    // 应用偏移的持续时间（秒）
    public float duration;

    // 复位到初始位置的持续时间（秒）
    public float resetDuration;

    // 记录物体的初始局部位置
    protected Vector3 m_initialPosition; 
    
    /// <summary>
    /// 应用偏移效果：
    /// 将物体从初始位置平滑移动到 (初始位置 + offset)。
    /// </summary>
    public virtual void ApplyOffset()
    {
        // 停止所有正在执行的协程，避免动画冲突
        StopAllCoroutines();
        // 启动偏移动画协程
        StartCoroutine(ApplyOffsetRoutine(m_initialPosition, m_initialPosition + offset, duration));
    }
    
    /// <summary>
    /// 通用的平滑移动协程。
    /// 在给定的时间内，将物体从 from 移动到 to。
    /// </summary>
    /// <param name="from">起始位置</param>
    /// <param name="to">目标位置</param>
    /// <param name="duration">持续时间</param>
    protected virtual IEnumerator ApplyOffsetRoutine(Vector3 from, Vector3 to, float duration)
    {
        var elapsedTime = 0f;

        // 在持续时间内逐帧插值
        while (elapsedTime < duration)
        {
            // 计算插值进度（0~1）
            var t = elapsedTime / duration;

            // 使用线性插值 (Lerp) 计算当前位置
            transform.localPosition = Vector3.Lerp(from, to, t);

            // 增加已用时间
            elapsedTime += Time.deltaTime;

            // 等待下一帧
            yield return null;
        }

        // 确保最终精确到达目标位置
        transform.localPosition = to;
    }
    
    /// <summary>
    /// 重置物体位置：
    /// 将物体从当前位置平滑移动回初始位置。
    /// </summary>
    public virtual void Reset()
    {
        StopAllCoroutines();
        StartCoroutine(ApplyOffsetRoutine(transform.localPosition, m_initialPosition, resetDuration));
    }
    
    /// <summary>
    /// 记录初始位置，用于后续的偏移和复位。
    /// </summary>
    protected virtual void Start()
    {
        m_initialPosition = transform.localPosition;
    }
}