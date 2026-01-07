using UnityEngine;

public class BossAttackState : IBossState
{
    private BossStateMachine stateMachine;
    private BossController boss;

    public BossAttackState(BossStateMachine sm, BossController boss)
    {
        this.stateMachine = sm;
        this.boss = boss;
    }

    public void Enter()
    {
        Debug.Log("ボス: 攻撃状態に移行(トリガー)");
        boss.ExecuteNextAttack(); // 変更点
        stateMachine.ChangeState(new BossIdleState(stateMachine, boss));
    }

    public void Execute() { }

    public void Exit() { }
}