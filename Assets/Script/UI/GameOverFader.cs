using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverFader : MonoBehaviour
{
    // ===== フィールド (修正) =====

    [Header("フェード対象のImage (文字)")]
    [SerializeField] private Image textImage; // 文字用のImage

    [Header("フェード対象のImage (背景)")]
    [SerializeField] private Image backgroundImage; // 背景用のImage

    [Header("フェード時間 (秒)")]
    [SerializeField] private float fadeDuration = 2.0f; // アルファ0.5→1になるまでの秒数

    [Header("遷移先シーン名")]
    [SerializeField] private string sceneToLoad = "GameOverScene"; // 遷移したいシーン名

    // ===== 外部から呼ぶ関数 =====

    // フェード開始関数
    public void Play()
    {
        StartCoroutine(FadeInThenLoad());
    }

    // ===== 内部処理 =====

    // フェードインしてからシーンを読み込むコルーチン
    private IEnumerator FadeInThenLoad()
    {
        // 開始アルファと終了アルファ
        float startAlpha = 0.5f;
        float endAlpha = 1.0f;

        // --- 1. 初期設定 (両方のImageに開始アルファを設定し、アクティブにする) ---

        // textImage の初期設定
        if (textImage != null)
        {
            Color cText = textImage.color;
            cText.a = startAlpha;
            textImage.color = cText;
            textImage.gameObject.SetActive(true);
        }

        // backgroundImage の初期設定
        if (backgroundImage != null)
        {
            Color cBg = backgroundImage.color;
            cBg.a = startAlpha;
            backgroundImage.color = cBg;
            backgroundImage.gameObject.SetActive(true);
        }

        // --- 2. フェード処理 (両方のImageのアルファを同時に変更) ---
        float t = 0f;
        while (t < 1f)
        {
            // 経過時間に応じてアルファ値を補間
            t += Time.deltaTime / fadeDuration;
            float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, t); // 現在のアルファ値

            // textImage のアルファ更新
            if (textImage != null)
            {
                Color cText = textImage.color;
                cText.a = currentAlpha;
                textImage.color = cText;
            }

            // backgroundImage のアルファ更新
            if (backgroundImage != null)
            {
                Color cBg = backgroundImage.color;
                cBg.a = currentAlpha;
                backgroundImage.color = cBg;
            }

            yield return null;
        }

        // --- 3. 最終処理 (アルファを1に固定) ---

        // textImage のアルファを固定
        if (textImage != null)
        {
            Color cText = textImage.color;
            cText.a = endAlpha;
            textImage.color = cText;
        }

        // backgroundImage のアルファを固定
        if (backgroundImage != null)
        {
            Color cBg = backgroundImage.color;
            cBg.a = endAlpha;
            backgroundImage.color = cBg;
        }

        // --- 4. シーン遷移 ---
        Debug.Log($"<color=cyan>--- 2. GameOverFader: シーン遷移を実行します。読み込み先: {sceneToLoad} ---</color>");
        SceneManager.LoadScene(sceneToLoad);
    }
}