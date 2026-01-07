using UnityEngine;

public class AdvancedLoopScroller : MonoBehaviour
{
    // 移動速度（Inspectorから調整）
    [SerializeField]
    private float scrollSpeed = -20f; // 例: -20f (左へ速めに移動)

    // 2枚の子画像のRectTransformを格納する配列
    private RectTransform[] imageRects;

    // 1枚の画像の幅
    private float imageWidth;

    void Start()
    {
        // 2枚の子オブジェクト（画像）を取得
        if (transform.childCount < 2)
        {
            Debug.LogError("このオブジェクトの子として、最低2枚の画像を配置してください。");
            enabled = false;
            return;
        }

        imageRects = new RectTransform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            imageRects[i] = transform.GetChild(i).GetComponent<RectTransform>();
            if (imageRects[i] == null)
            {
                Debug.LogError("全ての子オブジェクトにRectTransformが必要です。");
                enabled = false;
                return;
            }
        }

        // 1枚目の画像の幅を取得（ループ基準）
        imageWidth = imageRects[0].sizeDelta.x;
        if (imageWidth <= 0)
        {
            Debug.LogError("画像の幅が0以下です。RectTransformのWidthを設定してください。");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        // 全ての画像を移動させる
        for (int i = 0; i < imageRects.Length; i++)
        {
            RectTransform rect = imageRects[i];

            // ★修正: Time.deltaTime → Time.unscaledDeltaTime
            // これで Time.timeScale = 0 (ポーズ中など) でも背景が止まらず動きます
            float newX = rect.anchoredPosition.x + scrollSpeed * Time.unscaledDeltaTime;
            rect.anchoredPosition = new Vector2(newX, rect.anchoredPosition.y);
        }

        // ループのチェックと位置のリセット
        if (scrollSpeed < 0) // 左へ移動している場合
        {
            // 0番目の画像が完全に画面外（-imageWidth）に出たら
            if (imageRects[0].anchoredPosition.x < -imageWidth)
            {
                // その画像を、今画面に残っている2番目の画像の右端（一番後ろ）へテレポートさせる
                float newPosX = imageRects[1].anchoredPosition.x + imageWidth;
                imageRects[0].anchoredPosition = new Vector2(newPosX, imageRects[0].anchoredPosition.y);

                // 配列の要素を入れ替える (0 -> 1, 1 -> 0)
                SwapArrayElements(0, 1);
            }
        }
        else if (scrollSpeed > 0) // 右へ移動している場合
        {
            // 0番目の画像が画面外に出たら
            if (imageRects[0].anchoredPosition.x > imageWidth)
            {
                // その画像を、今画面に残っている2番目の画像の左端（一番後ろ）へテレポートさせる
                float newPosX = imageRects[1].anchoredPosition.x - imageWidth;
                imageRects[0].anchoredPosition = new Vector2(newPosX, imageRects[0].anchoredPosition.y);

                // 配列の要素を入れ替える
                SwapArrayElements(0, 1);
            }
        }
    }

    // 配列内の要素を入れ替えるヘルパーメソッド
    private void SwapArrayElements(int indexA, int indexB)
    {
        RectTransform temp = imageRects[indexA];
        imageRects[indexA] = imageRects[indexB];
        imageRects[indexB] = temp;
    }
}