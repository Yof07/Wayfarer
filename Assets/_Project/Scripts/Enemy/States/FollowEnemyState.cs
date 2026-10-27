using UnityEngine;

// 添加到 Unity 菜单中的组件路径
[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Follow Enemy State")]
public class FollowEnemyState : EnemyState
{
    // 当进入这个状态时调用（这里没有特殊处理）
    protected override void OnEnter(Enemy enemy) { }

    // 当退出这个状态时调用（这里没有特殊处理）
    protected override void OnExit(Enemy enemy) { }

    // 每帧或固定时间间隔内在此状态下执行的逻辑
    protected override void OnStep(Enemy enemy)
    {
        // 应用重力
        enemy.Gravity();
        // 保持敌人紧贴地面（防止悬空或漂浮）
        enemy.SnapToGround();
        // 计算敌人到玩家头部的方向向量
        var head = enemy.player.position - enemy.position;
        // 只保留水平方向（忽略 Y 轴），并单位化方向向量
        var direction = new Vector3(head.x, 0, head.z).normalized;
        // 让敌人朝玩家方向加速
        // followAcceleration：跟随加速度
        // followTopSpeed：跟随时的最大速度
        enemy.Accelerate(direction, enemy.stats.current.followAcceleration, enemy.stats.current.followTopSpeed);
        // 平滑地旋转敌人朝向玩家的方向
        enemy.FaceDirectionSmooth(direction);
    }

    // 当敌人与其他碰撞体接触时调用（这里没有特殊处理）
    public override void OnContact(Enemy enemy, Collider other) { }
}
