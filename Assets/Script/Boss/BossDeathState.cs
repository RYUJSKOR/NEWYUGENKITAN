using UnityEngine;
using System.Collections;

public class BossDeathState : IBossState
{
    private BossController boss;

    public BossDeathState(BossController boss)
    {
        this.boss = boss;
    }

    public void Enter()
    {
        Debug.Log("ボス: 死亡状態に移行。");
        // 実際の演出はBossControllerのDeathAnimationSequenceコルーチンが担当する
    }

    public void Execute()
    {
        // 死亡演出中は特に何もしない
    }

    public void Exit()
    {
        // このステートから他のステートに遷移することはない
    }
}