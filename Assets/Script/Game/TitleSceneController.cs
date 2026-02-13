using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class TitleSceneController : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string videoSceneName = "VideoScene";
    [SerializeField] private float idleTimeToReturn = 5f;

    [Header("Fade Settings")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private float fadeDuration = 1f;

    private float idleTimer = 0f;
    private bool isTransitioning = false;

    void Start()
    {
        // 開始時は透明にしておく
        if (fadePanel != null)
        {
            Color c = fadePanel.color;
            c.a = 0f;
            fadePanel.color = c;
            fadePanel.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (isTransitioning) return;

        // 入力があればタイマーリセット
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            idleTimer = 0f;
        }
        else
        {
            idleTimer += Time.unscaledDeltaTime; // TimeScale無視

            if (idleTimer >= idleTimeToReturn)
            {
                StartCoroutine(FadeAndReturn());
            }
        }
    }

    private IEnumerator FadeAndReturn()
    {
        isTransitioning = true;

        float elapsed = 0f;
        Color c = fadePanel.color;

        // フェードアウト（透明 → 黒）
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadePanel.color = c;
            yield return null;
        }

        c.a = 1f;
        fadePanel.color = c;

        SceneManager.LoadScene(videoSceneName);
    }
}
