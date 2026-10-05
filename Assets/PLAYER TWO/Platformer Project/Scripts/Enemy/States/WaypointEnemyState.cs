using UnityEngine;

// 在 Unity Inspector 菜单中添加一个可选组件
// 路径：PLAYER TWO/Platformer Project/Enemy/States/Waypoint Enemy State
// 用于实现“敌人按路线点(waypoint)巡逻”的状态
[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Waypoint Enemy State")]
public class WaypointEnemyState : EnemyState
{
    /// <summary>
    /// 当敌人进入“Waypoint状态”时调用
    /// （此处未实现任何初始化逻辑）
    /// </summary>
    protected override void OnEnter(Enemy enemy) { }

    /// <summary>
    /// 当敌人退出“Waypoint状态”时调用
    /// （此处未实现任何清理逻辑）
    /// </summary>
    protected override void OnExit(Enemy enemy) { }

    /// <summary>
    /// 每帧执行一次，用于更新敌人在 Waypoint 巡逻状态下的行为
    /// </summary>
    protected override void OnStep(Enemy enemy)
    {
        // 施加重力
        enemy.Gravity();
        // 将敌人贴合到地面
        enemy.SnapToGround();

        // 获取当前巡逻点的目标位置（waypoints.current.position）
        var destination = enemy.waypoints.current.position;
        // 保持敌人当前的垂直高度（只移动 X/Z）
        destination = new Vector3(destination.x, enemy.position.y, destination.z);

        // 计算从敌人当前位置指向目标点的向量
        var head = destination - enemy.position;
        // 计算距离
        var distance = head.magnitude;
        // 归一化方向向量
        var direction = head / distance;

        // 如果距离小于等于最小到达距离，说明已经到达当前巡逻点
        if (distance <= enemy.stats.current.waypointMinDistance)
        {
            // 减速
            enemy.Decelerate();
            // 切换到下一个巡逻点
            enemy.waypoints.Next();
        }
        else
        {
            // 向目标方向加速，使用巡逻加速度和最大速度限制
            enemy.Accelerate(
                direction,
                enemy.stats.current.waypointAcceleration,
                enemy.stats.current.waypointTopSpeed
            );

            // 如果配置要求朝向巡逻点方向
            if (enemy.stats.current.faceWaypoint)
            {
                // 平滑地转向目标方向
                enemy.FaceDirectionSmooth(direction);
            }
        }
    }


    /// <summary>
    /// 当敌人与其他碰撞体接触时调用
    /// （此处未做额外处理）
    /// </summary>
    public override void OnContact(Enemy enemy, Collider other) { }
}
