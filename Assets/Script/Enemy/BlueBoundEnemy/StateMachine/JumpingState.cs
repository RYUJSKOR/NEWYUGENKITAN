using UnityEngine;

public class JumpingState : BaseState
{
    public JumpingState(BlueBoundEnemyMovement enemy) : base(enemy) { }

    public override void UpdateState()
    {
        // 落下中で地面に接地したら待機状態へ（この判定はUpdateのままでOK）
        if (enemy.Rb.linearVelocity.y < 0 && enemy.IsGrounded())
        {
            enemy.ChangeState(enemy.idleState);
        }
    }

    public override void FixedUpdateState()
    {
        // スローモーション効果のため、調整したカスタム重力を適用し続ける
        Vector3 scaledGravity = Physics.gravity * enemy.CurrentSpeedModifier;
        enemy.Rb.AddForce(scaledGravity, ForceMode.Acceleration);
    }
}