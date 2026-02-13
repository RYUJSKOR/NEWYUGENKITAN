using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewDoubleArmPressAttack", menuName = "Boss Attacks/Double Arm Press Attack")]
public class DoubleArmPressAttack : BossAttackPattern
{
    [Header("両腕プレス攻撃の設定")]
    public float windUpSpeed = 25f;
    public float pressSpeed = 60f;
    public float returnSpeed = 20f;
    public float impactPauseTime = 1.0f;
    public float screenEdgeOffset = 2.0f;
    public float pressArmSpacing = 2.5f;

    [Header("溜め演出の設定")]
    [Tooltip("溜め時間。この時間だけプルプルします。")]
    public float chargeTime = 1.0f;
    [Tooltip("溜め中の腕の震えの『強さ』")]
    public float chargeShakeIntensity = 0.1f;
    [Tooltip("溜め中の腕の震えの『速さ』")]
    public float chargeShakeSpeed = 50f;

    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking || !boss.leftArmObject.activeSelf || !boss.rightArmObject.activeSelf) return;
        boss.RunAttackCoroutine(PressRoutine(boss));
    }

    private IEnumerator PressRoutine(BossController boss)
    {
        var SE = FindAnyObjectByType<SEController>();

        boss.SetBothArmsAttacking(true);

        Rigidbody leftArmRb = boss.leftArmObject.GetComponent<Rigidbody>();
        Rigidbody rightArmRb = boss.rightArmObject.GetComponent<Rigidbody>();
        Collider leftDamageCollider = boss.leftArmObject.transform.Find("DamageZone")?.GetComponent<Collider>();
        Collider rightDamageCollider = boss.rightArmObject.transform.Find("DamageZone")?.GetComponent<Collider>();

        try
        {
            // --- 1. 振りかぶり (Wind-up) ---
            boss.SetArmAnimation(true, prepareHandState);
            boss.SetArmAnimation(false, prepareHandState);

            Camera mainCamera = Camera.main;
            float distanceToPlayer = Vector3.Dot(boss.playerTransform.position - mainCamera.transform.position, mainCamera.transform.forward);
            Vector3 leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, distanceToPlayer));
            Vector3 rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, distanceToPlayer));

            Vector3 leftWindUpPos = new Vector3(leftEdge.x - screenEdgeOffset, boss.playerTransform.position.y, leftEdge.z);
            Vector3 rightWindUpPos = new Vector3(rightEdge.x + screenEdgeOffset, boss.playerTransform.position.y, rightEdge.z);

            float lockedAttackY = boss.playerTransform.position.y;
            float timeoutTimer = 0f; // ★ 無限ループ防止用のタイマー

            while ((Vector3.Distance(boss.leftArmObject.transform.position, leftWindUpPos) > 0.1f ||
                    Vector3.Distance(boss.rightArmObject.transform.position, rightWindUpPos) > 0.1f) &&
                    timeoutTimer < 3.0f)
            {
                lockedAttackY = boss.playerTransform.position.y;
                leftWindUpPos.y = lockedAttackY;
                rightWindUpPos.y = lockedAttackY;
                leftArmRb.MovePosition(Vector3.MoveTowards(boss.leftArmObject.transform.position, leftWindUpPos, windUpSpeed * Time.deltaTime));
                rightArmRb.MovePosition(Vector3.MoveTowards(boss.rightArmObject.transform.position, rightWindUpPos, windUpSpeed * Time.deltaTime));

                timeoutTimer += Time.deltaTime;
                yield return null;
            }

            // ★ 障害物に引っかかってもタイマーで抜けて必ず音が鳴る
            if (SE != null) SE.Play("Boss.Charge");

            // --- 2. 溜め（プルプル演出） ---
            float chargeTimer = 0f;
            Vector3 leftChargePos = boss.leftArmObject.transform.position;
            Vector3 rightChargePos = boss.rightArmObject.transform.position;

            while (chargeTimer < chargeTime)
            {
                float offsetX = Mathf.Sin(Time.time * chargeShakeSpeed) * chargeShakeIntensity;
                float offsetY = Mathf.Cos(Time.time * chargeShakeSpeed * 1.2f) * chargeShakeIntensity;

                leftArmRb.MovePosition(leftChargePos + new Vector3(offsetX, offsetY, 0));
                rightArmRb.MovePosition(rightChargePos + new Vector3(-offsetX, offsetY, 0));

                chargeTimer += Time.deltaTime;
                yield return null;
            }

            leftArmRb.MovePosition(leftChargePos);
            rightArmRb.MovePosition(rightChargePos);

            // --- 3. プレス ---
            boss.SetArmAnimation(true, actionHandState);
            boss.SetArmAnimation(false, actionHandState);
            if (leftDamageCollider != null) leftDamageCollider.enabled = true;
            if (rightDamageCollider != null) rightDamageCollider.enabled = true;

            Vector3 targetCenterPos = new Vector3(boss.playerTransform.position.x, lockedAttackY, boss.playerTransform.position.z);
            Vector3 leftTargetPos = targetCenterPos - boss.transform.right * (pressArmSpacing / 2);
            Vector3 rightTargetPos = targetCenterPos + boss.transform.right * (pressArmSpacing / 2);

            timeoutTimer = 0f; // ★ タイマーリセット
            while ((Vector3.Distance(boss.leftArmObject.transform.position, leftTargetPos) > 0.1f ||
                    Vector3.Distance(boss.rightArmObject.transform.position, rightTargetPos) > 0.1f) &&
                    timeoutTimer < 3.0f)
            {
                leftArmRb.MovePosition(Vector3.MoveTowards(boss.leftArmObject.transform.position, leftTargetPos, pressSpeed * Time.deltaTime));
                rightArmRb.MovePosition(Vector3.MoveTowards(boss.rightArmObject.transform.position, rightTargetPos, pressSpeed * Time.deltaTime));

                timeoutTimer += Time.deltaTime;
                yield return null;
            }

            // ★ ここも必ず鳴る
            if (SE != null) SE.Play("Boss.Press");

            // --- 4. 硬直 ---
            yield return new WaitForSeconds(impactPauseTime);

            // --- 5. 待機位置に戻る ---
            boss.SetArmAnimation(true, returnHandState);
            boss.SetArmAnimation(false, returnHandState);
            if (leftDamageCollider != null) leftDamageCollider.enabled = false;
            if (rightDamageCollider != null) rightDamageCollider.enabled = false;

            Transform leftRest = boss.leftArmRestPosition;
            Transform rightRest = boss.rightArmRestPosition;
            float returnDuration = Vector3.Distance(boss.leftArmObject.transform.position, leftRest.position) / returnSpeed;
            if (returnDuration < 0.1f) returnDuration = 0.1f;
            Vector3 leftStartPos = boss.leftArmObject.transform.position;
            Vector3 rightStartPos = boss.rightArmObject.transform.position;
            float returnTimer = 0f;

            while (returnTimer < returnDuration)
            {
                float ratio = returnTimer / returnDuration;
                leftArmRb.MovePosition(Vector3.Lerp(leftStartPos, leftRest.position, ratio));
                rightArmRb.MovePosition(Vector3.Lerp(rightStartPos, rightRest.position, ratio));
                returnTimer += Time.deltaTime;
                yield return null;
            }
            leftArmRb.MovePosition(leftRest.position);
            rightArmRb.MovePosition(rightRest.position);
        }
        finally
        {
            boss.SetArmAnimation(true, ArmAnimationState.Default);
            boss.SetArmAnimation(false, ArmAnimationState.Default);
            if (leftDamageCollider != null) leftDamageCollider.enabled = true;
            if (rightDamageCollider != null) rightDamageCollider.enabled = true;
            boss.SetBothArmsAttacking(false);
        }
    }
}