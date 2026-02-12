using UnityEngine;

public class BossStateMachine : MonoBehaviour
{
    private IBossState currentState;

    void Update()
    {
        // 現在のステ?トがあれば、その処理を毎フレ??実行
        currentState?.Execute();
    }

    // 新しいステ?トに切り替えるメ?ッド
    public void ChangeState(IBossState newState)
    {
        // ▼▼▼ ログ出力処理を追加 ▼▼▼
        // 以前のステ?ト名を取得 (初回はnullなので "NULL" とする)
        string prevStateName = (currentState != null) ? currentState.GetType().Name : "NULL";
        // 新しいステ?ト名を取得
        string nextStateName = newState.GetType().Name;

        // 色付きのログで見やすく出力する
        Debug.Log($"<color=yellow>[BossState] State changed from [ {prevStateName} ] to [ {nextStateName} ]</color>");
        // ▲▲▲ ▲▲▲

        // 現在のステ?トがあれば、終了処理を呼び出す
        currentState?.Exit();

        // 新しいステ?トに更新し、開始処理を呼び出す
        currentState = newState;
        currentState.Enter();
    }
}