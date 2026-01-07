using UnityEngine;
using System.Collections;

public class FallEnemyMovement : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("上下に動く高さ")]
    [SerializeField] private float moveDistance = 3.0f;

    [Header("タイミング設定（秒数で指定）")]
    [Tooltip("上昇にかかる時間")]
    [SerializeField] private float moveUpDuration = 1.5f;
    [Tooltip("頂点で停止する時間")]
    [SerializeField] private float pauseAtTopDuration = 0.5f;
    [Tooltip("下降にかかる時間")]
    [SerializeField] private float moveDownDuration = 2.0f;
    [Tooltip("地面の下で隠れている（待機する）時間")]
    [SerializeField] private float pauseAtBottomDuration = 1.0f;

    // ★追加：回転設定
    [Header("回転設定")]
    [Tooltip("上下反転にかかる時間")]
    [SerializeField] private float rotationDuration = 0.25f;

    // --- 内部変数 ---
    private Vector3 startPosition;
    private Vector3 topPosition;
    private Quaternion initialRotation;
    private float currentSpeedModifier = 1.0f;

    public void SetSpeedModifier(float modifier)
    {
        currentSpeedModifier = modifier;
    }

    void Start()
    {
        startPosition = transform.position;
        topPosition = startPosition + new Vector3(0, moveDistance, 0);
        initialRotation = transform.rotation;
        StartCoroutine(MovementCycle());
    }

    private IEnumerator MovementCycle()
    {
        while (true)
        {
            // 地面に戻ったら、滑らかに元の向きに戻す
            yield return StartCoroutine(RotateOverTime(initialRotation, rotationDuration));

            if (pauseAtBottomDuration > 0)
            {
                yield return new WaitForSeconds(pauseAtBottomDuration / currentSpeedModifier);
            }

            yield return MoveBetween(startPosition, topPosition, moveUpDuration);

            if (pauseAtTopDuration > 0)
            {
                yield return new WaitForSeconds(pauseAtTopDuration / currentSpeedModifier);
            }

            // 頂点に達したら、滑らかに逆さまにする
            Quaternion targetFlipRotation = initialRotation * Quaternion.Euler(0, 0, 180);
            yield return StartCoroutine(RotateOverTime(targetFlipRotation, rotationDuration));

            yield return MoveBetween(topPosition, startPosition, moveDownDuration);
        }
    }

    private IEnumerator MoveBetween(Vector3 from, Vector3 to, float duration)
    {
        float actualDuration = duration / currentSpeedModifier;

        if (actualDuration <= 0)
        {
            transform.position = to;
            yield break;
        }

        float timer = 0f;
        while (timer < actualDuration)
        {
            float progress = timer / actualDuration;
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            transform.position = Vector3.Lerp(from, to, easedProgress);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
    }

    // ★追加：指定時間をかけて滑らかに回転させるコルーチン
    private IEnumerator RotateOverTime(Quaternion targetRotation, float duration)
    {
        // 速度倍率を回転時間にも適用する
        float actualDuration = duration / currentSpeedModifier;

        if (actualDuration <= 0)
        {
            transform.rotation = targetRotation;
            yield break;
        }

        Quaternion startRotation = transform.rotation;
        float timer = 0f;

        while (timer < actualDuration)
        {
            // Slerpで球面線形補間を行い、滑らかな回転を実現
            float progress = timer / actualDuration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);

            timer += Time.deltaTime;
            yield return null;
        }

        // 最後にきっちり目標の角度に設定する
        transform.rotation = targetRotation;
    }
}