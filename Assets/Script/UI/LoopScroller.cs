using UnityEngine;
using UnityEngine.UI;

public class LoopScroller : MonoBehaviour
{
    // 移動速度を設定します。Inspectorから調整可能です。（負の値で左へ移動）
    [SerializeField]
    private float scrollSpeed = -20f; // 例: -20f (左へ速めに移動)

    // 親オブジェクトのRectTransform
    private RectTransform containerRectTransform;

    // ループの基準となる幅（画像の幅と同じ値）
    private float imageWidth;

    void Start()
    {
        containerRectTransform = GetComponent<RectTransform>();

        // 【重要】最初の画像（子オブジェクトの0番目）の幅を取得し、ループ幅とします。
        // これが1枚分の画像の幅になります。
        if (transform.childCount > 0)
        {
            RectTransform childRect = transform.GetChild(0).GetComponent<RectTransform>();
            if (childRect != null)
            {
                // widthを基準とする
                imageWidth = childRect.sizeDelta.x;
            }
            else
            {
                Debug.LogError("子オブジェクトにRectTransformがありません。");
                enabled = false;
                return;
            }
        }
        else
        {
            Debug.LogError("子オブジェクト（画像）がありません。2枚の画像をこのオブジェクトの子として配置してください。");
            enabled = false;
        }

        // 開始時に、2枚の画像が並んでいるか確認するチェック
        if (transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition.x != 0 ||
            transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.x != imageWidth)
        {
            Debug.LogWarning("子画像の位置が正しく設定されていません。手動で調整してください。");
        }
    }

    void Update()
    {
        // 1. コンテナ自体を移動させる
        float newX = containerRectTransform.anchoredPosition.x + scrollSpeed * Time.deltaTime;

        // 2. ループ処理
        // コンテナが画像1枚分の幅だけ移動したら、元の位置に戻します。
        if (scrollSpeed < 0) // 左へ移動する場合
        {
            // X位置が -imageWidth より左へ行ったら、0の位置に戻す
            if (newX < -imageWidth)
            {
                newX += imageWidth;
            }
        }
        else if (scrollSpeed > 0) // 右へ移動する場合
        {
            // X位置が imageWidth より右へ行ったら、0の位置に戻す
            if (newX > imageWidth)
            {
                newX -= imageWidth;
            }
        }

        // 3. 新しい位置を適用
        containerRectTransform.anchoredPosition = new Vector2(newX, containerRectTransform.anchoredPosition.y);
    }
}