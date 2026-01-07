using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class ButtonCloro : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("Text Settings")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color normalColor = Color.white;   // 黒フィルター越しに見える色
    [SerializeField] private Color highlightColor = Color.black; // 金背景に乗る色

    [Header("Background Objects")]
    [Tooltip("テキストより【手前】に置く黒い画像 (CanvasGroup必須)")]
    [SerializeField] private CanvasGroup blackOverlayGroup;

    [Tooltip("テキストより【奥】に置く金の画像 (CanvasGroup必須)")]
    [SerializeField] private CanvasGroup goldBackgroundGroup;

    [Header("Scale Settings")]
    [SerializeField] private float selectScale = 1.1f;
    [SerializeField] private float animDuration = 0.2f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // 有効化時、強制的に初期状態（未選択）に戻す
        // DOTweenが残っていると悪さをするのでリセット
        transform.DOKill();
        if (blackOverlayGroup != null) blackOverlayGroup.DOKill();
        if (goldBackgroundGroup != null) goldBackgroundGroup.DOKill();

        transform.localScale = originalScale;

        // 初期状態：黒ON、金OFF
        if (blackOverlayGroup != null)
        {
            blackOverlayGroup.alpha = 1f;
            blackOverlayGroup.gameObject.SetActive(true);
        }
        if (goldBackgroundGroup != null)
        {
            goldBackgroundGroup.alpha = 0f;
            goldBackgroundGroup.gameObject.SetActive(false);
        }

        if (text != null) text.color = normalColor;
    }

    public void OnSelect(BaseEventData eventData) => ChangeVisual(true);
    public void OnDeselect(BaseEventData eventData) => ChangeVisual(false);
    public void OnPointerEnter(PointerEventData eventData) => ChangeVisual(true);

    public void OnPointerExit(PointerEventData eventData)
    {
        if (EventSystem.current.currentSelectedGameObject != this.gameObject)
        {
            ChangeVisual(false);
        }
    }

    private void ChangeVisual(bool isActive)
    {
        // 1. テキスト色の変更
        if (text != null)
        {
            text.color = isActive ? highlightColor : normalColor;
        }

        // 2. 背景の切り替え (SetActive と Fade の組み合わせ)

        // --- 黒オーバーレイ (未選択で表示) ---
        if (blackOverlayGroup != null)
        {
            blackOverlayGroup.DOKill();
            if (isActive)
            {
                // 選択時: フェードアウト -> 非表示
                blackOverlayGroup.DOFade(0f, animDuration)
                    .SetUpdate(true)
                    .SetLink(gameObject)
                    .OnComplete(() => blackOverlayGroup.gameObject.SetActive(false));
            }
            else
            {
                // 未選択時: 表示 -> フェードイン
                blackOverlayGroup.gameObject.SetActive(true);
                blackOverlayGroup.DOFade(1f, animDuration)
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }
        }

        // --- 金背景 (選択で表示) ---
        if (goldBackgroundGroup != null)
        {
            goldBackgroundGroup.DOKill();
            if (isActive)
            {
                // 選択時: 表示 -> フェードイン
                goldBackgroundGroup.gameObject.SetActive(true);
                goldBackgroundGroup.DOFade(1f, animDuration)
                    .SetUpdate(true)
                    .SetLink(gameObject);
            }
            else
            {
                // 未選択時: フェードアウト -> 非表示
                goldBackgroundGroup.DOFade(0f, animDuration)
                    .SetUpdate(true)
                    .SetLink(gameObject)
                    .OnComplete(() => goldBackgroundGroup.gameObject.SetActive(false));
            }
        }

        // 3. サイズの変更
        transform.DOKill();
        if (isActive)
        {
            transform.DOScale(originalScale * selectScale, animDuration)
                .SetUpdate(true).SetLink(gameObject);
        }
        else
        {
            transform.DOScale(originalScale, animDuration)
                .SetUpdate(true).SetLink(gameObject);
        }
    }

    private void OnDisable()
    {
        // 無効化時はアニメーションを止めてリセット
        transform.DOKill();
        if (blackOverlayGroup != null) blackOverlayGroup.DOKill();
        if (goldBackgroundGroup != null) goldBackgroundGroup.DOKill();

        transform.localScale = originalScale;
    }
}