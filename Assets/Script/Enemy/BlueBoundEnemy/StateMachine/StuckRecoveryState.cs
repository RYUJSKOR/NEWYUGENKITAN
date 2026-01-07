using UnityEngine;

public class StuckRecoveryState : BaseState
{
    public StuckRecoveryState(BlueBoundEnemyMovement enemy) : base(enemy) { }

    public override void EnterState()
    {
        base.EnterState();
        enemy.Rb.linearVelocity = Vector3.zero;

        Vector3 directionToPlayer = (enemy.TargetObject.transform.position - enemy.transform.position).normalized;
        directionToPlayer.y = 0;

        Vector3 backwardDirection = -directionToPlayer;
        Vector3 jumpForceVector = (backwardDirection * enemy.moveForce * 1.2f) + (Vector3.up * enemy.jumpForce);

        enemy.Rb.AddForce(jumpForceVector * enemy.CurrentSpeedModifier, ForceMode.VelocityChange);
        enemy.Rb.useGravity = false;
    }

    public override void UpdateState()
    {
        // 接地判定はUpdateのまま
        if (enemy.Rb.linearVelocity.y < 0 && enemy.IsGrounded())
        {
            enemy.ChangeState(enemy.idleState);
        }
    }

    public override void FixedUpdateState()
    {
        // カスタム重力はFixedUpdateで適用
        Vector3 scaledGravity = Physics.gravity * enemy.CurrentSpeedModifier;
        enemy.Rb.AddForce(scaledGravity, ForceMode.Acceleration);
    }
}