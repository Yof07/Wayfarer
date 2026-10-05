using UnityEngine;

//Pole（杆子） 脚本主要用于 平台游戏中的爬杆功能
// 确保物体上必须有一个 CapsuleCollider 组件
[RequireComponent(typeof(CapsuleCollider))]
// 在 Unity 编辑器菜单中添加组件的路径
[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Pole")]
public class Pole : MonoBehaviour
{
    /// <summary>
    /// 返回该 Pole 的 Collider（强制为 CapsuleCollider 类型）
    /// 使用 new 关键字隐藏基类的 collider 属性
    /// </summary>
    public new CapsuleCollider collider { get; protected set; }

    /// <summary>
    /// 返回 Pole 的半径
    /// 其实就是 CapsuleCollider 的半径
    /// </summary>
    public float radius => collider.radius;

    /// <summary>
    /// 返回 Pole 的中心点
    /// 这里使用 Transform 的世界坐标
    /// </summary>
    public Vector3 center => transform.position;

    /// <summary>
    /// 初始化时设置 Pole 的 Tag 和 Collider 引用
    /// </summary>
    protected virtual void Awake()
    {
        // 设置物体的 Tag 为 "Pole"
        tag = GameTags.Pole;
        // 获取物体上的 CapsuleCollider 组件
        collider = GetComponent<CapsuleCollider>();
    }

    /// <summary>
    /// 获取某个 Transform 面向 Pole 的方向
    /// </summary>
    /// <param name="other">目标 Transform</param>
    /// <returns>从目标指向 Pole 的方向向量</returns>
    public Vector3 GetDirectionToPole(Transform other) => GetDirectionToPole(other, out _);

    /// <summary>
    /// 获取某个 Transform 面向 Pole 的方向，并返回两者的距离
    /// </summary>
    /// <param name="other">目标 Transform</param>
    /// <param name="distance">输出：与 Pole 的水平距离</param>
    /// <returns>从目标指向 Pole 的单位向量</returns>
    public Vector3 GetDirectionToPole(Transform other, out float distance)
    {
        // Pole 的位置在水平面上投影，保证方向只考虑 XZ 平面
        var target = new Vector3(center.x, other.position.y, center.z) - other.position;
        // 计算距离（向量长度）
        distance = target.magnitude;
        // 返回标准化方向向量
        return target / distance;
    }

    /// <summary>
    /// 将一个点的高度限制在 Pole 的范围内
    /// </summary>
    /// <param name="point">需要被约束的点</param>
    /// <param name="offset">上下偏移量，用于调整 Pole 的最小/最大高度范围</param>
    /// <returns>被约束到 Pole 高度范围内的新点</returns>
    public Vector3 ClampPointToPoleHeight(Vector3 point, float offset)
    {
        // 计算 Pole 的最低点（加上偏移）
        var minHeight = collider.bounds.min.y + offset;
        // 计算 Pole 的最高点（减去偏移）
        var maxHeight = collider.bounds.max.y - offset;
        // 将点的 y 值限制在 minHeight 和 maxHeight 之间
        var clampedHeight = Mathf.Clamp(point.y, minHeight, maxHeight);
        // 返回新的点（x、z 不变，只修改 y）
        return new Vector3(point.x, clampedHeight, point.z);
    }
}
