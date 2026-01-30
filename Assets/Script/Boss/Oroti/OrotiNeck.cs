using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrotiNeck : MonoBehaviour
{
	[Header("Neck ID")]
	public int neckId;

    [Header("Attack Cooldown")]
    [SerializeField] private float attackCooldown = 3f;

    private float lastAttackTime = -999f;

    [Header("Shoot")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletPower = 1f;

    [Header("Idle Random Range")]
    [SerializeField] private Vector2 idleSpeedRange = new Vector2(0.8f, 1.2f);

    [SerializeField] private Vector2 idleOffsetRange = new Vector2(0f, 1f);
    public float IdleSpeed => idleSpeed;

    private float idleSpeed;
    private float idleOffset;

    private Animator animator;
    private OrotiDamageDealer dealer;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int ShootTrigger = Animator.StringToHash("Shoot");

    private void Awake()
	{
		animator = GetComponent<Animator>();
		dealer = GetComponentInChildren<OrotiDamageDealer>();

        // 起動時にランダム決定（1回だけ）
        idleSpeed = Random.Range(idleSpeedRange.x, idleSpeedRange.y);
        idleOffset = Random.Range(idleOffsetRange.x, idleOffsetRange.y);
    }

    private void Start()
    {
        ApplyIdleOffset();
    }

    public bool CanAttack =>
        Time.time >= lastAttackTime + attackCooldown;

    public void PlayAttack()
	{
        // 攻撃確定時にクールタイム開始
        lastAttackTime = Time.time;

        dealer.DisableDamage();
        animator.ResetTrigger(AttackTrigger);
        animator.SetTrigger(AttackTrigger);
    }

    public void PlayShoot()
    {
        animator.SetTrigger(ShootTrigger);
    }

    public void SpawnBullet(Transform target)
    {
        if (shootPoint == null || bulletPrefab == null)
            return;

        var bulletObj = Instantiate(
            bulletPrefab,
            shootPoint.position,
            Quaternion.identity
        );

        var dir = (target.position - shootPoint.position).normalized;

        var bullet = bulletObj.GetComponent<OrotiBullet>();
        if (bullet != null)
        {
            bullet.Initialize(
                dir,
                gameObject,   // Owner
                bulletPower
            );
        }
    }

    /// <summary>
    /// Idle の再生位置をずらす
    /// </summary>
    public void ApplyIdleOffset()
    {
        animator.Play(0, 0, idleOffset);
        animator.Update(0f);
    }

    // Animator StateMachineBehaviour から呼ばれる
    public void EnableDamage()
	{
		dealer.EnableDamage();
	}

	public void DisableDamage()
	{
		dealer.DisableDamage();
	}
}
