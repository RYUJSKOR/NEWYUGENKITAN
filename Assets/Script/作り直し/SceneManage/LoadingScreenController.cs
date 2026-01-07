using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Slider progressBar;
    [SerializeField] private bool performGC = true;
    [SerializeField] private float minLoadingTime = 1.0f;

    private void Start()
    {
        if (SceneNavigator.Instance == null)
        {
            SceneManager.LoadScene("Title");
            return;
        }

        StartCoroutine(LoadSequence(SceneNavigator.Instance.NextSceneName));
    }

    private IEnumerator LoadSequence(string sceneName)
    {
        // 非同期ロード開始（画面切り替えはまだ禁止！）
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        float timer = 0f;

        // ロード中演出
        while (op.progress < 0.9f || timer < minLoadingTime)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            float displayProgress = Mathf.Min(progress, timer / minLoadingTime);

            if (progressBar != null) progressBar.value = displayProgress;
            yield return null;
        }

        if (progressBar != null) progressBar.value = 1.0f;

        if (performGC)
        {
            System.GC.Collect();
            yield return null;
        }

        // ===== 修正ポイント =====

        // 1. ロード準備完了。ここで「暗転」を命令する
        // yield return を使うことで、完全に真っ黒になるまでここで処理が止まる
        yield return SceneNavigator.Instance.FadeOut();

        // 2. 完全に真っ黒になったので、シーンを切り替える
        // これで「見えてるのに切り替わる」現象は起きない
        op.allowSceneActivation = true;

        // ※明転（FadeIn）は新しいシーンに着いた瞬間にSceneNavigatorが勝手にやるので記述不要
    }
}