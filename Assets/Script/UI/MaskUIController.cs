using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MaskUIController : MonoBehaviour
{
    [SerializeField] private Shooting shooting;
    [SerializeField] private PlayerStateMachine stateMachine;

    private Color centerDefaultColor = Color.white;
    private Color sideDefaultColor = Color.white;
    private readonly Color disabledGray = new Color(0.6f, 0.6f, 0.6f, 1f);

    [SerializeField] private Image centerImage;
    [SerializeField] private Image leftImage;
    [SerializeField] private Image rightImage;

    [Header("カラー（メイン）の仮面スプライト")]
    [SerializeField] private Sprite[] maskSprites;

    // ★追加: グレー（サイド用）の仮面スプライトを格納する配列
    [Header("グレー（サイド用）の仮面スプライト")]
    [SerializeField] private Sprite[] grayMaskSprites;

    private List<string> bulletNames = new() { "NohMaskBullet", "DemonBullet", "FoxBullet" };
    private GameObject lastBullet;

    // 位置は「エディタに置いた場所」を正解として記憶する
    private Vector2 centerPos;
    private Vector2 leftPos;
    private Vector2 rightPos;

    [Header("サイズの正解値")]
    [SerializeField] private Vector3 targetCenterScale = Vector3.one;
    [SerializeField] private Vector3 targetSideScale = new Vector3(0.35f, 0.35f, 1f);

    void Start()
    {
        if (stateMachine == null)
            stateMachine = FindFirstObjectByType<PlayerStateMachine>();

        if (centerImage != null) centerPos = centerImage.rectTransform.anchoredPosition;
        if (leftImage != null) leftPos = leftImage.rectTransform.anchoredPosition;
        if (rightImage != null) rightPos = rightImage.rectTransform.anchoredPosition;

        // 開始した瞬間に、強制的に「正解のサイズ」にする
        if (centerImage != null) centerImage.rectTransform.localScale = targetCenterScale;
        if (leftImage != null) leftImage.rectTransform.localScale = targetSideScale;
        if (rightImage != null) rightImage.rectTransform.localScale = targetSideScale;

        lastBullet = shooting.GetBulletObject();

        if (centerImage != null) centerDefaultColor = centerImage.color;
        if (leftImage != null) sideDefaultColor = leftImage.color;
        if (rightImage != null) sideDefaultColor = rightImage.color;

        ApplyLockColors();
    }

    void Update()
    {
        var current = shooting.GetBulletObject();
        if (current == null) return;

        if (lastBullet == null)
        {
            lastBullet = current;
            ApplyLockColors();
            return;
        }

        if (current != lastBullet)
        {
            lastBullet = current;
            UpdateUI();
            ApplyLockColors();
        }

        ApplyLockColors();
    }

    void UpdateUI()
    {
        if (shooting.GetBulletObject() == null) return;

        centerImage.gameObject.SetActive(true);
        leftImage.gameObject.SetActive(true);
        rightImage.gameObject.SetActive(true);

        string currentBulletName = shooting.GetBulletObject().name.Replace("(Clone)", "");
        int currentIndex = bulletNames.IndexOf(currentBulletName);
        if (currentIndex == -1) return;

        int nextIndex = (currentIndex + 1) % bulletNames.Count;
        int afterNextIndex = (currentIndex + 2) % bulletNames.Count;

        centerImage.rectTransform.DOKill();
        leftImage.rectTransform.DOKill();
        rightImage.rectTransform.DOKill();

        // アニメーション設定
        leftImage.rectTransform.DOAnchorPos(centerPos, 0.4f).SetEase(Ease.OutQuad);
        leftImage.rectTransform.DOScale(targetCenterScale, 0.4f).SetEase(Ease.OutQuad);

        centerImage.rectTransform.DOAnchorPos(rightPos, 0.4f).SetEase(Ease.OutQuad);
        centerImage.rectTransform.DOScale(targetSideScale, 0.4f).SetEase(Ease.OutQuad);

        rightImage.rectTransform.DOAnchorPos(leftPos, 0.4f).SetEase(Ease.OutQuad);
        rightImage.rectTransform.DOScale(targetSideScale, 0.4f).SetEase(Ease.OutQuad);

        // 表示順序（描画順）の入れ替え
        rightImage.transform.SetSiblingIndex(0);
        centerImage.transform.SetSiblingIndex(1);
        leftImage.transform.SetAsLastSibling();

        // 参照の入れ替え
        var tmp = centerImage;
        centerImage = leftImage;
        leftImage = rightImage;
        rightImage = tmp;

        // ★修正: メイン（中央）はカラー画像、他はグレー画像（grayMaskSprites）を適用
        // 配列の順番は maskSprites と同じにしてください（能面、鬼、狐の順）
        centerImage.sprite = maskSprites[currentIndex];        // メイン: カラー画像

        // グレー画像配列が割り当てられており、サイズが一致する場合のみ適用（エラー防止）
        if (grayMaskSprites != null && grayMaskSprites.Length == maskSprites.Length)
        {
            leftImage.sprite = grayMaskSprites[nextIndex];         // 左: グレー画像
            rightImage.sprite = grayMaskSprites[afterNextIndex];   // 右: グレー画像
        }
        else
        {
            // 万が一グレー画像が設定されていない場合は、既存のロジック（カラー）を使用
            leftImage.sprite = maskSprites[nextIndex];
            rightImage.sprite = maskSprites[afterNextIndex];
        }
    }

    private bool IsFoxLocked()
    {
        if (stateMachine == null) return false;
        var fox = stateMachine.GetState<FoxSkill>();
        return fox != null && fox.IsInCounterMode;
    }

    private void ApplyLockColors()
    {
        bool locked = IsFoxLocked();

        if (!locked)
        {
            if (centerImage != null) centerImage.color = centerDefaultColor;
            if (leftImage != null) leftImage.color = sideDefaultColor;
            if (rightImage != null) rightImage.color = sideDefaultColor;
        }
        else
        {
            if (centerImage != null) centerImage.color = centerDefaultColor;
            if (leftImage != null) leftImage.color = disabledGray;
            if (rightImage != null) rightImage.color = disabledGray;
        }
    }
}