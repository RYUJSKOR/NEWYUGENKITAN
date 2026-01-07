using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(BlueBoundEnemyMovement enemy) : base(enemy) { }

    public override void EnterState()
    {
        base.EnterState();
        // 着地したので、Unityの通常の重力に戻す
        enemy.Rb.useGravity = true;
    }

    public override void UpdateState()
    {
        // タイマーの進みに速度倍率を適用
        stateTimer += Time.deltaTime * enemy.CurrentSpeedModifier;
        if (stateTimer > enemy.idleDuration && enemy.IsGrounded())
        {
            enemy.ChangeState(enemy.preparingState);
        }
    }
}