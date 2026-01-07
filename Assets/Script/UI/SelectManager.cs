using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Selectable))]
public class SelectManager : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    // ---------- 下線ターゲット ----------
    [Header("下線ターゲット")]
    [SerializeField] private RectTransform underlineRect;
    [SerializeField] private Image underlineImage;
    [SerializeField] private bool useFillAmount = false;

    // ---------- アニメ設定 ----------
    // 消去（退場）にかける時間（秒）
    [Header("アニメーション設定")]
    [SerializeField, Min(0.01f)] private float eraseDuration = 0.15f;

    // イージング
    [SerializeField] private Ease easeType = Ease.Linear;

    // ポーズ中でも動く（TimeScaleの影響を受けない）
    [SerializeField] private bool unscaledUpdate = true;

    // ---------- 効果音 ----------
    [Header("効果音")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip drawClip; // 選択時の音

    // 選択解除で完全に非表示にするか
    [SerializeField] private bool deactivateOnDeselect = true;

    // ---------- 選択統一（任意） ----------
    [Header("選択統一（任意）")]
    [SerializeField] private bool selectOnHover = true;

    // ---------- 内部制御 ----------
    private Tween currentTween;
    private bool isShown = false;

    // ---------- コントローラ保証 ----------
    [Header("初期選択（コントローラ保証）")]
    [SerializeField] private Selectable defaultSelectable;
    [SerializeField] private bool forceKeepSelection = true;
    [SerializeField] private bool deselectOnPointerExit = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponentInChildren<AudioSource>();

        ResetUnderline(instant: true);
    }

    private void Update()
    {
        if (!forceKeepSelection || EventSystem.current == null) return;

        if (!IsValidSelection(EventSystem.current.currentSelectedGameObject) && HasNavigateIntent())
        {
            if (defaultSelectable == null) defaultSelectable = GetComponent<Selectable>();
            SelectDefault();
        }
    }

    // 有効化された際に見た目をリセット
    private void OnEnable()
    {
        ResetUnderline(instant: true);

        // もし今、自分が「選択されている」なら、即座に表示する
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject)
        {
            if (underlineRect != null) underlineRect.gameObject.SetActive(true);
            if (underlineImage != null) underlineImage.gameObject.SetActive(true);
            SetProgress(1f);
            isShown = true;
        }

        if (forceKeepSelection && EventSystem.current != null)
        {
            if (!IsValidSelection(EventSystem.current.currentSelectedGameObject))
            {
                if (defaultSelectable == null) defaultSelectable = GetComponent<Selectable>();
                SelectDefault();
            }
        }
    }

    // ボタンが選択された時
    public void OnSelect(BaseEventData eventData)
    {
        if (isShown) return;

        currentTween?.Kill();

        if (deactivateOnDeselect)
        {
            if (underlineRect != null) underlineRect.gameObject.SetActive(true);
            if (underlineImage != null) underlineImage.gameObject.SetActive(true);
        }

        // ★修正: アニメーションさせず、一瞬で最大(1f)にする
        SetProgress(1f);

        // 効果音再生
        if (audioSource != null && drawClip != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(drawClip);
        }

        isShown = true;
    }

    // ポインタがボタン外へ出た時
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selectOnHover) return;

        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == gameObject)
        {
            if (deselectOnPointerExit)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }

            currentTween?.Kill();
            PlayEraseTween(eraseDuration);

            currentTween.OnComplete(() =>
            {
                if (deactivateOnDeselect)
                {
                    if (underlineRect != null) underlineRect.gameObject.SetActive(false);
                    if (underlineImage != null) underlineImage.gameObject.SetActive(false);
                }
            });

            isShown = false;
        }
    }

    // ボタンの選択が外れた時
    public void OnDeselect(BaseEventData eventData)
    {
        currentTween?.Kill();

        currentTween = DOTween.To(() => GetProgress(), v => SetProgress(v), 0f, eraseDuration)
                       .SetEase(easeType)
                       .SetUpdate(unscaledUpdate)
                       .SetLink(gameObject) // ★追加: シーン遷移時のエラー防止
                       .OnComplete(() =>
                       {
                           if (deactivateOnDeselect)
                           {
                               if (underlineRect != null) underlineRect.gameObject.SetActive(false);
                               if (underlineImage != null) underlineImage.gameObject.SetActive(false);
                           }
                       });
        isShown = false;
    }

    private void OnDisable()
    {
        currentTween?.Kill();

        if (useFillAmount && underlineImage != null)
        {
            underlineImage.fillAmount = 0f;
            if (deactivateOnDeselect) underlineImage.gameObject.SetActive(false);
        }
        else if (underlineRect != null)
        {
            var s = underlineRect.localScale;
            s.x = 0f;
            underlineRect.localScale = s;
            if (deactivateOnDeselect) underlineRect.gameObject.SetActive(false);
        }

        isShown = false;
    }

    // マウスが乗った時に選択状態へ
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selectOnHover) return;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != gameObject)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    // ===== 内部メソッド =====

    private void ResetUnderline(bool instant)
    {
        if (useFillAmount)
        {
            if (underlineImage != null)
            {
                if (instant) underlineImage.fillAmount = 0f;
                else underlineImage.fillAmount = Mathf.Clamp01(underlineImage.fillAmount);
            }
        }
        else
        {
            if (underlineRect != null)
            {
                var ls = underlineRect.localScale;
                ls.x = instant ? 0f : Mathf.Clamp01(ls.x);
                underlineRect.localScale = ls;
            }
        }
        isShown = false;
        currentTween?.Kill();
        currentTween = null;
    }

    private float GetProgress()
    {
        if (useFillAmount && underlineImage != null) return underlineImage.fillAmount;
        if (!useFillAmount && underlineRect != null) return underlineRect.localScale.x;
        return 0f;
    }

    private void SetProgress(float p)
    {
        p = Mathf.Clamp01(p);
        if (useFillAmount)
        {
            if (underlineImage != null) underlineImage.fillAmount = p;
        }
        else
        {
            if (underlineRect != null)
            {
                var ls = underlineRect.localScale;
                ls.x = p;
                underlineRect.localScale = ls;
            }
        }
    }

    // 消去Tween（現在→0へ）
    private void PlayEraseTween(float duration)
    {
        float from = GetProgress();

        if (useFillAmount && underlineImage != null)
        {
            currentTween = DOTween.To(
                    () => from,
                    v => underlineImage.fillAmount = v,
                    0f, duration)
                .SetEase(easeType)
                .SetUpdate(unscaledUpdate)
                .SetLink(gameObject); // ★追加: 安全対策
        }
        else if (!useFillAmount && underlineRect != null)
        {
            currentTween = underlineRect
                .DOScaleX(0f, duration)
                .SetEase(easeType)
                .SetUpdate(unscaledUpdate)
                .SetLink(gameObject); // ★追加: 安全対策
        }
    }

    // ===== 有効な選択か判定 =====
    private bool IsValidSelection(GameObject go)
    {
        if (go == null) return false;
        if (!go.activeInHierarchy) return false;
        var sel = go.GetComponent<Selectable>();
        if (sel == null) return false;
        if (!sel.IsInteractable()) return false;
        return true;
    }

    // ===== デフォルト選択に戻す =====
    private void SelectDefault()
    {
        if (defaultSelectable == null) return;
        if (!defaultSelectable.gameObject.activeInHierarchy) return;
        if (!defaultSelectable.IsInteractable()) return;
        defaultSelectable.Select();
    }

    // ===== ナビ意図の簡易検出 =====
    private bool HasNavigateIntent()
    {
        float h = 0f, v = 0f;
        try
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
        }
        catch { }
        return Input.anyKeyDown || Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
    }
}