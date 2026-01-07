using UnityEngine;

public class GameResetter : MonoBehaviour
{
    // タイトルに戻るボタンなどで、OnClickTransitionの「前」に呼び出す
    public void ResetAllGameData()
    {
        // GameDataのリセット
        if (GameData.Instance != null)
        {
            GameData.Instance.ResetAll();
            GameData.Instance.saveBossTime(0.0f);
        }

        // Boss関連のリセット
        if (BossGameManager.Instance != null)
        {
            BossGameManager.Instance.ResetSavedData();
        }

        // タイマー破棄
        var bossTimer = FindFirstObjectByType<BossTimerManager>();
        if (bossTimer != null)
        {
            bossTimer.StopTimer();
            Destroy(bossTimer.gameObject);
        }

        // 時間停止の解除
        Time.timeScale = 1.0f;

        Debug.Log("ゲームデータをリセットしました。");
    }
}