using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingTextAnimator : MonoBehaviour
{
    [Header("参照コンポーネント")]
    [SerializeField]
    private TextMeshProUGUI loadingText;

    [Header("アニメーション設定")]
    [SerializeField]
    private string baseText = "読み込み中";
    [SerializeField]
    [Range(0.1f, 2.0f)]
    private float blinkSpeed = 0.7f;
    [SerializeField]
    [Range(0.1f, 1.0f)]
    private float dotInterval = 0.5f;

    private Color baseColor;
    private Coroutine dotsCoroutine;
    private Coroutine blinkingCoroutine;

    // 変更点: Start -> OnEnable
    // オブジェクトがアクティブになるたびに実行
    void OnEnable()
    {
        if (loadingText == null)
        {
            loadingText = GetComponent<TextMeshProUGUI>();
        }
        if (loadingText == null)
        {
            Debug.LogError("TextMeshProUGUI が見つかりません。");
            return;
        }

        baseColor = loadingText.color;

        // 以前のコルーチンが残らないように、一度停止してから開始
        StopAllCoroutines();
        dotsCoroutine = StartCoroutine(AnimateDotsCoroutine());
        blinkingCoroutine = StartCoroutine(AnimateBlinkingCoroutine());
    }

    // 変更点: OnDisable を追加
    // オブジェクトが非アクティブになる時にコルーチンを停止
    void OnDisable()
    {
        // 念のためコルーチンを停止し、テキストを元に戻す
        if (dotsCoroutine != null) StopCoroutine(dotsCoroutine);
        if (blinkingCoroutine != null) StopCoroutine(blinkingCoroutine);

        // (オプション) 非表示になる時にテキストとアルファを元に戻す
        if (loadingText != null)
        {
            loadingText.text = baseText;
            loadingText.color = baseColor;
        }
    }

    private IEnumerator AnimateDotsCoroutine()
    {
        int dotCount = 0;
        while (true)
        {
            dotCount = (dotCount + 1) % 4;
            loadingText.text = baseText.PadRight(baseText.Length + dotCount, '.');

            // 変更点: WaitForSeconds -> WaitForSecondsRealtime
            // Time.timeScaleが0でも待機できるようにする
            yield return new WaitForSecondsRealtime(dotInterval);
        }
    }

    private IEnumerator AnimateBlinkingCoroutine()
    {
        while (true)
        {
            // 変更点: Time.time -> Time.unscaledTime
            // Time.timeScaleが0でも動作する時間を使う
            float alpha = Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1.0f);
            loadingText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
    }
}