using UnityEngine;

public class BossStateMachine : MonoBehaviour
{
    private IBossState currentState;

    void Update()
    {
        // 現在のステートがあれば、その処理を毎フレーム実行
        currentState?.Execute();
    }

    // 新しいステートに切り替えるメソッド
    public void ChangeState(IBossState newState)
    {
        // ▼▼▼ ログ出力処理を追加 ▼▼▼
        // 以前のステート名を取得 (初回はnullなので "NULL" とする)
        string prevStateName = (currentState != null) ? currentState.GetType().Name : "NULL";
        // 新しいステート名を取得
        string nextStateName = newState.GetType().Name;

        // 色付きのログで見やすく出力する
        Debug.Log($"<color=yellow>[BossState] State changed from [ {prevStateName} ] to [ {nextStateName} ]</color>");
        // ▲▲▲ ▲▲▲

        // 現在のステートがあれば、終了処理を呼び出す
        currentState?.Exit();

        // 新しいステートに更新し、開始処理を呼び出す
        currentState = newState;
        currentState.Enter();
    }
}