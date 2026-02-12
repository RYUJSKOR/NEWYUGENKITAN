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
        Debug.Log("?ス: 攻撃状態に移行(トリガ?)");
        boss.ExecuteNextAttack(); // 変更?
        stateMachine.ChangeState(new BossIdleState(stateMachine, boss));
    }

    public void Execute() { }

    public void Exit() { }
}