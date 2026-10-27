using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 生命值系统组件
/// 用于管理游戏对象（如玩家或敌人）的生命值，包括：
/// - 初始值、最大值
/// - 受伤和恢复冷却机制
/// - 生命值改变或受伤时触发的事件回调
/// </summary>
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Health")]
public class Health : MonoBehaviour
{
    /// <summary>
    /// 初始生命值（游戏开始时的生命值）
    /// </summary>
    public int initial = 3;

    /// <summary>
    /// 最大生命值
    /// </summary>
    public int max = 3;

    /// <summary>
    /// 受伤后的冷却时间（在此时间内不会再次受到伤害）
    /// </summary>
    public float coolDown = 1f;

    /// <summary>
    /// 当生命值变化时调用的事件（比如 UI 更新）
    /// </summary>
    public UnityEvent onChange;

    /// <summary>
    /// 当对象受到伤害时调用的事件（比如播放受伤动画、音效）
    /// </summary>
    public UnityEvent onDamage;

    /// <summary>
    /// 当前的生命值
    /// </summary>
    protected int m_currentHealth;

    /// <summary>
    /// 上一次受到伤害的时间（用于计算冷却）
    /// </summary>
    protected float m_lastDamageTime;

    /// <summary>
    /// 当前生命值属性
    /// - 只允许在 [0, max] 范围内
    /// - 发生变化时调用 onChange 事件
    /// </summary>
    public int current
    {
        get { return m_currentHealth; }

        protected set
        {
            var last = m_currentHealth;

            if (value != last)
            {
                m_currentHealth = Mathf.Clamp(value, 0, max);
                onChange?.Invoke();
            }
        }
    }

    /// <summary>
    /// 是否死亡（生命值为 0）
    /// </summary>
    public virtual bool isEmpty => current == 0;

    /// <summary>
    /// 是否处于受伤冷却中（还不能再次受伤）
    /// </summary>
    public virtual bool recovering => Time.time < m_lastDamageTime + coolDown;

    /// <summary>
    /// 直接设置生命值
    /// </summary>
    /// <param name="amount">目标生命值</param>
    public virtual void Set(int amount) => current = amount;

    /// <summary>
    /// 增加生命值（通常用于回血或吃到回复道具）
    /// </summary>
    /// <param name="amount">增加的生命值</param>
    public virtual void Increase(int amount) => current += amount;

    /// <summary>
    /// 扣减生命值（受到伤害时调用）
    /// </summary>
    /// <param name="amount">伤害值</param>
    public virtual void Damage(int amount)
    {
        if (!recovering)
        {
            current -= Mathf.Abs(amount);
            m_lastDamageTime = Time.time;
            onDamage?.Invoke();
        }
    }

    /// <summary>
    /// 重置生命值为初始值（常用于角色复活或关卡重置）
    /// </summary>
    public virtual void Reset() => current = initial;

    /// <summary>
    /// 初始化：在 Awake 时将生命值设置为初始值
    /// </summary>
    protected virtual void Awake() => current = initial;
}
