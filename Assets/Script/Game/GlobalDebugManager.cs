using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GlobalDebugManager : MonoBehaviour
{
    // 어디서든 접근 가능한 싱글톤 인스턴스
    public static GlobalDebugManager Instance { get; private set; }
    private GameData gameData;
    BossTimerManager bossTimerManager;
    private BulletSkill bulletskill;
    private PlayerStateMachine playerstate;

    public string titleSceneName = "TitleScene";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerstate = FindAnyObjectByType<PlayerStateMachine>();
        bossTimerManager = FindAnyObjectByType<BossTimerManager>();
        gameData = FindAnyObjectByType<GameData>();
        if (playerstate != null)
        {
            bulletskill = playerstate.GetStateByBaseClass<BulletSkill>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ReturnToTitle();
        }
    }

    public void ReturnToTitle()
    {
        if (GameData.Instance != null)
            GameData.Instance.ResetAll();

        if (bulletskill != null)
        {
            bulletskill.SetGauge(0f);
        }

        if (BossGameManager.Instance != null)
        {
            BossGameManager.Instance.ResetSavedData();
        }

        if (gameData != null && bossTimerManager != null)
        {
            gameData.saveBossTime(0.0f);
            bossTimerManager.ResetTimer(0.0f);
        }

        if (bossTimerManager != null)
        {
            bossTimerManager.StopTimer(); // 最終時間を保存
            Destroy(bossTimerManager.gameObject); // タイマーオブジェクトを削除
        }


        SceneManager.LoadScene("Title");
    }
}