using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    [Header("Ref")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image fadeImage;

    [Header("Boss Settings")]
    [SerializeField] private Material bossTransitionMaterial;
    [SerializeField] private float bossFadeDuration = 1.5f;

    // 波の高さがある分、1.0より少し大きく動かす必要がある
    private const float MaxCutoff = 1.3f;

    [Header("Normal Settings")]
    [SerializeField] private float defaultDuration = 0.5f;

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
        if (fadeImage != null) fadeImage.material = null;
        if (fadeCanvasGroup != null && fadeCanvasGroup.alpha > 0.9f) StartCoroutine(FadeIn());
    }

    // --- Boss Fade Logic ---

    // 暗転（下から上へ黒くなる）
    public IEnumerator FadeOutBoss()
    {
        if (fadeImage == null || bossTransitionMaterial == null) yield break;

        fadeImage.material = bossTransitionMaterial;
        fadeCanvasGroup.alpha = 1f;
        fadeCanvasGroup.blocksRaycasts = true;

        // ★モード設定：下から黒くする
        bossTransitionMaterial.SetFloat("_Inverse", 0f);

        float timer = 0f;
        bossTransitionMaterial.SetFloat("_Cutoff", 0f);

        while (timer < bossFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            // 0 -> 1.3 へ増やす（下から黒が上がってくる）
            float val = Mathf.Lerp(0f, MaxCutoff, timer / bossFadeDuration);
            bossTransitionMaterial.SetFloat("_Cutoff", val);
            yield return null;
        }
        bossTransitionMaterial.SetFloat("_Cutoff", MaxCutoff);
    }

    // 明転（下から上へ黒が消えていく）
    public IEnumerator FadeInBoss()
    {
        if (fadeImage == null || bossTransitionMaterial == null) yield break;

        fadeImage.material = bossTransitionMaterial;
        fadeCanvasGroup.alpha = 1f;

        // ★モード設定：下から透明にする（＝黒い部分が上に逃げる）
        bossTransitionMaterial.SetFloat("_Inverse", 1f);

        float timer = 0f;
        // 0 -> 1.3 へ増やす（透明エリアが下から上がってくる）
        bossTransitionMaterial.SetFloat("_Cutoff", 0f);

        while (timer < bossFadeDuration)
        {
            timer += Time.unscaledDeltaTime;
            float val = Mathf.Lerp(0f, MaxCutoff, timer / bossFadeDuration);
            bossTransitionMaterial.SetFloat("_Cutoff", val);
            yield return null;
        }
        bossTransitionMaterial.SetFloat("_Cutoff", MaxCutoff);

        // 終了処理
        fadeImage.material = null;
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    // --- Normal Fade (変更なし) ---
    public IEnumerator FadeOut(float duration = -1f)
    {
        if (fadeImage != null) fadeImage.material = null;
        yield return FadeProcess(0f, 1f, duration < 0 ? defaultDuration : duration);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        if (fadeImage != null) fadeImage.material = null;
        yield return FadeProcess(1f, 0f, duration < 0 ? defaultDuration : duration);
    }

    private IEnumerator FadeProcess(float start, float end, float time)
    {
        if (fadeCanvasGroup == null) yield break;
        fadeCanvasGroup.blocksRaycasts = true;
        float timer = 0f;
        fadeCanvasGroup.alpha = start;
        while (timer < time)
        {
            timer += Time.unscaledDeltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(start, end, timer / time);
            yield return null;
        }
        fadeCanvasGroup.alpha = end;
        if (end == 0f) fadeCanvasGroup.blocksRaycasts = false;
    }
}