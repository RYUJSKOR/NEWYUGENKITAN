using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;      // UIを操作するために追加
using System.Collections;  // コルーチンを使うために追加

[RequireComponent(typeof(VideoPlayer))]
public class SkipController : MonoBehaviour
{
    [SerializeField]
    private string titleSceneName = "Title";

    // --- フェード用の変数を追加 ---
    [Header("Fade Settings")]
    [SerializeField]
    private Image fadePanel; // フェードに使うImageをインスペクタから設定

    [SerializeField]
    private float fadeDuration = 1.0f; // フェードにかかる時間（秒）

    private VideoPlayer videoPlayer;
    private bool isTransitioning = false; // 多重実行を防ぐためのフラグ

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoEnd;

        // 開始時にフェードパネルが透明であることを保証する
        if (fadePanel != null)
        {
            fadePanel.color = new Color(fadePanel.color.r, fadePanel.color.g, fadePanel.color.b, 0);
            fadePanel.gameObject.SetActive(true); // パネルをアクティブにしておく
        }
        else
        {
            Debug.LogError("Fade Panelが設定されていません！", this);
        }
    }

    void Update()
    {
        // まだ遷移処理中でなく、何らかの入力があった場合
        if (!isTransitioning && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            StartTransition();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        StartTransition();
    }

    // 遷移処理を開始する
    private void StartTransition()
    {
        if (isTransitioning) return; // すでに処理が始まっていれば何もしない

        isTransitioning = true;
        StartCoroutine(FadeAndLoadScene());
    }

    // フェードアウトしてからシーンをロードするコルーチン
    private IEnumerator FadeAndLoadScene()
    {
        Debug.Log("フェードアウトを開始します。");

        // 動画の音を徐々に消す（オプション）
        // videoPlayer.SetDirectAudioVolume(0, 0); // 必要であれば

        float elapsedTime = 0f;
        Color color = fadePanel.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // 経過時間に基づいて透明度を0から1へ変更
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadePanel.color = color;
            yield return null; // 次のフレームまで待つ
        }

        // 完全に不透明にしてからシーンをロード
        color.a = 1f;
        fadePanel.color = color;

        SceneManager.LoadScene(titleSceneName);
    }
}