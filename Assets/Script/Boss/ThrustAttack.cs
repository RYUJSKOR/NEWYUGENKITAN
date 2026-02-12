using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewThrustAttack", menuName = "Boss Attacks/Thrust Attack")]
public class ThrustAttack : BossAttackPattern
{
    [Header("突き刺し攻撃の設定")]
    public float windUpDepth = 10f;
    public float lungeDepth = -5f;
    public float moveSpeed = 50f;
    public float lungeSpeed = 70f;
    public float returnSpeed = 25f;
    [Tooltip("プレイヤ?を追尾する照?時間")]
    public float aimDuration = 1.0f;
    [Tooltip("照?完了後、突き刺すまでの溜め時間")]
    public float chargeTime = 0.7f;
    public float postLungePauseTime = 1.0f;

    [Header("溜め演出の設定")]
    [Tooltip("溜め中の腕の震えの『強さ』")]
    public float chargeShakeIntensity = 0.1f;
    [Tooltip("溜め中の腕の震えの『速さ』")]
    public float chargeShakeSpeed = 50f;

    private bool isLeftArmAttackingNext = true;

    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking) return;

        bool isLeftArm = isLeftArmAttackingNext;
        Transform armToAttack = isLeftArm ? boss.leftArmObject.transform : boss.rightArmObject.transform;

        if (!armToAttack.gameObject.activeSelf)
        {
            isLeftArm = !isLeftArm;
            armToAttack = isLeftArm ? boss.leftArmObject.transform : boss.rightArmObject.transform;
        }

        if (armToAttack.gameObject.activeSelf)
        {
            Transform restPosition = isLeftArm ? boss.leftArmRestPosition : boss.rightArmRestPosition;
            boss.RunAttackCoroutine(ThrustRoutine(boss, armToAttack, restPosition, isLeftArm));
            isLeftArmAttackingNext = !isLeftArmAttackingNext;
        }
    }

    private IEnumerator ThrustRoutine(BossController boss, Transform arm, Transform restPosition, bool isLeftArm)
    {
        var SE = FindAnyObjectByType<SEController>();
        boss.SetAttackingState(true, isLeftArm);
        Rigidbody armRb = arm.GetComponent<Rigidbody>();
        Collider damageCollider = arm.Find("DamageZone")?.GetComponent<Collider>();
        if (damageCollider != null) damageCollider.enabled = false;

        try
        {
            // --- ステップ1: 振りかぶり (指定の?さまで下がる) ---
            boss.SetArmAnimation(isLeftArm, prepareHandState);
            Vector3 initialWindUpPos = new Vector3(restPosition.position.x, restPosition.position.y, windUpDepth);
            while (Vector3.Distance(arm.position, initialWindUpPos) > 0.1f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, initialWindUpPos, moveSpeed * Time.deltaTime));
                yield return null;
            }

            // --- ステップ2: 照? (プレイヤ?を追尾し続ける) ---
            float aimTimer = 0f;
            while (aimTimer < aimDuration)
            {
                Vector3 playerTargetPos = boss.playerTransform.position;
                Vector3 aimPos = new Vector3(playerTargetPos.x, playerTargetPos.y, windUpDepth);

                // プレイヤ?を滑らかに追尾
                armRb.MovePosition(Vector3.Lerp(arm.position, aimPos, Time.deltaTime * 10f));

                aimTimer += Time.deltaTime;
                yield return null;
            }

            if (SE != null) SE.Play("Boss.Charge");

            // --- ステップ3: 溜め (追尾をやめてその場で震える) ---
            float chargeTimer = 0f;
            Vector3 chargeBasePos = arm.position; // 照?完了時?の座標を保存
            while (chargeTimer < chargeTime)
            {
                float offsetX = Mathf.Sin(Time.time * chargeShakeSpeed) * chargeShakeIntensity;
                float offsetY = Mathf.Cos(Time.time * chargeShakeSpeed * 1.2f) * chargeShakeIntensity;
                armRb.MovePosition(chargeBasePos + new Vector3(offsetX, offsetY, 0));

                chargeTimer += Time.deltaTime;
                yield return null;
            }
            armRb.MovePosition(chargeBasePos); // 震えを?める

            // --- ステップ4: 突き刺し ---
            boss.SetArmAnimation(isLeftArm, actionHandState);
            if (damageCollider != null) damageCollider.enabled = true;

            if (SE != null) SE.Play("Boss.Slam");

            // 溜め完了時?のX, Y座標に向かって突き刺す
            Vector3 lungePos = new Vector3(chargeBasePos.x, chargeBasePos.y, lungeDepth);
            while (Mathf.Abs(arm.position.z - lungePos.z) > 0.1f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, lungePos, lungeSpeed * Time.deltaTime));
                yield return null;
            }
            armRb.MovePosition(lungePos);

            // --- ステップ5: 硬直 ---
            yield return new WaitForSeconds(postLungePauseTime);

            // --- ステップ6: 待?位置に戻る ---
            boss.SetArmAnimation(isLeftArm, returnHandState);
            if (damageCollider != null) damageCollider.enabled = false;

            while (Vector3.Distance(arm.position, restPosition.position) > 0.1f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, restPosition.position, returnSpeed * Time.deltaTime));
                yield return null;
            }
            armRb.MovePosition(restPosition.position);
        }
        finally
        {
            // --- 終了処理 ---
            boss.SetArmAnimation(isLeftArm, ArmAnimationState.Default);
            if (damageCollider != null) damageCollider.enabled = false;
            boss.SetAttackingState(false, isLeftArm);
        }
    }
}