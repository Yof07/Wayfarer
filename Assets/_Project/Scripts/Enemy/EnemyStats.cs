using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "PLAYER TWO/Platformer Project/Enemy/New Enemy Stats")]
public class EnemyStats : EntityStats<EnemyStats>
{
    [Header("General Stats")]
    // 重力值，用于模拟敌人下落速度
    public float gravity = 35f;

    // 吸附力，敌人贴地面时向下的作用力
    public float snapForce = 15f;

    // 旋转速度，控制敌人转向的快慢（单位：度/秒）
    public float rotationSpeed = 970f;

    // 减速度，敌人停止移动时的减速速率
    public float deceleration = 28f;

    // 摩擦力，用于模拟地面摩擦带来的减速
    public float friction = 16f;

    // 转向阻力，控制敌人转弯时速度变化的阻力
    public float turningDrag = 28f;


    [Header("Contact Attack Stats")]
    // 是否允许敌人通过接触攻击玩家
    public bool canAttackOnContact = true;

    // 接触攻击时是否施加击退效果
    public bool contactPushback = true;

    // 接触检测的偏移量，检测攻击范围的大小
    public float contactOffset = 0.15f;

    // 接触攻击造成的伤害值
    public int contactDamage = 1;

    // 击退时施加的推力大小
    public float contactPushBackForce = 18f;

    // 接触攻击中容忍玩家踩踏的距离（垂直容差）
    public float contactSteppingTolerance = 0.1f;


    [Header("View Stats")]
    // 发现玩家的视野范围（范围内能初步发现玩家）
    public float spotRange = 5f;

    // 敌人追踪玩家的最大视野范围（超出后失去目标）
    public float viewRange = 8f;


    [Header("Follow Stats")]
    // 追踪时的加速度
    public float followAcceleration = 10f;

    // 追踪时的最高移动速度
    public float followTopSpeed = 4f;


    [Header("Waypoint Stats")]
    // 是否朝向当前路径点旋转
    public bool faceWaypoint = true;

    // 距离路径点多近时算到达，切换下一个点
    public float waypointMinDistance = 0.5f;

    // 巡逻时的加速度
    public float waypointAcceleration = 10f;

    // 巡逻时的最高移动速度
    public float waypointTopSpeed = 2f;
}
