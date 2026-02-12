using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "NewArmSlamAttack", menuName = "Boss Attacks/Arm Slam Attack")]
public class ArmSlamAttack : BossAttackPattern
{
    [Header("òr?Ç´Ç¬ÇØçUåÇÇÃê›íË")]
    public float armAimSpeed = 30f;
    public float attackTellDuration = 0.5f;
    public float attackChargeTime = 1.5f;
    public float postChargePauseDuration = 0.2f;
    public float attackImpactStunTime = 2.0f;
    public float armSlamSpeed = 80f;
    public float armReturnSpeed = 20f;
    public float armSlamInitialForce = 50f;

    [Header("ââèoópÇÃê›íË")]
    public GameObject groundMarkerPrefab;
    public GameObject chargeUpVFXPrefab;
    public string vfxAnchorName = "VFX_Anchor";

    private GameObject markerInstance;
    private GameObject vfxInstance;
    private Collider damageCollider;

    private bool isLeftArmAttackingNext = true;

    public override void Execute(BossController boss)
    {
        if (boss.IsAttacking) return;
        Transform armToAttack = null;
        Transform restPosition = null;
        bool isLaunchingLeft = false;
        if (isLeftArmAttackingNext && boss.leftArmObject.activeSelf)
        {
            armToAttack = boss.leftArmObject.transform;
            restPosition = boss.leftArmRestPosition;
            isLaunchingLeft = true;
        }
        else if (!isLeftArmAttackingNext && boss.rightArmObject.activeSelf)
        {
            armToAttack = boss.rightArmObject.transform;
            restPosition = boss.rightArmRestPosition;
            isLaunchingLeft = false;
        }
        else if (boss.leftArmObject.activeSelf)
        {
            armToAttack = boss.leftArmObject.transform;
            restPosition = boss.leftArmRestPosition;
            isLaunchingLeft = true;
        }
        else if (boss.rightArmObject.activeSelf)
        {
            armToAttack = boss.rightArmObject.transform;
            restPosition = boss.rightArmRestPosition;
            isLaunchingLeft = false;
        }
        if (armToAttack != null)
        {
            boss.RunAttackCoroutine(ArmSlamRoutine(boss, armToAttack, restPosition, isLaunchingLeft));
            isLeftArmAttackingNext = !isLeftArmAttackingNext;
        }
    }

    private IEnumerator ArmSlamRoutine(BossController boss, Transform arm, Transform restPosition, bool isLeftArm)
    {
        var SE = FindAnyObjectByType<SEController>();

        boss.SetAttackingState(true, isLeftArm);

        Rigidbody armRb = arm.GetComponent<Rigidbody>();
        BossWeakPoint armWp = arm.GetComponent<BossWeakPoint>();
        Transform damageZoneTransform = arm.Find("DamageZone");
        damageCollider = (damageZoneTransform != null) ? damageZoneTransform.GetComponent<Collider>() : null;

        markerInstance = null;
        vfxInstance = null;

        try
        {
            // --- çUåÇ?îı ---
            boss.SetArmAnimation(isLeftArm, prepareHandState);
            yield return new WaitForSeconds(attackTellDuration);

            Vector3 aimPosition = boss.playerTransform.position + Vector3.up * 5f;
            while (Vector3.Distance(arm.position, aimPosition) > 0.1f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, aimPosition, armAimSpeed * Time.deltaTime));
                yield return null;
            }

            if (SE != null) SE.Play("Boss.Charge");

            // --- ?ÉÅ ---
            float timer = 0;
            Vector3 lockedOnPosition = arm.position;
            float lockOnTime = attackChargeTime - 0.1f;
            while (timer < attackChargeTime)
            {
                Vector3 targetPos;
                if (timer < lockOnTime)
                {
                    targetPos = new Vector3(boss.playerTransform.position.x, arm.position.y, boss.playerTransform.position.z);
                    lockedOnPosition = targetPos;
                }
                else
                {
                    targetPos = lockedOnPosition;
                }

                float shakeAmount = 0.15f;
                float shakeSpeed = 50f;
                float offsetX = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount;
                float offsetZ = Mathf.Cos(Time.time * shakeSpeed * 1.2f) * shakeAmount;
                armRb.MovePosition(targetPos + new Vector3(offsetX, 0, offsetZ));

                timer += Time.deltaTime;
                yield return null;
            }
            armRb.MovePosition(lockedOnPosition);

            // --- çUåÇé¿çs ---
            if (groundMarkerPrefab != null)
            {
                Vector3 markerPos = new Vector3(lockedOnPosition.x, boss.groundLevelY + 0.01f, lockedOnPosition.z);
                markerInstance = Object.Instantiate(groundMarkerPrefab, markerPos, Quaternion.Euler(90, 0, 0));
                markerInstance.transform.localScale = new Vector3(5, 5, 1);
            }
            if (chargeUpVFXPrefab != null)
            {
                Transform anchor = arm.Find(vfxAnchorName);
                Transform spawnTransform = (anchor != null) ? anchor : arm;
                vfxInstance = Object.Instantiate(chargeUpVFXPrefab, spawnTransform.position, spawnTransform.rotation, spawnTransform);
            }

            yield return new WaitForSeconds(postChargePauseDuration);

            boss.SetArmAnimation(isLeftArm, actionHandState);

            armWp.ResetGroundedFlag();
            armRb.linearVelocity = Vector3.zero;
            armRb.angularVelocity = Vector3.zero;
            armRb.isKinematic = false;
            armRb.useGravity = true;
            armRb.AddForce(Vector3.down * armSlamInitialForce, ForceMode.Impulse);
            while (!armWp.IsGrounded)
            {
                Vector3 correctedPos = new Vector3(lockedOnPosition.x, arm.position.y, lockedOnPosition.z);
                armRb.position = correctedPos;
                yield return null;
            }

            if (SE != null) SE.Play("Boss.Slam");

            armRb.isKinematic = true;
            armRb.useGravity = false;
            yield return new WaitForSeconds(attackImpactStunTime);

            if (damageCollider != null) damageCollider.enabled = false;

            // --- ñﬂÇË ---
            boss.SetArmAnimation(isLeftArm, returnHandState);

            while (Vector3.Distance(arm.position, restPosition.position) > 0.1f)
            {
                armRb.MovePosition(Vector3.MoveTowards(arm.position, restPosition.position, armReturnSpeed * Time.deltaTime));
                yield return null;
            }
            armRb.MovePosition(restPosition.position);
        }
        finally
        {
            Cleanup(boss, isLeftArm);
        }
    }

    public override void Cleanup(BossController boss)
    {
        Cleanup(boss, true);
        Cleanup(boss, false);
    }

    private void Cleanup(BossController boss, bool isLeftArm)
    {
        if (markerInstance != null) Object.Destroy(markerInstance);
        if (vfxInstance != null) Object.Destroy(vfxInstance);
        if (damageCollider != null) damageCollider.enabled = true;

        boss.SetArmAnimation(isLeftArm, ArmAnimationState.Default);
        boss.SetAttackingState(false, isLeftArm);

        markerInstance = null;
        vfxInstance = null;
        damageCollider = null;
    }
}