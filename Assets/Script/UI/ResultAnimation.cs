using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultSequenceManager : MonoBehaviour
{
    [Header("1. Clear Time Settings")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private float countUpDuration = 1.5f;
    [SerializeField] private float timeScaleDuration = 0.5f;

    [Header("2. HP Bonus Settings")]
    [SerializeField] private Transform hpSpawnPoint;
    [SerializeField] private List<GameObject> hpPrefabs;
    [SerializeField] private GameObject hpDenominatorObject; // 分母オブジェクト（/3）
    [SerializeField] private float hpAnimationDuration = 1.0f;
    [SerializeField] private float hpScaleDuration = 0.5f;

    [Header("3. Rank Settings (Stamp)")]
    [SerializeField] private Transform rankSpawnPoint;
    [SerializeField] private List<GameObject> rankPrefabs;
    [SerializeField] private float rankTiltAngle = 15f; // ハンコの傾き最大角度

    [Header("4. Next Button Settings")]
    [SerializeField] private GameObject nextButton;

    [Header("General Settings")]
    [SerializeField] private float intervalBetweenSteps = 0.5f;

    // 内部変数
    private string finalTimeStr = "01:00";
    private int finalHpIndex = 0;
    private int finalRankIndex = 0;

    private GameObject currentHpObject;
    private GameObject currentRankObject;

    private void OnEnable()
    {
        ClearOldObjects();

        if (nextButton != null) nextButton.SetActive(false);
        if (hpDenominatorObject != null) hpDenominatorObject.SetActive(false);

        if (timeText != null)
        {
            timeText.gameObject.SetActive(false);
            timeText.text = "00:00";
        }
        StartCoroutine(PlayResultSequence());
    }

    private void OnDisable()
    {
        ClearOldObjects();
    }

    public void SetResultData(string timeString, int hpIndex, int rankIndex)
    {
        finalTimeStr = string.IsNullOrEmpty(timeString) ? "00:00" : timeString;
        finalHpIndex = (hpIndex < 0) ? 0 : hpIndex;
        finalRankIndex = (rankIndex < 0) ? 0 : rankIndex;
    }

    private IEnumerator PlayResultSequence()
    {
        yield return new WaitForSeconds(0.2f);

        // 1. クリアタイム
        if (timeText != null)
        {
            timeText.gameObject.SetActive(true);
            float targetSeconds = ParseTimeToSeconds(finalTimeStr);
            float elapsed = 0f;
            while (elapsed < countUpDuration)
            {
                elapsed += Time.deltaTime;
                float currentSeconds = Mathf.Lerp(0f, targetSeconds, elapsed / countUpDuration);
                timeText.text = FormatSecondsToTime(currentSeconds);
                yield return null;
            }
            timeText.text = finalTimeStr;
            yield return StartCoroutine(AnimateScale(timeText.transform, timeScaleDuration));
        }

        yield return new WaitForSeconds(intervalBetweenSteps);

        // 2. 体力ボーナス (分母も一緒にドン！)
        if (hpSpawnPoint != null && hpPrefabs != null && hpPrefabs.Count > 0)
        {
            int index = Mathf.Clamp(finalHpIndex, 0, hpPrefabs.Count - 1);
            currentHpObject = Instantiate(hpPrefabs[index], hpSpawnPoint.position, hpSpawnPoint.rotation, hpSpawnPoint);

            // 分母を表示（まだサイズ変更はしない）
            if (hpDenominatorObject != null) hpDenominatorObject.SetActive(true);

            // 数字のアニメが終わるのを待つ
            if (hpAnimationDuration > 0f) yield return new WaitForSeconds(hpAnimationDuration);

            // ★ここ！分母を並列でドン！
            if (hpDenominatorObject != null)
            {
                StartCoroutine(AnimateScale(hpDenominatorObject.transform, hpScaleDuration));
            }

            // 数字本体をドン！
            yield return StartCoroutine(AnimateScale(currentHpObject.transform, hpScaleDuration));
        }

        yield return new WaitForSeconds(intervalBetweenSteps);

        // 3. ランク (ハンコ演出：傾き + ドン！)
        if (rankSpawnPoint != null && rankPrefabs != null && rankPrefabs.Count > 0)
        {
            int index = Mathf.Clamp(finalRankIndex, 0, rankPrefabs.Count - 1);
            currentRankObject = Instantiate(rankPrefabs[index], rankSpawnPoint.position, rankSpawnPoint.rotation, rankSpawnPoint);

            // ★ここ！ランダムに傾きをつける
            float randomZ = Random.Range(-rankTiltAngle, rankTiltAngle);
            currentRankObject.transform.localRotation = Quaternion.Euler(0, 0, randomZ);

            // 拡大演出（ドン！）
            yield return StartCoroutine(AnimateScale(currentRankObject.transform, timeScaleDuration));
        }

        yield return new WaitForSeconds(intervalBetweenSteps);

        // 4. ボタン表示
        if (nextButton != null)
        {
            nextButton.SetActive(true);
            yield return StartCoroutine(AnimateScale(nextButton.transform, timeScaleDuration));
        }
    }

    // 膨らんで戻る演出
    private IEnumerator AnimateScale(Transform target, float duration)
    {
        Vector3 originalScale = target.localScale;
        if (originalScale == Vector3.zero) originalScale = Vector3.one;

        Vector3 maxScale = originalScale * 1.5f;

        float expandDuration = duration * 0.2f;
        float shrinkDuration = duration * 0.8f;
        float elapsed = 0f;

        while (elapsed < expandDuration)
        {
            target.localScale = Vector3.Lerp(originalScale, maxScale, elapsed / expandDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localScale = maxScale;

        elapsed = 0f;
        while (elapsed < shrinkDuration)
        {
            target.localScale = Vector3.Lerp(maxScale, originalScale, elapsed / shrinkDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localScale = originalScale;
    }

    private float ParseTimeToSeconds(string timeStr)
    {
        string[] parts = timeStr.Split(':');
        if (parts.Length == 2 && float.TryParse(parts[0], out float min) && float.TryParse(parts[1], out float sec))
            return min * 60f + sec;
        return 0f;
    }

    private string FormatSecondsToTime(float totalSeconds)
    {
        int min = Mathf.FloorToInt(totalSeconds / 60f);
        int sec = Mathf.FloorToInt(totalSeconds % 60f);
        return $"{min:D2}:{sec:D2}";
    }

    private void ClearOldObjects()
    {
        if (currentHpObject != null) Destroy(currentHpObject);
        if (currentRankObject != null) Destroy(currentRankObject);
    }
}