using UnityEngine;
using System;
using System.Collections;

public class SkillCutInManager : MonoBehaviour
{
    [Header("フェード演出設定")]
    [SerializeField] private CanvasGroup darkMask;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float holdTime = 0.3f;

    [Header("スローモーション設定")]
    [SerializeField, Range(0.001f, 1f)]
    private float slowTimeScale = 0.002f;

    public bool IsPlaying { get; private set; } = false;

    private float originalTimeScale;

    /// <summary>
    /// 外部（BulletSkill）から呼ぶメインAPI
    /// </summary>
    public void PlaySkillCutIn(Action onSkillExecute)
    {
        if (IsPlaying) return;

        StartCoroutine(CutInSequence(onSkillExecute));
    }

    /// <summary>
    /// カットインの一連の流れ
    /// </summary>
    private IEnumerator CutInSequence(Action onSkillExecute)
    {
        IsPlaying = true;

        originalTimeScale = Time.timeScale;

        // ① スロー開始
        SetSlowMotion(true);

        // ② フェードイン
        yield return FadeIn();

        // ③ スキル実行（暗転中）
        onSkillExecute?.Invoke();

        // ④ 暗転キープ
        yield return HoldDark();

        // ⑤ フェードアウト
        yield return FadeOut();

        // ⑥ スロー解除
        SetSlowMotion(false);

        IsPlaying = false;
    }

    // ============================
    // フェード処理群
    // ============================

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            darkMask.alpha = Mathf.Lerp(0f, 0.7f, t / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator HoldDark()
    {
        yield return new WaitForSecondsRealtime(holdTime);
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            darkMask.alpha = Mathf.Lerp(0.7f, 0f, t / fadeDuration);
            yield return null;
        }
    }

    // ============================
    // スローモーション管理
    // ============================

    private void SetSlowMotion(bool enabled)
    {
        if (enabled)
        {
            Time.timeScale = slowTimeScale;
        }
        else
        {
            Time.timeScale = originalTimeScale;
        }

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}
