using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrotiController : MonoBehaviour
{
	[Header("HP")]
	[SerializeField] private CharacterHealthManager healthManager;

	[Header("Phase")]
	[SerializeField] private OrotiPhaseController phaseController;

	[Header("Necks")]
	[SerializeField] private List<OrotiNeck> necks;

	[Header("Attacks")]
	[SerializeField] private List<OrotiAttackBase> attacks;

	[SerializeField] private Transform player;

	[Header("Attack Interval")]
	[SerializeField] private float attackCooldown = 2f;
	private float attackTimer;

    private Coroutine sequentialRoutine;

    private void Awake()
	{
		healthManager.OnDamageTaken += OnBossDamaged;
		healthManager.OnDeath += OnBossDead;
	}

	private void OnDestroy()
	{
		healthManager.OnDamageTaken -= OnBossDamaged;
		healthManager.OnDeath -= OnBossDead;
	}

	private void Update()
	{
		if (!phaseController.IsAttackPhase)
			return;

		attackTimer -= Time.deltaTime;
		if (attackTimer > 0f)
			return;

		ExecuteAttack();
		attackTimer = attackCooldown;
	}

	private void ExecuteAttack()
	{
        if (attacks.Count == 0) return;

        var attack = attacks[Random.Range(0, attacks.Count)];

        bool executed = attack.Execute(necks, player, this);
        if (executed)
        {
            phaseController.OnAttackExecuted();
        }
    }

    // AttackBase から呼ばれる
    public void StartSequentialAttack(IEnumerator routine)
    {
        if (sequentialRoutine != null)
            StopCoroutine(sequentialRoutine);

        sequentialRoutine = StartCoroutine(routine);
    }

    public void ApplyDamageToBoss(float damage)
	{
		healthManager.ApplyDamage(damage, true);
	}

	private void OnBossDamaged()
    {
        float hpPercent = GetHPPercent();

        // HP減少時の演出・挙動変更をここに集約
        if (hpPercent < 0.75f)
        {
            // 攻撃が激しくなった演出など
        }

        if (hpPercent < 0.4f)
        {
            // 終盤演出
        }
    }

    private void OnBossDead()
    {
        Debug.Log("ヤマタノオロチ撃破");
        // 撃破演出・遷移
    }

    private float GetHPPercent()
    {
        return healthManager.GetHealth() / healthManager.GetMaxHealth();
    }
}
