// BossIdleState.cs（完全版）

using UnityEngine;

public class BossIdleState : IBossState
{
    private BossStateMachine stateMachine;
    private BossController boss;
    private float idleTimer;
    private Rigidbody leftArmRb;
    private Rigidbody rightArmRb;

    public BossIdleState(BossStateMachine sm, BossController boss)
    {
        this.stateMachine = sm;
        this.boss = boss;
        if (boss.leftArmObject != null)
            this.leftArmRb = boss.leftArmObject.GetComponent<Rigidbody>();
        if (boss.rightArmObject != null)
            this.rightArmRb = boss.rightArmObject.GetComponent<Rigidbody>();
    }

    public void Enter()
    {
        Debug.Log("ボス: アイドル状態に移行");

        idleTimer = 0f;
    }

    public void Execute()
    {
        // 常に腕の浮遊処理を呼び出す
        FloatArms();

        // ボスが全体として攻撃中でない時だけ、次の攻撃へのタイマーを進める
        if (!boss.IsAttacking)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= boss.GetCurrentPhaseAttackInterval())
            {
                stateMachine.ChangeState(new BossAttackState(stateMachine, boss));
            }
        }
    }

    public void Exit() { }

    private void FloatArms()
    {
        float floatX = Mathf.Cos(Time.time * boss.idleFloatSpeed) * boss.idleFloatHorizontalRadius;
        float floatY = Mathf.Sin(Time.time * boss.idleFloatSpeed) * boss.idleFloatVerticalRadius;

        // 左腕の浮遊処理（左腕が攻撃中でない場合のみ）
        if (!boss.IsLeftArmAttacking && leftArmRb != null && leftArmRb.gameObject.activeSelf)
        {
            Vector3 leftOffset = new Vector3(floatX, floatY, 0);
            leftArmRb.MovePosition(boss.leftArmRestPosition.position + leftOffset);
        }

        // 右腕の浮遊処理（右腕が攻撃中でない場合のみ）
        if (!boss.IsRightArmAttacking && rightArmRb != null && rightArmRb.gameObject.activeSelf)
        {
            Vector3 rightOffset = new Vector3(-floatX, floatY, 0); // Xの動きを反転
            rightArmRb.MovePosition(boss.rightArmRestPosition.position + rightOffset);
        }
    }
}