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

    private float attackCooldown = 2f;
    private float attackTimer;

    private void Awake()
    {
        // HPイベント購読
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
        // 攻撃フェーズ中のみ攻撃
        if (!phaseController.IsAttackPhase) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0) return;

        ExecuteAttack();
        attackTimer = attackCooldown;
    }

    /// <summary>
    /// 首経由で呼ばれるボスへのダメージ
    /// </summary>
    public void ApplyDamageToBoss(float damage)
    {
        // 無敵を完全無視
        healthManager.ApplyDamage(damage, true);
    }


    private void ExecuteAttack()
    {
        if (attacks.Count == 0) return;

        var attack = attacks[Random.Range(0, attacks.Count)];
        var selected = GetRandomNecks(attack.UseNeckCount);

        attack.Execute(selected, player);
        phaseController.OnAttackExecuted();
    }


    private List<OrotiNeck> GetRandomNecks(int count)
    {
        List<OrotiNeck> pool = new(necks);
        List<OrotiNeck> result = new();

        count = Mathf.Min(count, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        return result;
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
