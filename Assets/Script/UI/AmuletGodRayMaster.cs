using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // DOTween

public class AmuletGodRayMaster : MonoBehaviour
{
    // =================================================
    // 設定用クラス
    // =================================================
    [System.Serializable]
    public class AmuletTypeSettings
    {
        public string label;
        public CardAmuletEffect finalCard;
        public Color rayColor = Color.white;
    }

    [Header("監視対象設定 (3種類)")]
    public List<AmuletTypeSettings> amuletTypes = new List<AmuletTypeSettings>();

    // =================================================
    // 参照 & パラメータ
    // =================================================
    [Header("放射光エフェクト (UI Image)")]
    public Image radialLightImage;

    [Header("演出設定 (遅延ありバースト)")]
    [Tooltip("ゲージが溜まってから発動するまでの「タメ」時間（秒）")]
    public float startDelay = 0.2f; // ★ 0.1 ~ 0.3秒くらいが自然です

    [Tooltip("拡大して消えるまでの時間（秒）")]
    public float burstDuration = 0.5f;

    [Tooltip("どれくらい拡大するか（倍率）")]
    public float burstScale = 6.0f;

    // 内部変数
    private Sequence masterSequence;
    private bool isEffectTriggered = false;

    // =================================================
    // 初期化 & 更新処理
    // =================================================

    void Start()
    {
        ForceResetEffect();
    }

    void Update()
    {
        CheckFinalCards();
    }

    // =================================================
    // メインロジック
    // =================================================

    private void CheckFinalCards()
    {
        AmuletTypeSettings activeType = null;

        foreach (var type in amuletTypes)
        {
            if (type.finalCard == null || type.finalCard.cardImage == null) continue;

            // 5番目のカードが満タン(0.99以上)かチェック
            if (type.finalCard.cardImage.fillAmount >= 0.99f)
            {
                activeType = type;
                break;
            }
        }

        if (activeType != null)
        {
            // まだ演出していないなら発動
            if (!isEffectTriggered)
            {
                // フラグは即座に立てる（重複実行防止）
                isEffectTriggered = true;
                PlayBurstEffectWithDelay(activeType.rayColor);
            }
        }
        else
        {
            // ゲージが減った（スキル使用） -> リセット
            if (isEffectTriggered)
            {
                isEffectTriggered = false;
                ForceResetEffect();
            }
        }
    }

    // =================================================
    // 演出制御 (Delay -> Burst)
    // =================================================
    private void PlayBurstEffectWithDelay(Color targetColor)
    {
        if (radialLightImage == null) return;

        // 前の演出があればキル
        if (masterSequence != null) masterSequence.Kill();

        // ★シーケンス開始
        masterSequence = DOTween.Sequence();

        // [Phase 1] ほんの少し待つ (タメ)
        masterSequence.AppendInterval(startDelay);

        // [Phase 2] セットアップ (Callback)
        // 待機時間が終わった瞬間に実行される処理
        masterSequence.AppendCallback(() =>
        {
            radialLightImage.gameObject.SetActive(true);
            radialLightImage.transform.localScale = Vector3.one;

            // 角度ランダム
            float randomAngle = Random.Range(0f, 360f);
            radialLightImage.transform.localEulerAngles = new Vector3(0, 0, randomAngle);

            // 色設定
            radialLightImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1.0f);
        });

        // [Phase 3] バーストアニメーション (拡大 & フェードアウト)
        masterSequence.Append(radialLightImage.transform.DOScale(Vector3.one * burstScale, burstDuration)
            .SetEase(Ease.OutExpo));

        masterSequence.Join(radialLightImage.DOFade(0f, burstDuration)
            .SetEase(Ease.InQuad));

        // [End] 終了処理
        masterSequence.OnComplete(() =>
        {
            radialLightImage.gameObject.SetActive(false);
            radialLightImage.transform.localScale = Vector3.one;
        });
    }

    // 強制リセット
    private void ForceResetEffect()
    {
        if (masterSequence != null) masterSequence.Kill();

        if (radialLightImage != null)
        {
            radialLightImage.transform.localScale = Vector3.one;
            radialLightImage.color = new Color(1, 1, 1, 0);
            radialLightImage.gameObject.SetActive(false);
        }
    }
}