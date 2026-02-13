using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewSlapAttack", menuName = "Boss Attacks/Slap Attack")]
public class ArmSlapAttack : BossAttackPattern
{
    [Header("ビンタ攻撃の設定")]
    public float attackHeightOffset = 1.0f;
    public float screenEdgeOffset = 2.0f;
    public float swipeSpeed = 40f;
    public float returnSpeed = 20f;
    public float windUpTime = 0.8f;

    [Header("溜めの演出設定")]
    public bool enableChargeShake = true;
    public float shakeAmount = 0.1f;
    public float shakeSpeed = 30f;

    private bool isLeftArmAttackingNext = true;

    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking) return;

        Transform armToAttack = isLeftArmAttackingNext ? boss.leftArmObject.transform : boss.rightArmObject.transform;
        Transform restPosition = isLeftArmAttackingNext ? boss.leftArmRestPosition : boss.rightArmRestPosition;

        if (armToAttack.gameObject.activeSelf)
        {
            boss.RunAttackCoroutine(SlapRoutine(boss, armToAttack, restPosition, isLeftArmAttackingNext));
            isLeftArmAttackingNext = !isLeftArmAttackingNext;
        }
        else
        {
            armToAttack = !isLeftArmAttackingNext ? boss.leftArmObject.transform : boss.rightArmObject.transform;
            restPosition = !isLeftArmAttackingNext ? boss.leftArmRestPosition : boss.rightArmRestPosition;
            if (armToAttack.gameObject.activeSelf)
            {
                boss.RunAttackCoroutine(SlapRoutine(boss, armToAttack, restPosition, !isLeftArmAttackingNext));
                isLeftArmAttackingNext = !isLeftArmAttackingNext;
            }
        }
    }

    private IEnumerator SlapRoutine(BossController boss, Transform arm, Transform restPosition, bool isLeftArm)
    {
        var SE = FindAnyObjectByType<SEController>();

        boss.SetAttackingState(true, isLeftArm);
        Rigidbody armRb = arm.GetComponent<Rigidbody>();
        Transform damageZoneTransform = arm.Find("DamageZone");
        Collider damageCollider = (damageZoneTransform != null) ? damageZoneTransform.GetComponent<Collider>() : null;

        try
        {
            // --- 攻撃位置の計算 ---
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Cameraが見つかりません！");
                yield break;
            }

            float distanceToPlayer = Vector3.Dot(boss.playerTransform.position - mainCamera.transform.position, mainCamera.transform.forward);
            Vector3 leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, distanceToPlayer));
            Vector3 rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, distanceToPlayer));

            Vector3 startPos, endPos;
            if (isLeftArm)
            {
                startPos = leftEdge - boss.transform.right * screenEdgeOffset;
                endPos = rightEdge + boss.transform.right * screenEdgeOffset;
            }
            else
            {
                startPos = rightEdge + boss.transform.right * screenEdgeOffset;
                endPos = leftEdge - boss.transform.right * screenEdgeOffset;
            }
            startPos.y = endPos.y = boss.playerTransform.position.y + attackHeightOffset;


            // --- 振りかぶり (Wind-up) ---
            boss.SetArmAnimation(isLeftArm, prepareHandState);
            float timeoutTimer = 0f; // ★ 無限ループ防止用のタイマー
            while (Vector3.Distance(arm.position, startPos) > 0.1f && timeoutTimer < 3.0f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, startPos, swipeSpeed * 1.5f * Time.deltaTime));
                timeoutTimer += Time.deltaTime;
                yield return null;
            }

            if (SE != null) SE.Play("Boss.Charge");

            // --- 溜め (Charge) ---
            float timer = 0f;
            Vector3 chargePosition = arm.position;

            while (timer < windUpTime)
            {
                if (enableChargeShake)
                {
                    float shake = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
                    armRb.MovePosition(chargePosition + new Vector3(0, shake, 0));
                }
                timer += Time.deltaTime;
                yield return null;
            }
            armRb.MovePosition(chargePosition);

            if (SE != null) SE.Play("Boss.Swipe");

            // --- ビンタ (Swipe) ---
            boss.SetArmAnimation(isLeftArm, actionHandState);
            timeoutTimer = 0f;
            while (Vector3.Distance(arm.position, endPos) > 0.1f && timeoutTimer < 3.0f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, endPos, swipeSpeed * Time.deltaTime));
                timeoutTimer += Time.deltaTime;
                yield return null;
            }

            // --- 硬直 ---
            yield return new WaitForSeconds(0.5f);

            if (damageCollider != null) damageCollider.enabled = false;

            // --- 待機位置に戻る ---
            boss.SetArmAnimation(isLeftArm, returnHandState);
            timeoutTimer = 0f;
            while (Vector3.Distance(arm.position, restPosition.position) > 0.1f && timeoutTimer < 3.0f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, restPosition.position, returnSpeed * Time.deltaTime));
                timeoutTimer += Time.deltaTime;
                yield return null;
            }
            armRb.MovePosition(restPosition.position);

            if (damageCollider != null) damageCollider.enabled = true;
        }
        finally
        {
            boss.SetArmAnimation(isLeftArm, ArmAnimationState.Default);
            boss.SetAttackingState(false, isLeftArm);
        }
    }
}