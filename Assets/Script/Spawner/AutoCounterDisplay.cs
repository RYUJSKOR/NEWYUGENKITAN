using TMPro;
using UnityEngine;

/// <summary>
/// EnemyCounterの値を表示するTextMeshProオブジェクトを自動的に生成・管理します。
/// このスクリプトをアタッチしたオブジェクトの子として、テキストが生成されます。
/// </summary>
public class AutoCounterDisplay : MonoBehaviour
{
    // --- Inspectorで調整可能な設定 ---
    [Header("表示設定")]
    [Tooltip("テキストを表示する、このオブジェクトからの相対的な位置")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    [Tooltip("テキストオブジェクトの初期スケール")]
    [SerializeField] private float initialScale = 0.1f;

    [Tooltip("フォントサイズ")]
    [SerializeField] private float fontSize = 36;

    [Tooltip("テキストの色")]
    [SerializeField] private Color textColor = Color.cyan;

    [Tooltip("表示するテキストの書式。{0}が敵の数に置き換えられます。")]
    [SerializeField] private string displayTextFormat = "Enemies In View\n<size=150%>{0}</size>";

    // --- 内部参照 ---
    private TextMeshPro textMeshPro;
    private Camera mainCamera;

    // OnValidateはエディタ上で値が変更されたときに呼び出されます。
    // これにより、Inspectorでオフセットなどを変更した際に即座にプレビューできます。
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 既にTextMeshProがあるか探す
        textMeshPro = GetComponentInChildren<TextMeshPro>();

        if (textMeshPro == null)
        {
            // なければセットアップ処理を呼ぶ
            SetupTextMesh();
        }
        else
        {
            // あれば設定値を反映する
            ApplySettings();
        }
    }
#endif

    void Start()
    {
        // ゲーム実行時にTextMeshProがなければセットアップする
        if (textMeshPro == null)
        {
            textMeshPro = GetComponentInChildren<TextMeshPro>();
            if (textMeshPro == null)
            {
                SetupTextMesh();
            }
        }

        // メインカメラを探してキャッシュする
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera が見つかりません！", this);
            enabled = false; // エラー時はスクリプトを無効化
        }
    }

    void Update()
    {
        // 必要なコンポーネントが全て揃っているか確認
        if (textMeshPro != null && mainCamera != null && EnemyCounter.Instance != null)
        {
            // EnemyCounterから値を取得してテキストを更新
            int count = EnemyCounter.Instance.EnemyCountInView;
            textMeshPro.text = string.Format(displayTextFormat, count);

            // テキストが常にカメラの方を向くようにする
            textMeshPro.transform.rotation = mainCamera.transform.rotation;
        }
    }

    /// <summary>
    /// TextMeshProオブジェクトを生成し、初期設定を行います。
    /// </summary>
    private void SetupTextMesh()
    {
        // 子オブジェクトとしてTextMeshProを持つGameObjectを生成
        GameObject textObj = new GameObject("CounterText");
        textObj.transform.SetParent(this.transform);

        // TextMeshProコンポーネントを追加
        textMeshPro = textObj.AddComponent<TextMeshPro>();

        // 設定を適用
        ApplySettings();
    }

    /// <summary>
    /// Inspectorで設定された値をTextMeshProコンポーネントに適用します。
    /// </summary>
    private void ApplySettings()
    {
        if (textMeshPro == null) return;

        Transform textTransform = textMeshPro.transform;
        textTransform.localPosition = offset;
        textTransform.localRotation = Quaternion.identity;
        textTransform.localScale = Vector3.one * initialScale;

        textMeshPro.fontSize = fontSize;
        textMeshPro.color = textColor;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.text = "---"; // 初期テキスト
    }
}