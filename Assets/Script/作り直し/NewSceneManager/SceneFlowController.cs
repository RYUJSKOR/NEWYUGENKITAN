using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneFlowController : MonoBehaviour
{
    public static SceneFlowController Instance { get; private set; }

    public string NextSceneName { get; private set; }

    // ★インスペクターで確認できるように [SerializeField] をつけ、実体の変数を用意
    [SerializeField]
    private string lastPlayedStageName;

    // 外部（RetryButtonなど）から読み取るためのプロパティ
    public string LastPlayedStageName
    {
        get { return lastPlayedStageName; }
        private set { lastPlayedStageName = value; }
    }

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // シーンが切り替わったことを検知するイベントを登録
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // 新しいシーンが読み込まれたら自動で呼ばれる
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ★Loadingシーンや、ゲームオーバー、タイトル画面などは「ステージ」として記憶しない
        // ※ご自身のプロジェクトの実際のシーン名に合わせて調整してください
        if (scene.name != "Loading" && scene.name != "Dead" && scene.name != "Title")
        {
            LastPlayedStageName = scene.name;
        }

        // Loadingシーン以外（＝目的のステージ）に着いた時だけフェードインする
        if (scene.name != "Loading")
        {
            StartCoroutine(FadeManager.Instance.FadeIn());
            isTransitioning = false; // 遷移完了
        }
    }

    public void RequestScene(string sceneName)
    {
        if (isTransitioning) return;
        NextSceneName = sceneName;
        StartCoroutine(TransitionFlow());
    }

    private IEnumerator TransitionFlow()
    {
        isTransitioning = true;

        // 1. 今のシーンで暗転
        yield return FadeManager.Instance.FadeOut();

        // 2. LoadingScene へ移動
        yield return SceneManager.LoadSceneAsync("Loading");

        // ※あとはAsyncSceneLoaderとOnSceneLoadedにおまかせ
    }
}