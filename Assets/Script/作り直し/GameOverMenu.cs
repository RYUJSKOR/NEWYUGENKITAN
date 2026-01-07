using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameOverMenu : MonoBehaviour
{
    private ButtonManager buttonManager;

    [Header("アニメーションさせる対象")]
    [SerializeField] private RectTransform defeatTextTransform;
    [SerializeField] private CanvasGroup retryButtonCanvasGroup;
    [SerializeField] private CanvasGroup titleButtonCanvasGroup;

    [Header("テキストのアニメーション設定")]
    [SerializeField] private float textEndPositionY = 250f;
    [SerializeField] private float textEndScale = 0.5f;
    [SerializeField] private float textAnimationDuration = 1.0f;

    [Header("ボタンのアニメーション設定")]
    [Tooltip("各ボタンがフェードインし終わるまでの時間 (秒)")]
    [SerializeField] private float buttonFadeInDuration = 0.5f;

    // ===== 変更点 (1/2) ： 新しい変数を追加 =====
    [Tooltip("リトライボタンが表示された後、タイトルボタンが表示されるまでの待ち時間 (秒)")]
    [SerializeField] private float delayBetweenButtons = 0.2f;
    // ==========================================


    void Start()
    {
        // ButtonManager を探す
        buttonManager = FindAnyObjectByType<ButtonManager>();
        if (buttonManager == null)
        {
            Debug.LogError("ButtonManager がシーンに見つかりません！");
        }

        // アニメーションのコルーチンを開始する
        StartCoroutine(PlayIntroAnimation());
    }

    // ===== 変更点 (2/2) ： この関数の中身を書き換え =====
    private IEnumerator PlayIntroAnimation()
    {
        // --- 1. 初期化 (両方のボタンを透明に・操作不能に) ---
        if (retryButtonCanvasGroup != null)
        {
            retryButtonCanvasGroup.alpha = 0f;
            retryButtonCanvasGroup.interactable = false;
        }
        if (titleButtonCanvasGroup != null)
        {
            titleButtonCanvasGroup.alpha = 0f;
            titleButtonCanvasGroup.interactable = false;
        }

        // --- 2. 敗北テキストのアニメーション (ここは変更なし) ---
        if (defeatTextTransform != null)
        {
            Vector2 startPos = defeatTextTransform.anchoredPosition;
            Vector3 startScale = defeatTextTransform.localScale;
            Vector2 endPos = new Vector2(startPos.x, textEndPositionY);
            Vector3 endScaleVec = new Vector3(textEndScale, textEndScale, textEndScale);

            float t = 0f;
            while (t < textAnimationDuration)
            {
                t += Time.deltaTime;
                float rate = Mathf.Clamp01(t / textAnimationDuration);
                defeatTextTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, rate);
                defeatTextTransform.localScale = Vector3.Lerp(startScale, endScaleVec, rate);
                yield return null;
            }
            defeatTextTransform.anchoredPosition = endPos;
            defeatTextTransform.localScale = endScaleVec;
        }

        // --- 3. 【リトライボタン】のフェードイン ---
        float t_retry = 0f;
        while (t_retry < buttonFadeInDuration)
        {
            t_retry += Time.deltaTime;
            float rate = Mathf.Clamp01(t_retry / buttonFadeInDuration);
            if (retryButtonCanvasGroup != null) retryButtonCanvasGroup.alpha = rate;
            yield return null;
        }
        // リトライボタンの最終処理 (操作可能にする)
        if (retryButtonCanvasGroup != null)
        {
            retryButtonCanvasGroup.alpha = 1f;
            retryButtonCanvasGroup.interactable = true;
        }

        // --- 4. ボタン間の【待ち時間】 ---
        if (delayBetweenButtons > 0f)
        {
            yield return new WaitForSeconds(delayBetweenButtons);
        }

        // --- 5. 【タイトルボタン】のフェードイン ---
        float t_title = 0f;
        while (t_title < buttonFadeInDuration)
        {
            t_title += Time.deltaTime;
            float rate = Mathf.Clamp01(t_title / buttonFadeInDuration);
            if (titleButtonCanvasGroup != null) titleButtonCanvasGroup.alpha = rate;
            yield return null;
        }
        // タイトルボタンの最終処理 (操作可能にする)
        if (titleButtonCanvasGroup != null)
        {
            titleButtonCanvasGroup.alpha = 1f;
            titleButtonCanvasGroup.interactable = true;
        }
    }
    // ==========================================


    // --- ボタンから呼び出すための関数 (変更なし) ---

    public void RetryGame()
    {
        if (buttonManager != null)
        {
            buttonManager.BackToStage();
        }
    }

    public void GoToTitle()
    {
        if (buttonManager != null)
        {
            buttonManager.BackToTitle();
        }
    }
}