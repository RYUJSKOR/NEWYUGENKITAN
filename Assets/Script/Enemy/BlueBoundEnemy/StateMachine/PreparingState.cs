using UnityEngine;

public class PreparingState : BaseState
{
    public PreparingState(BlueBoundEnemyMovement enemy) : base(enemy) { }

    public override void EnterState()
    {
        base.EnterState();
        // しゃがむコルーチンを開始する
        enemy.StartCoroutine(enemy.CrouchCoroutine());
    }

    public override void UpdateState()
    {
        // 処理はコルーチンが担当するため、ここでは何もしない
    }
}