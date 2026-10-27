using UnityEngine;


// 在 Unity 的菜单中显示为 Misc/Floater
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Floater")]
public class Floater : MonoBehaviour
{
    // 浮动的速度（控制波动的频率，越大越快）
    public float speed = 2f;

    // 浮动的振幅（上下移动的幅度，越大越明显）
    public float amplitude = 0.5f;

    /// <summary>
    /// Unity 生命周期方法：在每一帧的 LateUpdate 阶段调用
    /// 用于产生上下浮动的效果
    /// </summary>
    protected virtual void LateUpdate()
    {
        // 计算一个随时间变化的正弦波值
        // Time.time * speed 控制频率（快慢）
        // 再乘上 amplitude 控制振幅（幅度大小）
        var wave = Mathf.Sin(Time.time * speed) * amplitude;

        // 根据物体自身的 up 方向，叠加一个位移
        // wave * Time.deltaTime 保证每帧移动平滑
        transform.position += transform.up * wave * Time.deltaTime;
    }
}