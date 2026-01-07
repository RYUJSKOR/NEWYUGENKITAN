using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class BossGameManager : MonoBehaviour
{
    public static BossGameManager Instance { get; private set; }

    [Header("参照")]
    public FadeManager bossFadeManager;

    [Header("管理対象シーン")]
    public List<string> bossSceneNames = new List<string>();

    [Header("ステージクリア特典")]
    [SerializeField] private float recoveryAmountOnStageClear = 20f;

    // --- 登録情報 ---
    public Player CurrentPlayer { get; private set; }
    public BossController CurrentBoss { get; private set; }

    // --- シーン間引き継ぎデータ ---
    public float SavedPlayerHealth { get; private set; }
    public float SavedBossHealth { get; private set; }
    public int SavedBossPhase { get; private set; }

    public bool HasSavedData { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (bossFadeManager == null)
        {
            var found = FindFirstObjectByType<FadeManager>();
            if (found != null) bossFadeManager = found;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (bossSceneNames.Contains(scene.name))
        {
            RestorePlayerData();

            if (bossFadeManager != null)
            {
                // ボス戦開始：波フェードイン（下から上へ透明になる）
                StartCoroutine(bossFadeManager.FadeInBoss());
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 遷移処理 ---

    public void GoToNextStage(string nextSceneName)
    {
        SaveCurrentState();

        if (bossFadeManager != null)
        {
            // ボス演出（波フェード）がある場合は、独自のコルーチンで遷移
            StartCoroutine(ProcessBossTransition(nextSceneName));
        }
        else
        {
            // ★修正：演出がない場合の保険として、新しいシステム（SceneFlowController）を使用
            if (SceneFlowController.Instance != null)
            {
                SceneFlowController.Instance.RequestScene(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    // ボス戦独自の波フェード遷移
    // ※SceneFlowControllerを使うと「通常の黒フェード＆ロード画面」になってしまい、
    //   波演出が上書きされてしまうため、ここだけは直接ロードを行います。
    private IEnumerator ProcessBossTransition(string sceneName)
    {
        // 1. 波フェードアウト（下から上へ黒くなる）
        yield return bossFadeManager.FadeOutBoss();

        // 2. 直接シーン切り替え（ロード画面は挟まない＝演出維持）
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// リザルト画面などへ遷移
    /// </summary>
    public void GoToResult(string resultSceneName)
    {
        ResetSavedData();

        // ★修正：ここも SceneNavigator ではなく SceneFlowController を使用
        if (SceneFlowController.Instance != null)
        {
            SceneFlowController.Instance.RequestScene(resultSceneName);
        }
        else
        {
            SceneManager.LoadScene(resultSceneName);
        }
    }

    // --- データ保存・復元 ---

    // (ここから下は変更なし)

    private void SaveCurrentState()
    {
        HasSavedData = true;

        if (CurrentPlayer != null)
        {
            var playerHealth = CurrentPlayer.GetComponent<CharacterHealthManager>();
            if (playerHealth != null)
            {
                SavedPlayerHealth = playerHealth.GetHealth();
            }
        }

        if (CurrentBoss != null)
        {
            SavedBossHealth = CurrentBoss.GetBodyHealth();
            SavedBossPhase = CurrentBoss.GetCurrentPhase();
        }
    }

    private void RestorePlayerData()
    {
        Player newPlayer = FindFirstObjectByType<Player>();

        if (newPlayer != null)
        {
            RegisterPlayer(newPlayer);

            var playerHealthManager = newPlayer.GetComponent<CharacterHealthManager>();
            if (playerHealthManager != null && HasSavedData)
            {
                playerHealthManager.SetHealth(SavedPlayerHealth);
                playerHealthManager.Recovery(recoveryAmountOnStageClear);
                HasSavedData = false;
            }
        }
    }

    public void ResetSavedData()
    {
        HasSavedData = false;
        SavedPlayerHealth = 0;
        SavedBossHealth = 0;
        SavedBossPhase = 0;
    }

    public void RegisterPlayer(Player player) => CurrentPlayer = player;
    public void RegisterBoss(BossController boss) => CurrentBoss = boss;
    public void UnregisterBoss() => CurrentBoss = null;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}