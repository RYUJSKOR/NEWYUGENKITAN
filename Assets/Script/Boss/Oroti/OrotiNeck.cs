using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum OrotiAttackType
{
    Swing,
    Slam,
    Bite,
    Shoot
}

public class OrotiNeck : MonoBehaviour
{
    [Header("Neck ID")]
    public int neckId;

    [Header("Dive Replacement")]
    [SerializeField] private GameObject diveReplacementNeck;

    [Header("Distance")]
    [SerializeField] private Transform distancePoint;

    [Header("Attack Cooldown")]
    [SerializeField] private float attackCooldown = 3f;
    private float lastAttackTime = -999f;

    [Header("Shoot")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Bullet Select Mode")]
    [SerializeField] private bool useRandomBullet = false;
    [SerializeField] private OrotiBulletType fixedBulletType;
    [SerializeField] private List<OrotiBulletEntry> bulletEntries;

    [Header("Phase Bullet Settings")]
    [SerializeField] private OrotiBulletSetting phase1Setting;
    [SerializeField] private OrotiBulletSetting phase2Setting;
    [SerializeField] private OrotiBulletSetting phase3Setting;

    private int remainingShots;
    private float shotInterval;
    private Coroutine shootRoutine;

    [Header("Idle Random Range")]
    [SerializeField] private Vector2 idleSpeedRange = new Vector2(0.8f, 1.2f);

    [SerializeField] private Vector2 idleOffsetRange = new Vector2(0f, 1f);

    [SerializeField] private GameObject visualRoot;

    private float idleSpeed;
    private float idleOffset;

    public float IdleSpeed => idleSpeed;
    public bool CanAttack =>
    Time.time >= lastAttackTime + attackCooldown;


    private Animator animator;
    private OrotiDamageDealer dealer;
    private OrotiController controller;
    private OrotiBulletEntry currentBullet;

    private Renderer[] renderers;
    private Collider[] colliders;

    private static readonly int SwingTrigger = Animator.StringToHash("Swing");

    private static readonly int SlamTrigger = Animator.StringToHash("Slam");

    private static readonly int BiteTrigger = Animator.StringToHash("Bite");

    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        dealer = GetComponentInChildren<OrotiDamageDealer>();
        controller = GetComponentInParent<OrotiController>();

        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();

        if (diveReplacementNeck != null)
            diveReplacementNeck.SetActive(false);

        // 起動時にランダム決定（1回だけ）
        idleSpeed = Random.Range(idleSpeedRange.x, idleSpeedRange.y);
        idleOffset = Random.Range(idleOffsetRange.x, idleOffsetRange.y);
    }

    private void Start()
    {
        ApplyIdleOffset();
    }

    public Vector3 GetDistancePosition()
    {
        return distancePoint != null
            ? distancePoint.position
            : transform.position; // フォールバック
    }

    // Player との距離
    public float GetSqrDistanceToPlayer(Transform player)
    {
        if (player == null) return float.MaxValue;
        return (GetDistancePosition() - player.position).sqrMagnitude;
    }

    public void PlayAttack(OrotiAttackType type,float duration)
    {
        // 攻撃確定時にクールタイム開始
        lastAttackTime = Time.time;

        dealer.DisableDamage();

        if (duration <= 0f) duration = 1f;

        float speed = 1f / duration;
        animator.speed = speed;

        switch (type)
        {
            case OrotiAttackType.Swing:
                animator.ResetTrigger(SwingTrigger);
                animator.SetTrigger(SwingTrigger);
                break;

            case OrotiAttackType.Slam:
                animator.ResetTrigger(SlamTrigger);
                animator.SetTrigger(SlamTrigger);
                break;
        }
    }

    // 攻撃開始（AttackScript から呼ばれる）
    public void PlayShoot(int bulletCount, float interval)
    {
        lastAttackTime = Time.time;

        DecideRandomBullet();
        remainingShots = bulletCount;
        shotInterval = interval;

        animator.SetTrigger(ShootTrigger);
    }

