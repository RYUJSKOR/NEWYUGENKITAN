using UnityEngine;

public class BossStunState : IBossState
{
    private BossStateMachine stateMachine;
    private BossController boss;
    private float stunTimer;

    private bool hasStateExited;

    public BossStunState(BossStateMachine sm, BossController boss, float duration)
    {
        this.stateMachine = sm;
        this.boss = boss;
        this.stunTimer = duration;
    }

    public void Enter()
    {
        Debug.Log("ボス: スタン状態に移行");
        // ステートに入る際に必ずフラグをリセット
        hasStateExited = false;
        // stunTimerもここで初期化するのが安全です
        stunTimer = boss.stunDuration; // durationを再設定
    }

    public void Execute()
    {
        // すでに終了処理が呼ばれていたら、タイマー処理を一切行わない
        if (hasStateExited) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0)
        {
            // フラグを立てる処理はここから削除します。
            // 命令を出す前に、再度終了していないかチェック（念のため）
            if (hasStateExited) return;

            // BossControllerの復活処理を呼び出す
            boss.EndStunAndRegenerateArms();
        }
    }

    public void Exit()
    {
        Debug.Log("ボス: スタン状態を終了");

        // ▼▼▼ 修正 ▼▼▼
        // このステートから退出する際にフラグを立てる
        // これにより、Executeメソッドが暴発するのを防ぐ
        hasStateExited = true;
    }
}