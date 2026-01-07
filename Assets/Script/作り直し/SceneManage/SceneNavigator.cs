using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneNavigator : MonoBehaviour
{
    public static SceneNavigator Instance { get; private set; }

    [Header("演出用コントローラー")]
    [SerializeField] private FadeController fadeController;

    [Header("設定")]
    [SerializeField] private float defaultFadeDuration = 1.0f;

    public string NextSceneName { get; private set; }

    private bool isTransitioning = false;
    // private string lastSceneName; // 未使用なら削除可

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        Debug.Log($"? Now Scene: {SceneManager.GetActiveScene().name}");
    }


    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Loading")
        {
            Debug.Log("SceneNavigator: LoadingではFadeInしません");
            return;
        }

        // シーンロード完了時は時間を確実に動かす
        Time.timeScale = 1.0f;

        // シーンが始まったら「フェードイン（画面を明るくする）」を行う
        if (fadeController != null)
        {
            StartCoroutine(fadeController.FadeIn(defaultFadeDuration));
        }
    }

    // --- メソッド群 ---

    public void ChangeScene(string sceneName)
    {
        ChangeScene(sceneName, defaultFadeDuration);
    }

    public void ChangeScene(string sceneName, float duration)
    {
        if (isTransitioning)
        {
            // Loading へ誘導する遷移だけは無視しない
            if (sceneName != "Loading")
                return;
        }

        // 遷移処理が始まったら、強制的に時間を動かす
        Time.timeScale = 1.0f;

        // lastSceneName = SceneManager.GetActiveScene().name;
        NextSceneName = sceneName;
        StartCoroutine(ProcessSceneTransition(sceneName, duration));
    }

    // Bool対応版
    public void ChangeScene(string sceneName, bool useFade)
    {
        float duration = useFade ? defaultFadeDuration : 0f;
        ChangeScene(sceneName, duration);
    }

    public void RetryLastStage()
    {
        string targetScene = SceneManager.GetActiveScene().name;
        ChangeScene(targetScene);
    }

    // 手動で暗転させたい場合
    public Coroutine FadeOut()
    {
        if (fadeController != null)
        {
            return StartCoroutine(fadeController.FadeOut(defaultFadeDuration));
        }
        return null;
    }

    // --- 内部処理 ---

    private IEnumerator ProcessSceneTransition(string sceneName, float duration)
    {
        isTransitioning = true;

        if (fadeController != null && duration > 0)
        {
            // 次のシーンへ行くために「フェードアウト（画面を暗くする）」を行う
            yield return StartCoroutine(fadeController.FadeOut(duration));
        }
        else
        {
            yield return null;
        }

        Debug.Log($"[SceneNavigator] Request → {sceneName} (via SceneLoadManager)");

        SceneLoadManager.Instance.LoadScene(sceneName);

        isTransitioning = false;
    }
}