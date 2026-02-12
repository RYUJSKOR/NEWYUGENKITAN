using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewDoubleArmSlamAttack", menuName = "Boss Attacks/Double Arm Slam Attack")]
public class DoubleArmSlamAttack : BossAttackPattern
{
    [Header("追撃設定")]
    [Tooltip("プレイヤ?をス?ンさせた場合に即座に実行する追撃用の攻撃パ??ン")]
    public BossAttackPattern stunFollowUpAttack;
    [Tooltip("ス?ンさせてから追撃を開始するまでの待?時間（秒）")]
    public float stunFollowUpDelay = 1.0f;

    [Header("両腕?きつけ攻撃の基?設定")]
    [Tooltip("腕が目標地?へ移動する速度")]
    public float armAimSpeed = 20f;
    [Tooltip("振りかぶる高さ")]
    public float attackHeight = 10f;
    [Tooltip("攻撃の?備動作（溜め）の時間")]
    public float attackChargeTime = 2.0f;
    [Tooltip("?きつけた後、腕を地面に置いておく時間")]
    public float impactDowntime = 1.0f;
    [Tooltip("?きつけ時のス?ン判定の半径")]
    public float attackRadius = 5f;
    [Tooltip("腕が待?位置に戻る速度")]
    public float armReturnSpeed = 15f;
    [Tooltip("?きつけ時に加える初期衝動の強さ")]
    public float armSlamInitialForce = 60f;
    [Tooltip("この攻撃が?えるス?ンの持続時間")]
    public float stunDuration = 2.0f;

    [Header("画面?での振りかぶり設定")]
    [Tooltip("画面の左右の?からどれだけ内側に寄せるか")]
    public float screenEdgeHorizontalOffset = 2.0f;
    [Tooltip("カメラから腕までの仮想的な距離（XY座標の計算にのみ使用）")]
    public float distanceToCamera = 25f;

    [Header("演出用の設定")]
    [Tooltip("地面に?示される攻撃範囲??カ?")]
    public GameObject groundMarkerPrefab;
    [Tooltip("?きつけ時に発生する衝撃エフェクト（土煙など）")]
    public GameObject impactEffectPrefab;
    [Header("カメラシェイク設定")]
    [Tooltip("揺れの持続時間")]
    public float shakeDuration = 0.3f;
    [Tooltip("揺れの強さ")]
    public float shakeMagnitude = 0.4f;

    [Header("溜め演出の設定")]
    [Tooltip("溜め中の腕の震えの『強さ』")]
    public float chargeShakeIntensity = 0.1f;
    [Tooltip("溜め中の腕の震えの『速さ』")]
    public float chargeShakeSpeed = 50f;

