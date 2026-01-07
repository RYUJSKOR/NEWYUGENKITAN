using UnityEngine;
using System;

// ===== 通常ステージのタイマー =====
public class TimerManager : MonoBehaviour
{
    // 開始時刻（Time.time）
    private float StartTimer;
    // 経過秒（常に「経過値」を保持）
    public float ElapsedTimer { get; private set; }

    // クリアフラグ（外部から立つ）
    public bool VictoryGame = false;

    private bool _running = false;  // 計測中フラグ
    private bool _stopped = false;  // StopTimer一度きりガード

    private GameData _gameData;

    private void Start()
    {
        _gameData = GameData.Instance;
        if (_gameData != null) _gameData.IsBossStage = false; // ★ 通常マップ開始
        StartTimer = Time.time;
        ElapsedTimer = 0f;
        VictoryGame = false;
        _running = true;  // ★ 自動開始
    }

    private void Update()
    {
        // ★ 計測はプレイ中のみ
        if (_running && !VictoryGame)
        {
            CountTime();
        }
        else
        {
            // ★ 勝利後は一度だけ確定保存
            if (!_stopped) StopTimer();
        }
    }

    // ===== 経過秒を更新 =====
    private void CountTime()
    {
        ElapsedTimer = Time.time - StartTimer;      // ★ 純粋な経過秒
        if (_gameData != null) _gameData.saveTime(ElapsedTimer); // 生値ミラー（任意）
    }

    // ===== 停止して最終値を確定（スナップショット）=====
    public float StopTimer()
    {
        if (_stopped) return ElapsedTimer; // ★ 二重保存防止
        _stopped = true;

        // 最終経過秒を確定
        ElapsedTimer = Time.time - StartTimer;
        _running = false;

        if (_gameData != null)
        {
            _gameData.saveTime(ElapsedTimer);     // 生値最終
            _gameData.PlayTimeFinal = ElapsedTimer; // ★ 結果用スナップショット
            Debug.Log("[TimerManager] Time saved: " + FormatTime(ElapsedTimer));
        }
        return ElapsedTimer;
    }

    // ===== mm:ss 文字列 =====
    public static string FormatTime(float timeInSeconds)
    {
        var t = TimeSpan.FromSeconds(timeInSeconds);
        return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }
}
