using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewPlatformSmashAttack", menuName = "Boss Attacks/Platform Smash Attack")]
public class PlatformSmashAttack : BossAttackPattern
{
    [Header("予兆エフェクト設定")]
    [Tooltip("攻撃対象の足場に表示するマーカーのプレハブ")]
    public GameObject targetMarkerPrefab;
    [Tooltip("マーカーが表示されてから腕が動き出すまでの時間")]
    public float warningDuration = 1.5f;
    [Tooltip("マーカー表示位置の微調整用オフセット")]
    public Vector3 markerOffset = new Vector3(0, 0.1f, 0);

    [Header("足場破壊攻撃の設定")]
    public int hitCount = 3;
    [Tooltip("叩きつけと次の叩きつけの間の時間")]
    public float timeBetweenHits = 0.5f;
    public LayerMask platformLayer;

    [Header("高さとタメ時間")]
    [Tooltip("叩きつける前に構える高さ")]
    public float hoverHeight = 4f;
    [Tooltip("上空で構えてから叩きつけを開始するまでの溜め時間")]
    public float chargeTime = 1.0f;

    [Header("★タメ（振動）設定")] // ▼▼▼ 追加 ▼▼▼
    [Tooltip("タメている間の揺れの大きさ")]
    public float shakeMagnitude = 0.5f;
    [Tooltip("タメている間の揺れの速さ")]
    public float shakeSpeed = 50f; // ▲▲▲ ▲▲▲

    [Header("動作速度の設定")]
    [Tooltip("足場の上まで移動する時の速度")]
    public float hoverMoveSpeed = 20f;
    [Tooltip("叩きつける時の振り下ろし速度")]
    public float smashSpeed = 100f;
    [Tooltip("待機位置に戻る時の速度")]
    public float returnSpeed = 30f;


    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking) return;
        boss.RunAttackCoroutine(AttackRoutine(boss));
    }

    private IEnumerator AttackRoutine(BossController boss)
    {
        bool useLeftArm = false;
        Rigidbody armRb = null;
        bool originalKinematicState = false;
        GameObject markerInstance = null;

        try
        {
            PlatformController targetPlatform = null;
            RaycastHit hit;
            if (Physics.Raycast(boss.playerTransform.position, Vector3.down, out hit, 50f, platformLayer))
            {
                targetPlatform = hit.collider.GetComponentInParent<PlatformController>();
            }

            if (targetPlatform == null)
            {
                yield break;
            }

            if (targetMarkerPrefab != null)
            {
                Vector3 markerPos = targetPlatform.transform.position + markerOffset;
                markerInstance = Instantiate(targetMarkerPrefab, markerPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(warningDuration);

            boss.SetAttackingState(true, false);

            useLeftArm = Vector3.Distance(boss.leftArmRestPosition.position, hit.point)
                            < Vector3.Distance(boss.rightArmRestPosition.position, hit.point);

            Transform arm = useLeftArm ? boss.leftArmObject.transform : boss.rightArmObject.transform;
            Transform restPosition = useLeftArm ? boss.leftArmRestPosition : boss.rightArmRestPosition;
            armRb = arm.GetComponent<Rigidbody>();

            originalKinematicState = armRb.isKinematic;
            armRb.isKinematic = true;

            boss.SetAttackingState(true, useLeftArm);

            Vector3 hitCenter = hit.collider.bounds.center;

            // --- 1. 腕を振り上げる ---
            boss.SetArmAnimation(useLeftArm, prepareHandState);
            Vector3 hoverPos = hitCenter + Vector3.up * hoverHeight;
            while (Vector3.Distance(arm.position, hoverPos) > 0.1f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, hoverPos, hoverMoveSpeed * Time.deltaTime));
                yield return null;
            }
            armRb.MovePosition(hoverPos); // ぴったり合わせる

            // ▼▼▼ ここからが「タメ（振動）」の処理 ▼▼▼
            float chargeTimer = 0f;
            while (chargeTimer < chargeTime)
            {
                // hoverPosを中心に、小刻みに揺らす
                float x = hoverPos.x + (Mathf.PerlinNoise(Time.time * shakeSpeed, 0) * 2 - 1) * shakeMagnitude;
                float y = hoverPos.y + (Mathf.PerlinNoise(0, Time.time * shakeSpeed) * 2 - 1) * shakeMagnitude;
                float z = hoverPos.z + (Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) * 2 - 1) * shakeMagnitude;

                armRb.MovePosition(new Vector3(x, y, z));

                chargeTimer += Time.deltaTime;
                yield return null;
            }
            // 揺れが終わったら、元の位置にきっちり戻す
            armRb.MovePosition(hoverPos);
            // ▲▲▲ ▲▲▲ ▲▲▲ ▲▲▲ ▲▲▲

            // --- 2. 複数回叩いて破壊する ---
            boss.SetArmAnimation(useLeftArm, actionHandState);

            Vector3 hitPos = hitCenter;
            for (int i = 0; i < hitCount; i++)
            {
                while (Vector3.Distance(arm.position, hitPos) > 0.1f)
                {
                    armRb.MovePosition(Vector3.MoveTowards(arm.position, hitPos, smashSpeed * Time.deltaTime));
                    yield return null;
                }
                while (Vector3.Distance(arm.position, hoverPos) > 0.1f)
                {
                    armRb.MovePosition(Vector3.MoveTowards(arm.position, hoverPos, hoverMoveSpeed * 0.8f * Time.deltaTime));
                    yield return null;
                }
                if (i < hitCount - 1)
                {
                    yield return new WaitForSeconds(timeBetweenHits);
                }
            }

            targetPlatform.BreakPlatform();

            yield return new WaitForSeconds(0.5f);
            boss.SetArmAnimation(useLeftArm, returnHandState);

            while (Vector3.Distance(arm.position, restPosition.position) > 0.1f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, restPosition.position, returnSpeed * Time.deltaTime));
                yield return null;
            }
            armRb.MovePosition(restPosition.position);
        }
        finally
        {
            if (armRb != null)
            {
                armRb.isKinematic = originalKinematicState;
            }

            if (markerInstance != null)
            {
                Destroy(markerInstance);
            }

            boss.SetArmAnimation(useLeftArm, ArmAnimationState.Default);
            boss.SetAttackingState(false, useLeftArm);
        }
    }
}