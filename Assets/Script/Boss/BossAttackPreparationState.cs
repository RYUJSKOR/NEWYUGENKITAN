using UnityEngine;

public class BossAttackPreparationState : IBossState
{
    private BossStateMachine stateMachine;
    private BossController boss;
    private float preparationTimer;

    public BossAttackPreparationState(BossStateMachine sm, BossController boss, float duration = 2.0f)
    {
        this.stateMachine = sm;
        this.boss = boss;
        this.preparationTimer = duration;
    }

    public void Enter()
    {
        Debug.Log("?ƒX: UŒ‚?”õó‘Ô‚ÉˆÚs");
    }

    public void Execute()
    {
        preparationTimer -= Time.deltaTime;
        if (preparationTimer <= 0)
        {
            stateMachine.ChangeState(new BossAttackState(stateMachine, boss));
        }
    }

    public void Exit() { }
}