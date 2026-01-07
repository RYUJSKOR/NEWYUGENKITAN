// BossPhaseTransitionState.cs

using UnityEngine;
using System.Collections;

public class BossPhaseTransitionState : IBossState
{
    private BossController boss;
    private BossStateMachine stateMachine;
    private float transitionDuration = 4.0f; // 演出全体の時間

    public BossPhaseTransitionState(BossStateMachine sm, BossController boss)
    {
        this.stateMachine = sm;
        this.boss = boss;
    }

    public void Enter()
    {
        Debug.Log("フェーズ移行ステートに突入！演出を開始します。");

        // ボスに演出の開始を伝え、無敵化と腕の復活を依頼 
        boss.BeginPhaseTransition(transitionDuration);

        // 演出用のコルーチンを開始
        boss.RunAttackCoroutine(TransitionRoutine());
    }

    public void Execute() { }
    public void Exit()
    {
        // 念のため、演出後にモデルの状態を元に戻す
        if (boss.transitionModelObject != null)
        {
            boss.transitionModelObject.SetActive(false);
        }
        if (boss.bodyObject != null)
        {
            // 次のフェーズでbodyが必要な場合に備える
            // boss.bodyObject.SetActive(true); // ←必要に応じてコメント解除
        }
    }

    private IEnumerator TransitionRoutine()
    {
        // 赤い発光をいったんリセット
        boss.ResetAllFlash();

        // 1. モデルを切り替えてアニメーションを再生
        Debug.Log("演出：モデルを切り替えてアニメーションを再生！");
        boss.PlayTransitionAnimation("Escape");

        // アニメーションの長さに合わせて待機
        yield return new WaitForSeconds(6.0f);

        /*
        // 2. 退場ポイントまで移動する処理をまるごと削除、またはコメントアウト
        Debug.Log("演出：退場ポイントへ移動開始");
        Transform escapePoint = boss.escapePoint;
        if (escapePoint != null)
        {
            while (Vector3.Distance(boss.transform.position, escapePoint.position) > 0.1f)
            {
                boss.transform.position = Vector3.MoveTowards(boss.transform.position, escapePoint.position, 15f * Time.deltaTime);
                yield return null;
            }
        }
        */

        // 演出完了
        Debug.Log("演出：完了。");
        yield return new WaitForSeconds(1.0f);

        if (boss.transitionModelObject != null)
        {
            boss.transitionModelObject.SetActive(false);
        }
        // BodyMeshは非表示のまま次のフェーズへ

        // 次のステージへ行くためのオブジェクト生成
        Debug.Log("次のステージへのトリガーとはしごを生成します。");
        boss.SpawnPhaseExitObjects();
    }
}