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

    [Header("Debug")]
    [SerializeField] private float logInterval = 30f;

    [Header("Attack Interval")]
	[SerializeField] private float attackCooldown = 2f;
	private float attackTimer;

    private OrotiAttackBase lastAttack;

    private Coroutine sequentialRoutine;

	public OrotiPhase CurrentPhase { get; private set; } = OrotiPhase.Phase1;
    public OrotiPhase GetPhase() => CurrentPhase;

    private void Awake()
	{
		healthManager.OnDamageTaken += OnBossDamaged;
		healthManager.OnDeath += OnBossDead;
	}

    private void Start()
    {
        StartCoroutine(LogPlayerDistanceRoutine());
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

        TryExecuteAttack();
        attackTimer = attackCooldown;
    }

    private IEnumerator LogPlayerDistanceRoutine()
    {
        var wait = new WaitForSeconds(logInterval);

        while (true)
        {
            LogDistancesToPlayer();
            yield return wait;
        }
    }

    private void LogDistancesToPlayer()
    {
        if (player == null) return;

        foreach (var neck in necks)
        {
            float dist = Vector3.Distance(
                neck.transform.position,
                player.position
            );

            Debug.Log(
                $"[Oroti] NeckID:{neck.neckId} Å® Player Distance: {dist:F2}",
                neck
            );
        }
    }

    public void StartSequentialAttack(  List<OrotiNeck> necks,float interval, OrotiAttackType type)
    {
        StartCoroutine(SequentialAttackCoroutine(necks, interval, type));
    }

    private IEnumerator SequentialAttackCoroutine(
        List<OrotiNeck> necks,
        float interval,
        OrotiAttackType type
    )
    {
        foreach (var neck in necks)
        {
            if (neck.CanAttack)
                neck.PlayAttack(type);

            yield return new WaitForSeconds(interval);
        }
    }

    private void TryExecuteAttack()
    {
        var candidates = GetAttackCandidates();
        if (candidates.Count == 0)
            return;

        var attack = candidates[Random.Range(0, candidates.Count)];

        bool executed = attack.Execute(
            necks,
            player,
            this
        );

        if (executed)
        {
            lastAttack = attack;
            phaseController.OnAttackExecuted();
        }
    }

    private List<OrotiAttackBase> GetAttackCandidates()
    {
        List<OrotiAttackBase> result = new();

        foreach (var attack in attacks)
        {
            if (!attack.allowRepeat && attack == lastAttack)
                continue;

            result.Add(attack);
        }

        return result;
    }

    // AttackBase Ç©ÇÁåƒÇŒÇÍÇÈ
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

        if (hpPercent < 0.33f)
        {
            CurrentPhase = OrotiPhase.Phase3;
            Debug.Log($" Oroti Phase Changed Å® Phase {CurrentPhase}");
        }
        else if (hpPercent < 0.66f)
        {
            CurrentPhase = OrotiPhase.Phase2;
            Debug.Log($" Oroti Phase Changed Å® Phase {CurrentPhase}");
        }
    }

    private void OnBossDead()
    {
        Debug.Log("ÉÑÉ}É^ÉmÉIÉçÉ`åÇîj");
        // åÇîjââèoÅEëJà⁄
    }

    private float GetHPPercent()
    {
        return healthManager.GetHealth() / healthManager.GetMaxHealth();
    }

    public Transform PlayerTransform => player;
}
