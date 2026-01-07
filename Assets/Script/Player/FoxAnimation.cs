using System.Collections.Generic;
using UnityEngine;

public class FoxAnimation : MonoBehaviour
{
    [SerializeField] private GameObject tailPrefab;
    [SerializeField] private float growSpeed = 5f;
    [SerializeField] private int tailCount = 3;

    [Header("Layout")]
    [SerializeField] private Vector3 rootOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private float totalAngle = 60f;   // 扇の広がり
    [SerializeField] private float pitch = 90f;        // しっぽを上向きにしたい等あれば

    private readonly List<GameObject> tails = new();
    private readonly List<Vector3> targetScales = new();

    // 現在の（再配置後の）レイアウト
    private readonly List<Vector3> tailOffsets = new();
    private readonly List<Quaternion> tailRotations = new();

    private Transform parent;
    private Vector3 prefabOriginalScale;
    [Tooltip("揺れる速さ（波の周波数）")]
    [SerializeField] private float swaySpeed = 3f;
    [Tooltip("揺れる角度の最大値（±）")]
    [SerializeField] private float swayAngle = 20f;
    private Vector3 lastPlayerVelocity;
    private Vector3 tailInertiaOffset = Vector3.zero;
    [SerializeField] private float inertiaStrength = 0.1f; // 慣性にどれだけ反応するか

    public void Init(Transform parentTransform)
    {
        ClearTails();
        parent = parentTransform;

        prefabOriginalScale = tailPrefab.transform.localScale;

        if (GetComponentInParent<Player>() != null && transform == GetComponentInParent<Player>().transform) { enabled = false; }

        // 初期本数分生成
        GenerateTails(tailCount);
        // 初回レイアウト
        ReLayout();
    }

    public void UpdateTailGrowth(Vector3 playerVelocity)
    {
        for (int i = 0; i < tails.Count; i++)
        {
            var tail = tails[i];
            if (tail == null) continue;

            tail.transform.localScale = Vector3.Lerp(
                tail.transform.localScale,
                targetScales[i],
                Time.deltaTime * growSpeed
            );

            // 慣性によるしなり（Y速度反映）
            float velocityY = Mathf.Clamp(playerVelocity.y, -10f, 10f);
            float inertiaPitch = velocityY * 3f; 

            Quaternion inertiaRotation = Quaternion.Euler(inertiaPitch, 0f, 0f);

            // 波のような揺れ（sway）
            float swayPhase = Time.time * swaySpeed + i * 0.5f;
            float swayZ = Mathf.Sin(swayPhase) * swayAngle;
            Quaternion swayRotation = Quaternion.Euler(0f, 0f, swayZ);

            // 合成回転
            Quaternion baseRotation = tailRotations[i];
            Quaternion targetRotation = baseRotation * inertiaRotation * swayRotation;

            // なめらか補間
            tail.transform.localRotation = Quaternion.Slerp(tail.transform.localRotation, targetRotation, Time.deltaTime * 10f);

            // 位置
            tail.transform.localPosition = tailOffsets[i];
        }
    }

    /// <summary>
    /// 末尾（配列の最後）の尻尾を1本消し、残りを左右対称に並び直す
    /// </summary>
    public void RemoveLastTail()
    {
        if (tails.Count == 0) return;

        var last = tails[^1];
        if (last != null) Destroy(last);

        tails.RemoveAt(tails.Count - 1);
        targetScales.RemoveAt(targetScales.Count - 1);

        // 並び直す
        ReLayout();
    }

    public void DestroyAllTails()
    {
        foreach (var tail in tails)
        {
            if (tail != null) Destroy(tail);
        }

        tails.Clear();
        targetScales.Clear();
        tailOffsets.Clear();
        tailRotations.Clear();
    }

    private void ClearTails() => DestroyAllTails();

    /// <summary>
    /// 現在残っている本数で、角度・配置を再計算して再配置
    /// </summary>
    private void ReLayout()
    {
        GenerateTailLayout(tails.Count, totalAngle, rootOffset, pitch, tailOffsets, tailRotations);

        for (int i = 0; i < tails.Count; i++)
        {
            var tail = tails[i];
            if (tail == null) continue;

            tail.transform.localPosition = tailOffsets[i];
            tail.transform.localRotation = tailRotations[i];
        }
    }

    /// <summary>
    /// 指定本数の尻尾を生成（生成だけ。配置は ReLayout に任せる）
    /// </summary>
    private void GenerateTails(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var tail = Instantiate(tailPrefab, parent);
            tail.transform.localScale = Vector3.zero; // 生える演出
            tails.Add(tail);
            targetScales.Add(prefabOriginalScale);
        }
    }

    /// <summary>
    /// 本数に応じた扇状レイアウト（offset と rotation）を求める
    /// </summary>
    private static void GenerateTailLayout(int count, float totalAngle, Vector3 rootOffset, float pitchDeg, List<Vector3> outOffsets, List<Quaternion> outRotations)
    {
        outOffsets.Clear();
        outRotations.Clear();

        if (count <= 0) return;

        // 1本だけなら中央（yaw=0）に固定
        if (count == 1)
        {
            outOffsets.Add(rootOffset);
            outRotations.Add(Quaternion.Euler(pitchDeg, 0f, 0f));
            return;
        }

        float startAngle = -totalAngle * 0.5f;
        float step = totalAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float yaw = startAngle + step * i;   // 左右対称
            Quaternion rot = Quaternion.Euler(pitchDeg, yaw, 0f);

            outOffsets.Add(rootOffset);
            outRotations.Add(rot);
        }
    }
}