    private GameObject leftMarkerInstance, rightMarkerInstance;

    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking || !boss.leftArmObject.activeSelf || !boss.rightArmObject.activeSelf) return;
        boss.RunAttackCoroutine(DoubleArmSlamRoutine(boss));
    }

    public override void Cleanup(BossController boss)
    {
        if (leftMarkerInstance != null) Object.Destroy(leftMarkerInstance);
        if (rightMarkerInstance != null) Object.Destroy(rightMarkerInstance);

        boss.SetArmAnimation(true, ArmAnimationState.Default);
        boss.SetArmAnimation(false, ArmAnimationState.Default);
        boss.SetBothArmsAttacking(false);
    }

    private IEnumerator DoubleArmSlamRoutine(BossController boss)
    {
        var SE = FindAnyObjectByType<SEController>();

        boss.SetBothArmsAttacking(true);

        Rigidbody leftArmRb = boss.leftArmObject.GetComponent<Rigidbody>();
        Rigidbody rightArmRb = boss.rightArmObject.GetComponent<Rigidbody>();
        BossWeakPoint leftArmWp = boss.leftArmObject.GetComponent<BossWeakPoint>();
        BossWeakPoint rightArmWp = boss.rightArmObject.GetComponent<BossWeakPoint>();

        try
        {
            // --- 1. ?備動作 (Aiming) ---
            boss.SetArmAnimation(true, prepareHandState);
            boss.SetArmAnimation(false, prepareHandState);
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Cameraが見つかりません！");
                yield break;
            }
            float distanceToPlayer = Vector3.Dot(boss.playerTransform.position - mainCamera.transform.position, mainCamera.transform.forward);
            Vector3 leftEdge = mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, distanceToPlayer));
            Vector3 rightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, distanceToPlayer));
            Vector3 leftTargetPos = leftEdge;
            Vector3 rightTargetPos = rightEdge;
            leftTargetPos.x += screenEdgeHorizontalOffset;
            rightTargetPos.x -= screenEdgeHorizontalOffset;
            leftTargetPos.y = boss.groundLevelY + attackHeight;
            rightTargetPos.y = boss.groundLevelY + attackHeight;

            while (Vector3.Distance(boss.leftArmObject.transform.position, leftTargetPos) > 0.5f ||
                   Vector3.Distance(boss.rightArmObject.transform.position, rightTargetPos) > 0.5f)
            {
                leftArmRb.MovePosition(Vector3.MoveTowards(boss.leftArmObject.transform.position, leftTargetPos, armAimSpeed * Time.deltaTime));
                rightArmRb.MovePosition(Vector3.MoveTowards(boss.rightArmObject.transform.position, rightTargetPos, armAimSpeed * Time.deltaTime));
                yield return null;
            }

            // --- 2. 溜め (Charging) ---
            Vector3 slamTargetPos = boss.playerTransform.position;
            if (groundMarkerPrefab != null)
            {
                Vector3 markerPos = new Vector3(slamTargetPos.x, boss.groundLevelY + 0.01f, slamTargetPos.z);
                leftMarkerInstance = Object.Instantiate(groundMarkerPrefab, markerPos, Quaternion.Euler(90, 0, 0));
                rightMarkerInstance = Object.Instantiate(groundMarkerPrefab, markerPos, Quaternion.Euler(90, 0, 0)); // Right marker added for symmetry
            }

            if (SE != null) SE.Play("Boss.Charge");

            float chargeTimer = 0f;
            Vector3 leftChargePos = boss.leftArmObject.transform.position;
            Vector3 rightChargePos = boss.rightArmObject.transform.position;

            while (chargeTimer < attackChargeTime)
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

            // --- 3. ?きつけ (Slam) ---
            boss.SetArmAnimation(true, actionHandState);
            boss.SetArmAnimation(false, actionHandState);
            leftArmWp.ResetGroundedFlag();
            rightArmWp.ResetGroundedFlag();
            leftArmRb.isKinematic = false;
            rightArmRb.isKinematic = false;
            leftArmRb.useGravity = true;
            rightArmRb.useGravity = true;
            leftArmRb.AddForce(Vector3.down * armSlamInitialForce, ForceMode.Impulse);
            rightArmRb.AddForce(Vector3.down * armSlamInitialForce, ForceMode.Impulse);

            yield return new WaitUntil(() => leftArmWp.IsGrounded && rightArmWp.IsGrounded);

            if (SE != null) SE.Play("Boss.Slam");

            // --- 4. 衝撃 (Impact) & 5. 追撃判定 ---
            leftArmRb.isKinematic = true;
            rightArmRb.isKinematic = true;

            // (A) 衝撃エフェクトとカメラシェイク
            Vector3 impactCenter = (boss.leftArmObject.transform.position + boss.rightArmObject.transform.position) / 2f;
            impactCenter.y = boss.groundLevelY;
            if (impactEffectPrefab != null)
            {
                Object.Instantiate(impactEffectPrefab, impactCenter, Quaternion.identity);
            }
            if (CameraShakeManager.instance != null)
            {
                CameraShakeManager.instance.TriggerShake(shakeDuration, shakeMagnitude);
            }

            // (B) プレイヤ?ス?ン判定
            Collider[] hitColliders = Physics.OverlapSphere(impactCenter, attackRadius);
            foreach (var hitCollider in hitColliders)
            {
                Player player = hitCollider.GetComponent<Player>();
                if (player != null && player.IsGrounded()) // Check if player is grounded
                {
                    player.TriggerStun(stunDuration);

                    if (stunFollowUpAttack != null)
                    {
                        yield return new WaitForSeconds(stunFollowUpDelay);
                        Cleanup(boss);
                        boss.ExecuteSpecificAttack(stunFollowUpAttack);
                        yield break;
                    }
                    break;
                }
            }

            yield return new WaitForSeconds(impactDowntime);

            // --- 6. 復帰 (Return) ---
            boss.SetArmAnimation(true, returnHandState);
            boss.SetArmAnimation(false, returnHandState);
            Vector3 leftRestPos = boss.leftArmRestPosition.position;
            Vector3 rightRestPos = boss.rightArmRestPosition.position;
            while (Vector3.Distance(boss.leftArmObject.transform.position, leftRestPos) > 0.5f ||
                   Vector3.Distance(boss.rightArmObject.transform.position, rightRestPos) > 0.5f)
            {
                leftArmRb.MovePosition(Vector3.MoveTowards(boss.leftArmObject.transform.position, leftRestPos, armReturnSpeed * Time.deltaTime));
                rightArmRb.MovePosition(Vector3.MoveTowards(boss.rightArmObject.transform.position, rightRestPos, armReturnSpeed * Time.deltaTime));
                yield return null;
            }
        }
        finally
        {
            if (boss.IsLeftArmAttacking && boss.IsRightArmAttacking)
            {
                Cleanup(boss);
            }
        }
    }
}