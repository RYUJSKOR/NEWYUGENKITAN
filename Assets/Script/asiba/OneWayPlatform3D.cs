using System.Collections;
using UnityEngine;

/// <summary>
/// 【最終版】一方通行足場の「下からの自動すり抜け」専門スクリプト。
/// トリガーコライダーを持つオブジェクトにアタッチしてください。
/// </summary>
public class OneWayPlatform3D : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Collider platformCollider;
    [SerializeField] private float targetLiftSpeed = 12f;
    [SerializeField] private float smoothFactor = 6f;
    [SerializeField] private float liftOffset = 0.1f;
    [SerializeField] private float reenableDelay = 0.05f;

    [Tooltip("上昇判定の最小速度（小ジャンプ対応）")]
    [SerializeField] private float upwardThreshold = -1.0f;

    private bool isIgnoring = false;
    private Coroutine climbRoutine;
    public bool IsManuallyControlled { get; set; }

    private void OnTriggerStay(Collider other)
    {
        if (IsManuallyControlled) return;
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        bool isMovingUp = rb.linearVelocity.y > upwardThreshold;
        bool isBelowPlatform = other.bounds.min.y < platformCollider.bounds.max.y - 0.05f;

        // 下から上昇中のみコリジョン無効化
        if (isMovingUp && isBelowPlatform && !isIgnoring)
        {
            Physics.IgnoreCollision(other, platformCollider, true);
            isIgnoring = true;

            if (climbRoutine != null)
                StopCoroutine(climbRoutine);

            climbRoutine = StartCoroutine(ClimbRoutine(other, rb));
        }
    }

    private IEnumerator ClimbRoutine(Collider player, Rigidbody rb)
    {
        float platformCenterY = platformCollider.bounds.center.y;
        float platformTop = platformCollider.bounds.max.y;
        float playerHalf = player.bounds.extents.y;

        // --- 半分到達まで待機 ---
        while (player.bounds.center.y < platformCenterY)
        {
            // 落下中なら中断せず待機継続（安全）
            yield return null;
        }

        // --- 押し上げフェーズ ---
        float targetY = platformTop + playerHalf + liftOffset;

        float startVelY = Mathf.Max(rb.linearVelocity.y, 0f);
        float elapsed = 0f;

        // 一度開始したら中断しない（条件削除）
        while (player.transform.position.y < targetY)
        {
            elapsed += Time.deltaTime * smoothFactor;
            float newVelY = Mathf.Lerp(startVelY, targetLiftSpeed, elapsed);

            // 滑らかに上昇
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, newVelY, rb.linearVelocity.z);
            player.transform.position += Vector3.up * newVelY * Time.deltaTime;

            yield return null;
        }

        // --- 押し上げ完了 ---
        Vector3 pos = player.transform.position;
        pos.y = Mathf.Max(pos.y, targetY);
        player.transform.position = pos;

        // 着地速度を自然減衰
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, 1.5f), rb.linearVelocity.z);

        yield return new WaitForSeconds(reenableDelay);
        ResetCollision(player);
    }

    private void ResetCollision(Collider player)
    {
        Physics.IgnoreCollision(player, platformCollider, false);
        isIgnoring = false;
        climbRoutine = null;
    }

    private void OnTriggerExit(Collider other)
    {
        // climbRoutine実行中ならリセットしない（押し上げ中は無効化維持）
        if (isIgnoring && climbRoutine == null)
            ResetCollision(other);
    }
}