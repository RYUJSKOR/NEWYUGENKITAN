using System.Collections.Generic;
using UnityEngine;

public class OrotiController : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private CharacterHealthManager healthManager;

    [Header("Phase")]
    [SerializeField] private OrotiPhaseController phaseController;

    [Header("Attack")]
    [SerializeField] private OrotiAttackManager attackManager;
    [SerializeField] private List<OrotiNeck> necks;
    [SerializeField] private Transform player;

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
        if (!phaseController.IsAttackPhase)
            return;

        ExecuteAttack();
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
        var attacks = attackManager.GetAvailableAttacks(GetHPPercent());
        if (attacks.Count == 0) return;

        OrotiAttackBase attack = attacks[Random.Range(0, attacks.Count)];
        OrotiNeck neck = necks[Random.Range(0, necks.Count)];

        attack.Execute(neck, player);
        phaseController.OnAttackExecuted();
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
