using UnityEngine;

public class DemonSkill : BulletSkill
{
    public override SkillType SkillType => SkillType.Demon;

    [Header("Damage Area")]
    [SerializeField] private float radius = 4f;
    [SerializeField, Min(0.01f)] private float duration = 10f;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField, Range(0f, 1f)] private float damagePercentPerTick = 1f;
    [SerializeField] private LayerMask enemyLayer = ~0;
    [SerializeField] private bool bypassInvincibilityForDot = false;

    [Header("Visual & Explosion")]
    [SerializeField] private DemonAnimation demonAnimation;
    [SerializeField] private bool followPlayer = true;
    [SerializeField] private float explosionIntervalMin = 0.5f;
    [SerializeField] private float explosionIntervalMax = 1.5f;

    private float elapsed = 0f;
    private float tickTimer = 0f;

    private float explosionTimer = 0f;
    private float nextExplosionTime = 0f;

    public float Radius => radius;
    public Vector3 Center => player != null ? player.transform.position : Vector3.zero;
    public bool IsActive => isActive;
    public Transform OwnerTransform => player != null ? player.transform : null;

    #region BulletSkill Lifecycle

    public override void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        base.Init(player, playerStateMachine);
        demonAnimation = player.GetComponent<DemonAnimation>();
    }

    protected override void Skill()
    {
        elapsed = 0f;
        tickTimer = 0f;
        explosionTimer = 0f;
        nextExplosionTime = Random.Range(explosionIntervalMin, explosionIntervalMax);
        demonAnimation.isSpecialMove = true;

        // 発動直後ダメージ
        ApplyAreaDamage();

        // 球体生成＆爆発
        demonAnimation?.PlayExplosionThenShowSphere(
            OwnerTransform,
            Center,
            radius,
            followPlayer
        );
    }

    public override void Update()
    {
        base.Update();
        if (!isActive) return;

        float dt = Time.deltaTime;
        elapsed += dt;
        tickTimer += dt;
        explosionTimer += dt;

        // 範囲ダメージ
        if (tickTimer >= tickInterval)
        {
            ApplyAreaDamage();
            tickTimer = 0f;
        }

        // 球体追従
        demonAnimation?.UpdateVisual(Center, radius);

        // ランダム爆発
        if (explosionTimer >= nextExplosionTime)
        {
            demonAnimation?.SpawnRandomExplosion(Center, radius);
            explosionTimer = 0f;
            nextExplosionTime = Random.Range(explosionIntervalMin, explosionIntervalMax);
        }

        // スキル終了
        if (elapsed >= duration)
        {
            EndSkill();
        }
    }

    protected override bool IsInstantSkill() => false;

    protected override void EndSkill()
    {
        demonAnimation.isSpecialMove = false;
        base.EndSkill();
        demonAnimation?.Hide();
    }

    public override void Remove()
    {
        if (isActive)
        {
            EndSkill();
        }
    }

    #endregion

    #region Damage

    private void ApplyAreaDamage()
    {
        if (player == null) return;

        Collider[] hits = Physics.OverlapSphere(Center, radius, enemyLayer, QueryTriggerInteraction.Ignore);
        int hitCount = 0;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player")) continue;

            var chm = hit.GetComponent<CharacterHealthManager>();
            if (chm == null) continue;

            float maxHp = chm.GetMaxHealth();
            float damagePercent = maxHp >= 100f ? 0.03f : maxHp >= 50f ? 0.13f : damagePercentPerTick;
            float damage = maxHp * damagePercent;

            chm.ApplyDamage(damage, bypassInvincibilityForDot);
            hitCount++;
        }
    }

    #endregion
}
