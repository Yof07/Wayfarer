using UnityEngine;

[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Idle Enemy State")]
public class IdleEnemyState : EnemyState
{
    protected override void OnEnter(Enemy enemy) { }

    protected override void OnExit(Enemy enemy) { }

    protected override void OnStep(Enemy enemy)
    {
        // 应用重力，使敌人受重力影响下落
        enemy.Gravity();
        // 将敌人贴合到地面，避免悬空
        enemy.SnapToGround();
        // 在地面时施加摩擦力，逐渐让敌人停止移动
        enemy.Friction();
    }

    public override void OnContact(Enemy enemy, Collider other) { }
}
