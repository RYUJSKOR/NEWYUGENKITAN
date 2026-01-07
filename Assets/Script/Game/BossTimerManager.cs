using UnityEngine;
using System;
using GLTFast;

// ===== ボスステージのタイマー =====
public class BossTimerManager : MonoBehaviour
{
    private bool dontDestroyOnLoad = false;

    private float StartTimer;
    public float ElapsedTimer { get; private set; }

    private static BossTimerManager _instance;

    public bool VictoryGame = false;

    private bool _running = false;
    private bool _stopped = false;

    private GameData _gameData;

    private void Awake()
    {
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        // ★ シングルトンガード
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _gameData = GameData.Instance;
        if (_gameData != null) _gameData.IsBossStage = true; // ★ ボスマップ開始
        StartTimer = Time.time;
        ElapsedTimer = 0f;
        VictoryGame = false;
        _running = true;  // ★ 自動開始

        // ※ DontDestroyOnLoad は付けない（結果シーンに持ち込まない）
    }

    private void Update()
    {
        if (_running && !VictoryGame)
        {
            ElapsedTimer = Time.time - StartTimer;
            if (_gameData != null) _gameData.saveBossTime(ElapsedTimer);
        }
        else
        {
            if (!_stopped) StopTimer(); // ★ 勝利後は一度だけ確定保存
        }
    }

    // ===== 停止＆最終保存 =====
    public float StopTimer()
    {
        if (_stopped) return ElapsedTimer;
        _stopped = true;

        ElapsedTimer = Time.time - StartTimer;
        _running = false;

        if (_gameData != null)
        {
            _gameData.saveBossTime(ElapsedTimer);        // 生値最終
            _gameData.Boss1PlayTimeFinal = ElapsedTimer; // ★ 結果用スナップショット
            Debug.Log("[BossTimerManager] Boss time saved: " + FormatTime(ElapsedTimer));
        }
        return ElapsedTimer;
    }

    public static string FormatTime(float timeInSeconds)
    {
        return TimerManager.FormatTime(timeInSeconds);
    }

    // （必要なら）任意秒から再開したい場合の補助
    public void ResetTimer(float a)
    {
        ElapsedTimer = a;
        StartTimer = Time.time - a; // ★ 「いまからa秒前」を基準に
        _running = true;
        _stopped = false;
        VictoryGame = false;
    }
}
