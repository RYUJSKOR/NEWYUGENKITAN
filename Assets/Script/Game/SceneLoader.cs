using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider loadingSlider;

    private IEnumerator Start()
    {
        // ★★★ 追加：ここが重要！ ★★★
        // ローディング画面に来た時点では画面が真っ暗なので、
        // まずフェードインして「ロード画面（バーなど）」を見えるようにする
        yield return FadeManager.Instance.FadeIn();

        // -------------------------------------------------------

        // 次のシーンを読み込み開始
        string sceneToLoad = SceneFlowController.Instance.NextSceneName;

        // 安全対策：もしシーン名が空ならエラーを出して止める
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("SceneLoader: 次のシーン名が設定されていません！");
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneToLoad);
        op.allowSceneActivation = false; // 読み込み完了しても勝手に移動しない

        // ロード進捗バーの更新
        while (op.progress < 0.9f)
        {
            if (loadingSlider != null)
                loadingSlider.value = Mathf.Clamp01(op.progress / 0.9f);

            yield return null;
        }

        if (loadingSlider != null) loadingSlider.value = 1f;

        // 読み込み終わったので、フェードアウト（幕を下ろす）
        yield return FadeManager.Instance.FadeOut();

        // 暗くなったので、こっそりシーンを切り替える
        op.allowSceneActivation = true;
    }
}