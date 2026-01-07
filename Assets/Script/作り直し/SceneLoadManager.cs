using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // Listを使うために必要
using System.Linq; // ListのContains(含むか)チェックを高速化するために必要

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }
    public string TargetSceneName { get; private set; }

    private AsyncOperation loadingSceneAsyncOp;

    // ===== ★ 修正点 1/3 ： 除外リストの変数を追加 =====
    [Header("事前ロード除外リスト")]
    [Tooltip("このリストに含まれるシーン名がロードされても、次のローディングシーンの事前準備を「実行しません」。(RealSceneで始まるシーンは自動で除外されます)")]
    [SerializeField]
    private List<string> preloadExclusionList = new List<string>
    {
        "Loading",
        "SceneSelecter",
        "Dead",
        "Title",
        "Result",
        "FinalResult",
        "Boss2Scene",
        "Boss3Scene"
    };
    // ===============================================

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
        StartCoroutine(DelayedPreload());
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private IEnumerator DelayedPreload()
    {
        // 1フレーム待機する
        yield return null;

        // その後、通常通り事前ロードを開始する
        StartCoroutine(PreloadLoadingScene());
    }

    // ===== ★ 修正点 2/3 ： OnSceneLoaded のロジックを変更 =====
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"<color=orange>--- 4. SceneLoadManager: OnSceneLoaded が検知されました。ロードされたシーン: {scene.name} ---</color>");

        // 除外リストにシーン名が含まれているか、または "RealScene" で始まるか確認
        if (preloadExclusionList.Contains(scene.name) || scene.name.StartsWith("RealScene"))
        {
            Debug.Log($"--- 4. SceneLoadManager: {scene.name} は対象外のため、PreloadLoadingScene を「実行しません」。---");
            return;
        }

        // 上記の条件以外（"SceneSelecter"など）の場合のみ、
        // 次のローディングに備えて、再度ローディングシーンを事前ロードする
        Debug.Log($"<color=red>--- 4. SceneLoadManager: {scene.name} は除外対象外のため、PreloadLoadingScene を「実行します！」。 ---</color>");
        StartCoroutine(PreloadLoadingScene());
    }
    // ===============================================


    private IEnumerator PreloadLoadingScene()
    {
        loadingSceneAsyncOp = SceneManager.LoadSceneAsync("Loading");
        loadingSceneAsyncOp.allowSceneActivation = false;
        yield return new WaitUntil(() => loadingSceneAsyncOp.progress >= 0.9f);
        Debug.Log("ローディングシーンの事前ロードが完了しました。");
    }


    public void LoadScene(string targetScene)
    {
        // Loading は中継地点なので、ここでは TargetSceneName を上書きしない
        if (targetScene != "Loading")
        {
            TargetSceneName = targetScene;
            Debug.Log($"[SceneLoadManager] Set TargetSceneName → {TargetSceneName}");
        }

        // Loading を経由する必要がある場合 Redirect
        if (targetScene != "Loading")
        {
            Debug.Log($"[SceneLoadManager] Redirect → Loading (Next: {TargetSceneName})");
            SceneNavigator.Instance.ChangeScene("Loading");
            return;
        }

        // ---- ここから Loading を実際に開く処理 ----
        if (loadingSceneAsyncOp != null)
        {
            loadingSceneAsyncOp.allowSceneActivation = true;
            return;
        }

        SceneManager.LoadScene("Loading");
    }
}