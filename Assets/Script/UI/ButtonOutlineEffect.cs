using UnityEngine;
using UnityEngine.EventSystems;

// ボタンに選択されたときに輪郭と縮小アニメーションを適用するスクリプト
public class ButtonOutlineEffect : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject outlineImage;  // 輪郭画像
    private Vector3 originalScale;                     // 元のサイズ
    private Vector3 focusedScale;                      // 縮小されたサイズ
    private float scaleSpeed = 8f;                     // アニメーションのスピード
    private bool isFocused = false;                    // 現在フォーカス中かどうか

    void Start()
    {
        // 初期サイズを保存
        originalScale = transform.localScale;
        focusedScale = originalScale * 0.95f; // 少し小さく見せる

        if (outlineImage != null)
            outlineImage.SetActive(false);
    }

    void Update()
    {
        // フォーカスされていれば縮小、されていなければ元に戻す
        if (isFocused)
            transform.localScale = Vector3.Lerp(transform.localScale, focusedScale, Time.unscaledDeltaTime * scaleSpeed);
        else
            transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.unscaledDeltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetFocus(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetFocus(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        SetFocus(true);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        SetFocus(false);
    }

    private void SetFocus(bool focus)
    {
        isFocused = focus;

        if (outlineImage != null)
            outlineImage.SetActive(focus);
    }
}
