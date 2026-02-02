using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject ClearUIPrefab;

    [SerializeField]
    GameObject UIParent;

    GameObject ScoreManager;

    [SerializeField]
    BossTimerManager bossTimerManager;

    [SerializeField]
    TimerManager timerManager;

    private PlayerHP _playerHP;

    bool IsClearShown = false;

    public static GameManager Instance;

    private float Timer = 0.0f;

    private SEController SE;

    // private float GoalTimer = 3.0f; // 未使用なら削除可

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Time.timeScale = 1.0f;

        timerManager = FindFirstObjectByType<TimerManager>();
        _playerHP = FindFirstObjectByType<PlayerHP>();
        bossTimerManager = FindFirstObjectByType<BossTimerManager>();
		SE = FindAnyObjectByType<SEController>();

	}

    void Update()
    {
        // 必要なら入力処理など
    }

    // ★変更点: Player側で遷移するので、ここでの自動遷移は廃止しました
    /*
    private IEnumerator LoadNextSceneAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        GetNextResultScene();
    }
    */

    // ★変更点: 古い SceneNavigator を SceneFlowController に置き換え
    private void Restart()
    {
        Time.timeScale = 1.0f;
        if (SceneFlowController.Instance != null)
        {
            SceneFlowController.Instance.RequestScene("RealScene");
        }
    }

    private void LoadTitle()
    {
        Time.timeScale = 1.0f;
        if (SceneFlowController.Instance != null)
        {
            SceneFlowController.Instance.RequestScene("Title");
        }
    }

    public void ChangeOverScene()
    {
        // 死亡時などの遷移
        Time.timeScale = 1.0f;
        if (SceneFlowController.Instance != null)
        {
            SceneFlowController.Instance.RequestScene("Dead");
        }
    }

    // ---------------------------------------------------------

    public void Clear()
    {
        if (timerManager != null)
        {
            timerManager.VictoryGame = true;
            timerManager.StopTimer();
        }

        _playerHP?.StopCountHP();

        var gd = GameData.Instance;
        if (gd != null) gd.IsBossClear = false;

        Instantiate(ClearUIPrefab, UIParent.transform);

        SE.Play("Player.Win");
		// 演出のために時間を止める
		Time.timeScale = 0.0f;

        // ★重要: シーン遷移は Player.cs の OnTriggerEnter で行われるため、
        // ここでの遷移命令（LoadNextSceneAfterDelay）は削除しました。
        // これで「2回ロードしてしまう」バグを防ぎます。
    }

    public void BossClear()
    {
        if (bossTimerManager != null)
        {
            bossTimerManager.VictoryGame = true;
            bossTimerManager.StopTimer();
        }

        _playerHP?.StopCountHP();

        var gd = GameData.Instance;
        if (gd != null) gd.IsBossClear = true;

        Instantiate(ClearUIPrefab, UIParent.transform);

		SE.Play("Player.Win");
		// 演出のために時間を止める
		Time.timeScale = 0.0f;

        // ★重要: 同様にここも削除
    }
}