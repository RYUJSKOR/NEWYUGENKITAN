using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // パーティクル制御などに使う可能性があるため残しています

public class CardAmuletEffect : MonoBehaviour
{
    [Header("必須設定 (Inspector)")]
    public Image cardImage;
    public ParticleSystem sparkleParticle;

    [Header("点滅設定 (Global Sync)")]
    // すべてのカードがこの速度を共有するため、同期して点滅します。
    public float flashSpeed = 5.0f;

    [Header("光の設定 (文字ネオン)")]
    [ColorUsage(true, true)]
    public Color glowColor = new Color(3.0f, 3.0f, 3.0f);

    [Header("光の設定 (パーティクル)")]
    [ColorUsage(true, true)]
    public Color individualSparkleColor = Color.yellow;

    [Header("紙の色設定 (点滅アニメーション)")]
    public Color cardBodyTint = new Color(0.5f, 0.5f, 0.5f, 1.0f); // 暗い時の色
    public Color cardBodyFlashColor = new Color(1.0f, 1.0f, 1.0f, 1.0f); // 明るい時の色

    // 内部変数
    private Material matInstance;
    private bool isGlowing = false;

    // シェーダープロパティID
    private readonly int emissionColorID = Shader.PropertyToID("_EmissionColor");
    private readonly int baseColorID = Shader.PropertyToID("_BaseColor");

    void Awake()
    {
        if (cardImage == null) cardImage = GetComponent<Image>();

        if (cardImage != null && matInstance == null)
        {
            matInstance = new Material(cardImage.material);
            cardImage.material = matInstance;
            matInstance.EnableKeyword("_EMISSION");

            // 初期カラー設定
            matInstance.SetColor(baseColorID, cardBodyTint);
            matInstance.SetColor(emissionColorID, Color.black);
        }

        if (sparkleParticle != null)
        {
            // パーティクルを停止状態で初期化
            sparkleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void OnDisable()
    {
        // オブジェクト無効化時に即座に発光をオフにする
        DeactivateGlow();
    }

    void Update()
    {
        if (cardImage == null || matInstance == null) return;

        // 1. ゲージチェック (99%以上で点灯)
        if (cardImage.fillAmount >= 0.99f)
        {
            if (!isGlowing) ActivateGlow();

            // ★★★ ここが重要: DOTweenの代わりにUpdateでリアルタイム計算 ★★★
            // Time.timeはゲーム全体の時間なので、すべてのカードが同じwave値を持ちます。
            // これにより、カードがいつ有効になっても点滅のタイミングが完全に同期します。
            float wave = (Mathf.Sin(Time.time * flashSpeed) + 1f) / 2f;

            // 背景色の同期 (Lerp)
            Color currentBodyColor = Color.Lerp(cardBodyTint, cardBodyFlashColor, wave);
            matInstance.SetColor(baseColorID, currentBodyColor);

            // 文字ネオンの同期 (Lerp) - 黒からglowColorへ点滅
            Color currentEmissionColor = Color.Lerp(Color.black, glowColor, wave);
            matInstance.SetColor(emissionColorID, currentEmissionColor);
        }
        else
        {
            // ゲージが足りない場合は発光停止
            if (isGlowing) DeactivateGlow();
        }
    }

    // [発光開始]
    public void ActivateGlow()
    {
        isGlowing = true;

        // パーティクルは非同期でも問題ないのでそのまま再生
        if (sparkleParticle != null)
        {
            sparkleParticle.gameObject.SetActive(true);
            var mainSettings = sparkleParticle.main;
            mainSettings.startColor = individualSparkleColor;
            sparkleParticle.Play();
        }
    }

    // [発光停止]
    public void DeactivateGlow()
    {
        isGlowing = false;

        if (matInstance != null)
        {
            // 色の初期化 (暗い状態で固定)
            matInstance.SetColor(baseColorID, cardBodyTint);
            matInstance.SetColor(emissionColorID, Color.black);
        }

        if (sparkleParticle != null)
        {
            sparkleParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void OnDestroy()
    {
        if (matInstance != null) Destroy(matInstance);
    }
}