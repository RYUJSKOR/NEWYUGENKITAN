using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 【修正済み】
/// シェーダーは使用せず、CanvasGroupのAlpha操作のみを行う通常遷移用クラス。
/// </summary>
public class FadeController : MonoBehaviour
{
    // シングルトン化（必要に応じて）
    public static FadeController Instance { get; private set; }

    [Header("UI参照")]
    // ★重要：インスペクターで、黒画像の親オブジェクトにあるCanvasGroupをアタッチしてください
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("設定")]
    [SerializeField] private float defaultDuration = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 画面を暗くする（シーン遷移開始時）
    /// </summary>
    public IEnumerator FadeOut(float duration = -1f)
    {
        float time = (duration < 0) ? defaultDuration : duration;

        // 操作ブロック用にレイキャストを有効化
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = true;

        yield return StartCoroutine(FadeProcess(0f, 1f, time));
    }

    /// <summary>
    /// 画面を明るくする（シーン開始時）
    /// </summary>
    public IEnumerator FadeIn(float duration = -1f)
    {
        float time = (duration < 0) ? defaultDuration : duration;

        yield return StartCoroutine(FadeProcess(1f, 0f, time));

        // フェード終了後は操作可能にする
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = false;
    }

    private IEnumerator FadeProcess(float startAlpha, float endAlpha, float duration)
    {
        if (fadeCanvasGroup == null) yield break;

        float elapsed = 0f;
        fadeCanvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // タイムスケール無視
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }
}