    public void StartDive()
    {
        // ① まず通常首のDiveアニメを再生
        animator.SetTrigger("Dive");

        // ② Dive専用首の準備だけしておく
        if (diveReplacementNeck != null)
        {
            DiveReturnHandler handler =
                diveReplacementNeck.GetComponent<DiveReturnHandler>();

            if (handler != null)
                handler.SetOwner(this);
        }
    }
    public void OnSubmerged()
    {
 HideNormalNeck();

    if (diveReplacementNeck != null)
    {
        diveReplacementNeck.SetActive(true);

        Animator diveAnim =
            diveReplacementNeck.GetComponent<Animator>();

        if (diveAnim != null)
            diveAnim.SetTrigger("Dive");

        // ★ 直進開始
        DiveReturnHandler handler =
            diveReplacementNeck.GetComponent<DiveReturnHandler>();

        if (handler != null)
        {
            handler.SetOwner(this);
            handler.StartDiveMove();
        }
    }    }
    // --------------------------------------------------
    // 通常首を隠す
    // --------------------------------------------------
    private void HideNormalNeck()
    {
        visualRoot.SetActive(false);
    }

    // --------------------------------------------------
    // Dive終了後に呼ばれる
    // --------------------------------------------------
    public void RestoreFromDive()
    {
        visualRoot.SetActive(true);

        if (diveReplacementNeck != null)
            diveReplacementNeck.SetActive(false);
    }

    private void DecideRandomBullet()
    {
        if (bulletEntries == null || bulletEntries.Count == 0) return;

        if (useRandomBullet)
        {
            currentBullet = bulletEntries[
                Random.Range(0, bulletEntries.Count)
            ];
        }
        else
        {
            currentBullet = bulletEntries.Find(
                e => e.type == fixedBulletType
            );
        }
    }

    // Animation Event から呼ばれる
    public void OnShootAnimationEvent()
    {
        if (remainingShots <= 0)
            return;

        if (shootRoutine != null)
            StopCoroutine(shootRoutine);

        shootRoutine = StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine()
    {
        while (remainingShots > 0)
        {
            FireOneShot();
            remainingShots--;

            yield return new WaitForSeconds(shotInterval);
        }
    }

    private void FireOneShot()
    {
        if (controller == null) return;

        SpawnBullet(
            controller.PlayerTransform,
            controller.GetPhase()
        );
    }

    public void SpawnBullet(Transform target, OrotiPhase phase)
    {
        if (currentBullet == null)
            return;

        var bulletObj = Instantiate(
              bulletPrefab,
              shootPoint.position,
              Quaternion.identity
          );

        var bullet = bulletObj.GetComponent<OrotiBulletBase>();
        if (bullet == null)
        {
            Debug.LogError("弾PrefabにOrotiBulletBase継承スクリプトが付いていない！");
            return;
        }
        Vector3 dir = (target.position - shootPoint.position).normalized;

        bullet.Initialize(
            (target.position - shootPoint.position),
            gameObject,
            GetSettingByPhase(currentBullet, phase),
            target
        );
    }

    private OrotiBulletSetting GetSettingByPhase(
        OrotiBulletEntry entry,
        OrotiPhase phase)
    {
        return phase switch
        {
            OrotiPhase.Phase2 => entry.phase2Setting,
            OrotiPhase.Phase3 => entry.phase3Setting,
            _ => entry.phase1Setting
        };
    }

    /// <summary>
    /// Idle の再生位置をずらす
    /// </summary>
    public void ApplyIdleOffset()
    {
        animator.Play(0, 0, idleOffset);
        animator.Update(0f);
    }

    public void SetSwingDamage(float value)
    {
        if (dealer != null)
            dealer.SetDamage(value);
    }

    // Animator StateMachineBehaviour から呼ばれる
    public void EnableDamage() => dealer.EnableDamage();
    public void DisableDamage() => dealer.DisableDamage();
}
