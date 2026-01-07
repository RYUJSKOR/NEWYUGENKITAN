using TMPro;               // ← UIテキスト
using UnityEngine;         // ← Unity基本

// ← ランク種別（数値小さいほど悪い：D=0, ... , S=4）
public enum RankGrade { D = 0, C = 1, B = 2, A = 3, S = 4 }

public class ScoreManager : MonoBehaviour
{
    // ===== UI参照 =====
    [SerializeField] private TextMeshProUGUI TimeText;
    [SerializeField] private TextMeshProUGUI HPBounusText;
    [SerializeField] private TextMeshProUGUI Rank;

    // ===== 参照 =====
    private GameManager gameManager;
    private TimerManager timerManager;
    private BossTimerManager bossTimerManager;
    private GameData _gameData;

    // 時間ランクの基準（同段階を“同時に”満たす必要あり）=====
    [Header("Time Rank (Speedrun)")]
    [Tooltip("一般マップ: Sはこの秒数以下（以降は1分刻みでA/B/C、超過でD）")]
    [SerializeField] private float normalBaseS = 120f; // 2分
    [Tooltip("ボスマップ: Sはこの秒数以下（以降は1分刻みでA/B/C、超過でD）")]
    [SerializeField] private float bossBaseS = 180f;   // 3分
    [Tooltip("時間の段差（秒）：1ランクごとに増える秒数")]
    [SerializeField] private float stepSeconds = 60f;  // 1分

    // HPランクの基準（同段階を“同時に”満たす必要あり）=====
    [Header("HP Rank")]
    [Tooltip("S段階で必要なHP（これ以上）")]
    [SerializeField] private int baseHPForS = 8;       // S: 8以上
    [Tooltip("1ランク下がるごとに必要HPを減らす個数")]
    [SerializeField] private int hpStep = 2;           // 2個ずつ低下（A:6,B:4,C:2）

    // ===== ライフサイクル =====
    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        timerManager = FindObjectOfType<TimerManager>();
        bossTimerManager = FindAnyObjectByType<BossTimerManager>();
        _gameData = FindAnyObjectByType<GameData>();

        if (GameData.Instance != null)
        {
            ShowPlayTimer();
            ShowHPBonus();
            ShowRank();
        }
    }

    private void Update()
    {
        // 結果画面で常時更新したいなら維持、ワンショット表示ならStartのみでもOK
        ShowPlayTimer();
        ShowHPBonus();
        ShowRank();
    }

    // ===== 表示：プレイ時間 =====
    private void ShowPlayTimer()
    {
        var gd = GameData.Instance;
        if (gd == null || TimeText == null) return;

        // リザルトで採用する時間（Final > 通常）／ボスか通常かで分岐
        bool boss = gd.IsBossClear;
        float time = boss
            ? (gd.Boss1PlayTimeFinal > 0f ? gd.Boss1PlayTimeFinal : gd.Boss1PlayTime)
            : (gd.PlayTimeFinal > 0f ? gd.PlayTimeFinal : gd.PlayTime);

        TimeText.text = TimerManager.FormatTime(time);
    }

    // ===== 表示：HP（リザルト用。In-Gameは画像表示を別途使用）=====
    private void ShowHPBonus()
    {
        if (HPBounusText == null || GameData.Instance == null) return;

        float current = GameData.Instance.PlayerHP;
        float max = GameData.Instance.PlayerMaxHP;
        HPBounusText.text = PlayerHP.FormatHP(current, max);
    }

    // ===== 表示：最終ランク（時間×HPの“同段階”を同時に満たす必要あり）=====
    private void ShowRank()
    {
        var gd = GameData.Instance;
        if (Rank == null || gd == null) return;

        // どの時間基準を使うか：ボスは3分起点、通常は2分起点
        bool isBoss = gd.IsBossClear; // プロジェクトの判定に合わせてOK
        float clearTime = isBoss
            ? (gd.Boss1PlayTimeFinal > 0f ? gd.Boss1PlayTimeFinal : gd.Boss1PlayTime)
            : (gd.PlayTimeFinal > 0f ? gd.PlayTimeFinal : gd.PlayTime);

        int hpCur = Mathf.FloorToInt(gd.PlayerHP);

        RankGrade final = GetCombinedRank(
            timeSec: clearTime,
            baseS: isBoss ? bossBaseS : normalBaseS, // Sの時間上限（ボスは3分、通常は2分）
            step: stepSeconds,                       // 1分ずつ緩和
            hp: hpCur,
            baseHP: baseHPForS,                        // Sは8個以上
            hpStep: hpStep                             // 2個ずつ緩和
        );

        Rank.text = ToLetter(final);
    }

    // ===== 同段階の同時条件を満たすかで決定 =====
    // S: time <= baseS             && hp >= baseHP
    // A: time <= baseS + step      && hp >= baseHP - hpStep
    // B: time <= baseS + 2*step    && hp >= baseHP - 2*hpStep
    // C: time <= baseS + 3*step    && hp >= baseHP - 3*hpStep
    // D: それ以外
    private RankGrade GetCombinedRank(float timeSec, float baseS, float step, int hp, int baseHP, int hpStep)
    {
        if (timeSec <= baseS && hp >= baseHP) return RankGrade.S;
        if (timeSec <= baseS + step && hp >= baseHP - hpStep) return RankGrade.A;
        if (timeSec <= baseS + step * 2f && hp >= baseHP - hpStep * 2) return RankGrade.B;
        if (timeSec <= baseS + step * 3f && hp >= baseHP - hpStep * 3) return RankGrade.C;
        return RankGrade.D;
    }

    // ===== 文字化（"S","A","B","C","D"）=====
    private string ToLetter(RankGrade g)
    {
        switch (g)
        {
            case RankGrade.S: return "S";
            case RankGrade.A: return "A";
            case RankGrade.B: return "B";
            case RankGrade.C: return "C";
            default: return "D";
        }
    }
}