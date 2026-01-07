using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening; // DOTween必須

[RequireComponent(typeof(Selectable))]
public class ButtonSubmitEffect : MonoBehaviour, ISubmitHandler, IPointerClickHandler
{
    [Header("アニメーション設定")]
    [Tooltip("へこむ強さ (0.1 ～ 0.5 くらい推奨)")]
    [SerializeField] private float punchStrength = 0.2f;

    [Tooltip("アニメーション時間")]
    [SerializeField] private float duration = 0.2f;

    [Tooltip("振動の回数")]
    [SerializeField] private int vibrato = 10;

    [Tooltip("弾力 (0～1)")]
    [SerializeField] private float elasticity = 1f;

    [Header("効果音 (任意)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip submitSound;

    private bool isAnimating = false;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

        // AudioSourceがアタッチされていなくて、自分自身についているなら取得
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // 有効化されたときにサイズがおかしくなっていたら直す
        transform.localScale = originalScale;
        isAnimating = false;
    }

    // ■ キーボードのEnter / ゲームパッドのAボタンなどを押した時
    public void OnSubmit(BaseEventData eventData)
    {
        PlaySubmitEffect();
    }

    // ■ マウスでクリックした時
    public void OnPointerClick(PointerEventData eventData)
    {
        PlaySubmitEffect();
    }

    private void PlaySubmitEffect()
    {
        // 連打防止（アニメ中は無視するか、リセットするか。今回はリセットして再生）
        transform.DOKill();
        transform.localScale = originalScale;

        // 1. 効果音再生
        if (audioSource != null && submitSound != null)
        {
            audioSource.PlayOneShot(submitSound);
        }

        // 2. 「ポヨン」と弾むアニメーション (DOPunchScale)
        // マイナスの値を渡すと「縮んでから戻る（押した感じ）」になります
        transform.DOPunchScale(Vector3.one * -punchStrength, duration, vibrato, elasticity)
            .SetUpdate(true) // ポーズ中でも動くように
            .SetLink(gameObject) // シーン遷移で消えてもエラーにならないように
            .OnComplete(() =>
            {
                transform.localScale = originalScale;
            });
    }

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
    }
}