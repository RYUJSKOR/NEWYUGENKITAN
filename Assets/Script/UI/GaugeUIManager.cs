using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class GaugeUIManager : MonoBehaviour
{
    // ゲージ表示に使用する Image（現在有効な5枚）
    [SerializeField] private Image[] maskImages;

    // プレイヤーの Shooting 参照（モード値を取得する）
    [SerializeField] private Shooting shooting;

    // ゲージ値の供給元（BulletSkill）
    private BulletSkill bulletSkill;

    // 1つの Image が担当するゲージ幅
    private const float gaugePerMask = 1f;

    // 最後に確認したモード値（変化検出用）
    private int cachedMode = -1;

    // モードごとのゲージセット
    // root: セットの親オブジェクト（ON/OFF切り替えに使用）
    // images: セットに含まれる5枚のゲージ Image
    [System.Serializable]
    public class ModeGaugeSet
    {
        public GameObject root;
        public Image[] images;
    }

    [Header("モードごとのゲージセット（各モードごとに5枚）")]
    [SerializeField] private ModeGaugeSet[] modeSets;

    // 初期化処理（BulletSkillを受け取り、現在モードのセットを適用）
    public void Init(BulletSkill owner)
    {
        bulletSkill = owner;

        int initMode = ResolveModeFromShooting();
        cachedMode = -1;           // 強制的にセット切替
        ApplyModeSet(initMode);
        cachedMode = initMode;
    }

    private void Update()
    {
        // モード変化を検出してセット切替
        int mode = ResolveModeFromShooting();
        if (mode != cachedMode)
        {
            ApplyModeSet(mode);
            cachedMode = mode;
        }

        // ゲージが無い場合は終了
        if (bulletSkill == null) return;

        // 現在のゲージ値を取得
        float gauge = bulletSkill.GetGauge();

        // 5枚のImageに順番にfillAmountを割り当て
        for (int i = 0; i < maskImages.Length; i++)
        {
            if (maskImages[i] == null) continue;

            float value = gauge - i * gaugePerMask;
            maskImages[i].fillAmount =
                (value <= 0f) ? 0f :
                (value >= 1f) ? 1f : value;
        }
    }

    // Shooting から現在モード(0/1/2)を取得する
    // GetMode() が無ければ代表的な名前のProperty/Fieldを探す
    private int ResolveModeFromShooting()
    {
        if (shooting == null) return -1;
        var bullet = shooting.GetBulletObject();
        if (bullet == null) return -1;

        string name = bullet.name.Replace("(Clone)", "");

        if (name == "NohMaskBullet") return 0;
        if (name == "DemonBullet") return 1;
        if (name == "FoxBullet") return 2;

        return -1;
    }


    // モードごとのセットを切り替える
    // ・すべてのセットを一度OFFにして、対象モードのセットだけONにする
    // ・maskImages参照を対象セットのImage配列に差し替える
    private void ApplyModeSet(int mode)
    {
        if (modeSets == null || modeSets.Length == 0) return;

        if (mode < 0 || mode >= modeSets.Length)
        {
            // 範囲外のモードなら全てOFF
            for (int i = 0; i < modeSets.Length; i++)
                if (modeSets[i]?.root) modeSets[i].root.SetActive(false);

            return;
        }

        // ON/OFF切り替え
        for (int i = 0; i < modeSets.Length; i++)
        {
            var set = modeSets[i];
            if (set == null) continue;
            if (set.root != null) set.root.SetActive(i == mode);
        }

        // 有効化したセットのImageをmaskImagesに適用
        var activeSet = modeSets[mode];
        if (activeSet != null && activeSet.images != null && activeSet.images.Length > 0)
        {
            maskImages = activeSet.images;

            // 安全のため type を Filled に統一
            for (int i = 0; i < maskImages.Length; i++)
            {
                if (maskImages[i] == null) continue;
                if (maskImages[i].type != Image.Type.Filled)
                    maskImages[i].type = Image.Type.Filled;
            }
        }
    }
}
