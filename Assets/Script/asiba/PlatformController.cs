using UnityEngine;
using System.Collections;

public class PlatformController : MonoBehaviour
{
    [Header("設定")]
    public float respawnTime = 5.0f; // 壊れてから復活するまでの時間

    [Header("参照")]
    [Tooltip("物理的な当たり判定を持つコライダー（SolidGroundなど）")]
    public Collider physicalCollider;

    [Tooltip("見た目を担当するオブジェクト（複数可）")]
    public GameObject[] visualObjects;

    public bool IsBroken { get; private set; } = false;

    // Awakeは不要になったので削除

    public void BreakPlatform()
    {
        Debug.Log($"[PlatformSmashDebug] BreakPlatformメソッドが呼ばれました: {gameObject.name}");
        if (IsBroken)
        {
            Debug.LogWarning($"[PlatformSmashDebug] 足場「{gameObject.name}」は既に壊れているため処理をスキップ。");
            return;
        }
        IsBroken = true;
        StartCoroutine(BreakAndRespawnRoutine());
    }

    private IEnumerator BreakAndRespawnRoutine()
    {
        Debug.Log($"[PlatformSmashDebug] 足場「{gameObject.name}」を非表示にします。");

        // 1. 破壊演出（当たり判定と見た目を消す）
        if (physicalCollider != null) physicalCollider.enabled = false;
        foreach (var visual in visualObjects)
        {
            if (visual != null) visual.SetActive(false);
        }

        // 2. 指定時間待機
        yield return new WaitForSeconds(respawnTime);

        Debug.Log($"[PlatformSmashDebug] 足場「{gameObject.name}」を復活させます。");

        // 3. 復活演出（当たり判定と見た目を戻す）
        if (physicalCollider != null) physicalCollider.enabled = true;
        foreach (var visual in visualObjects)
        {
            if (visual != null) visual.SetActive(true);
        }

        IsBroken = false;
    }
}