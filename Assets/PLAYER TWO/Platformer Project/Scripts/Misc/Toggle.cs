
using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Toggle（开关组件）
/// 用于控制某个状态的开启/关闭，并能触发事件或联动其它 Toggle。
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Toggle")] // 在 Unity 菜单中显示
public class Toggle : MonoBehaviour
{
    /// <summary>
    /// 当前的开关状态（true = 开启，false = 关闭）。
    /// </summary>
    public bool state = true;

    /// <summary>
    /// 切换状态时的延迟时间（秒）。
    /// </summary>
    public float delay;

    /// <summary>
    /// 关联的其他 Toggle，当当前 Toggle 状态切换时，这些 Toggle 会同步切换。
    /// </summary>
    public Toggle[] multiTrigger;

    /// <summary>
    /// 当开关被激活（开启）时触发的事件。
    /// </summary>
    public UnityEvent onActivate;

    /// <summary>
    /// 当开关被关闭时触发的事件。
    /// </summary>
    public UnityEvent onDeactivate;

    /// <summary>
    /// 设置开关状态。
    /// </summary>
    /// <param name="value">要设置的目标状态（true 开启 / false 关闭）。</param>
    public virtual void Set(bool value)
    {
        // 停止所有正在执行的协程，防止重复切换 
        StopAllCoroutines();

        // 启动新的协程，执行切换逻辑（带延迟）
        StartCoroutine(SetRoutine(value));
    }

    /// <summary>
    /// 协程：在延迟后执行开关状态的切换。
    /// </summary>
    protected virtual IEnumerator SetRoutine(bool value)
    {
        // 等待 delay 秒
        yield return new WaitForSeconds(delay);

        if (value) // 目标状态 = 开启
        {
            // 如果当前是关闭状态，则执行开启逻辑
            if (!state)
            {
                state = true;

                // 级联触发其它 Toggle
                foreach (var toggle in multiTrigger)
                {
                    toggle.Set(state);
                }

                // 触发激活事件
                onActivate?.Invoke();
            }
        }
        else if (state) // 目标状态 = 关闭，且当前是开启状态
        {
            state = false;

            // 级联触发其它 Toggle
            foreach (var toggle in multiTrigger)
            {
                toggle.Set(state);
            }

            // 触发关闭事件
            onDeactivate?.Invoke();
        }
    }
}