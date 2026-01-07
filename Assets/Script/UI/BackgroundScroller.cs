using UnityEngine;
using UnityEngine.UI;

public class BackgroundScroller : MonoBehaviour
{
    // 左右に移動する速度を設定します。Inspectorから調整可能です。
    // 正の値で右へ移動、負の値で左へ移動します。
    [SerializeField]
    private float scrollSpeed = 0.5f;

    // 画像のRectTransformを保持します。
    private RectTransform rectTransform;

    void Start()
    {
        // アタッチされているGameObjectからRectTransformコンポーネントを取得します。
        rectTransform = GetComponent<RectTransform>();

        // RectTransformが取得できたかチェック
        if (rectTransform == null)
        {
            Debug.LogError("このGameObjectにはRectTransformコンポーネントがありません。UI要素にアタッチしてください。");
            enabled = false; // スクリプトを無効化
        }
    }

    void Update()
    {
        // 毎フレーム、RectTransformの位置を更新します。
        // Time.deltaTimeをかけることで、フレームレートに依存しない滑らかな移動を実現します。

        // 現在のX位置に速度×時間を加算
        float newX = rectTransform.anchoredPosition.x + scrollSpeed * Time.deltaTime;

        // ループ処理（画面サイズに応じて調整が必要です）
        // 例: 画像が画面幅の2倍の幅を持ち、左端(0)から右に移動している場合
        // ここでは簡単なループ処理の例として、特定の範囲を超えたら反対側に戻す処理を実装します。

        // **【重要】ループの動作を調整するパラメータ**
        // 画像の端から端まで移動する距離（画像の幅など）を `loopWidth` に設定してください。
        // 例: 画像が親要素のちょうど2倍の幅で設定されている場合、半分の幅を `loopWidth` に設定。
        // ここでは仮の値として500を設定します。ご自身の画像のサイズに合わせて調整してください。
        float loopWidth = 500f;

        if (scrollSpeed > 0) // 右へ移動する場合
        {
            // 位置が特定の距離（loopWidth）を超えたら、反対側（loopWidthを引いた位置）に戻す
            if (newX > loopWidth)
            {
                newX -= loopWidth;
            }
        }
        else if (scrollSpeed < 0) // 左へ移動する場合
        {
            // 位置が特定の距離（-loopWidth）を超えたら、反対側（loopWidthを加えた位置）に戻す
            if (newX < -loopWidth)
            {
                newX += loopWidth;
            }
        }

        // 新しいX座標を適用
        rectTransform.anchoredPosition = new Vector2(newX, rectTransform.anchoredPosition.y);
    }
}