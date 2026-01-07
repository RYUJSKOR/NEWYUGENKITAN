using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RetryButton : MonoBehaviour
{
    private GameResetter gameResetter;
    private bool isRetrying = false;

    private void Start()
    {
        gameResetter = GetComponent<GameResetter>();
        if (gameResetter == null) gameResetter = FindFirstObjectByType<GameResetter>();
    }

    public void OnRetryClick()
    {
        if (isRetrying) return;
        StartCoroutine(RetryProcess());
    }

    private IEnumerator RetryProcess()
    {
        isRetrying = true;

        // 1. フェードアウト
        if (FadeManager.Instance != null)
        {
            yield return StartCoroutine(FadeManager.Instance.FadeOut());
        }

        // 2. データをリセット
        if (gameResetter != null)
        {
            gameResetter.ResetAllGameData();
        }
        else
        {
            if (GameData.Instance != null) GameData.Instance.ResetAll();
        }

        Time.timeScale = 1.0f;

        // ★修正：読み込むシーン名の決定ロジック
        string targetSceneName;

        // SceneFlowControllerに記録されたステージ名があればそれを使う
        if (SceneFlowController.Instance != null && !string.IsNullOrEmpty(SceneFlowController.Instance.LastPlayedStageName))
        {
            targetSceneName = SceneFlowController.Instance.LastPlayedStageName;
        }
        else
        {
            // 記録がない場合（エディタでGameOverシーンから直接再生した時など）は現在のシーン
            targetSceneName = SceneManager.GetActiveScene().name;
        }

        // 3. 目的のシーンを読み込む
        // もしGameOverシーンから「Loading画面」を経由して戻りたい場合は
        // SceneFlowController.Instance.RequestScene(targetSceneName); を使うことも検討してください
        SceneManager.LoadScene(targetSceneName);
    }
}