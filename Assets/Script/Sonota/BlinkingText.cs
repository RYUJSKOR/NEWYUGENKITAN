using UnityEngine;
using TMPro; // TextMeshProを使うために必要

public class BlinkingText : MonoBehaviour
{
    [SerializeField]
    private float blinkSpeed = 1.5f; // 点滅の速さ（1秒あたりの点滅サイクル数）

    [SerializeField]
    private float minAlpha = 0.2f;   // 最小の透明度 (0.0が完全透明)

    [SerializeField]
    private float maxAlpha = 1.0f;   // 最大の透明度 (1.0が完全不透明)

    private TextMeshProUGUI textMeshPro; // TextMeshProUGUIコンポーネント

    void Awake()
    {
        // アタッチされているTextMeshProUGUIコンポーネントを取得
        textMeshPro = GetComponent<TextMeshProUGUI>();
        if (textMeshPro == null)
        {
            Debug.LogError("BlinkingTextスクリプトはTextMeshProUGUIコンポーネントが必要です。", this);
            enabled = false; // コンポーネントが見つからない場合、このスクリプトを無効にする
        }
    }

    void Update()
    {
        // 時間に応じて透明度を計算
        // Mathf.PingPongは0からlengthまで往復する値なので、透明度の範囲に合わせて調整
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(Time.time * blinkSpeed, 1f));

        // テキストの色（Color）を取得し、透明度（alpha）のみ変更して設定し直す
        Color currentColor = textMeshPro.color;
        currentColor.a = alpha; // aはアルファ値（透明度）
        textMeshPro.color = currentColor;
    }
}