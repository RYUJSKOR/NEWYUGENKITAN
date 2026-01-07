using UnityEngine;

public class FoxSkill : BulletSkill
{
    private int counterCount = 0;
    private const int maxCounters = 3;
    private float counterCooldown = 0.1f;
    private float counterCooldownTimer = 0f;
    private bool recentlyCountered = false;
    private bool isInCounterMode = false;
    private bool eventFlag = false;

    // EndSkill 遅延処理関連（追加）
    private bool isEndSkillPending = false;
    private float endSkillDelay = 0.5f;
    private float endSkillTimer = 0f;

    private CharacterHealthManager healthManager;
    private Shooting shooting;
    private FoxAnimation foxAnimation;

    public override SkillType SkillType => SkillType.Fox;

    private float skillTimer = 0f;
    protected override float maxDuration => 15f;

    public bool IsInCounterMode => isInCounterMode;

    public override void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        base.Init(player, playerStateMachine);

        if (player == null) return;

        healthManager = player.GetComponent<CharacterHealthManager>();
        shooting = player.GetComponentInChildren<Shooting>();
        foxAnimation = player.GetComponentInChildren<FoxAnimation>();

        if (foxAnimation == null)
        {
            var tailHolder = new GameObject("FoxTails");
            tailHolder.transform.SetParent(player.transform);
            tailHolder.transform.localPosition = Vector3.zero;
            tailHolder.transform.localRotation = Quaternion.identity;
            foxAnimation = tailHolder.AddComponent<FoxAnimation>();
            Debug.Log("[FoxSkill] FoxAnimationを新規生成");
        }

        foxAnimation.Init(player.transform);

        if (!eventFlag)
        {
            player.OnAttackedByEnemy -= HandleAttackEvent;
            player.OnAttackedByEnemy += HandleAttackEvent;
            eventFlag = true;
        }
    }

    public override void Update()
    {
        base.Update();

        // EndSkill遅延実行処理
        if (isEndSkillPending)
        {
            endSkillTimer += Time.deltaTime;
            if (endSkillTimer >= endSkillDelay)
            {
                isEndSkillPending = false;
                EndSkill();
            }
        }

        if (!isInCounterMode) return;

        foxAnimation?.UpdateTailGrowth(player.GetRigidbody.linearVelocity);

        skillTimer += Time.deltaTime;
        if (skillTimer >= maxDuration)
        {
            HandleAttackEvent();
            skillTimer = 0f;
            Debug.Log("[FoxSkill] 時間経過でカウント増加。現在: " + counterCount);

            if (counterCount >= maxCounters)
            {
                // 即EndSkillを避ける
                isEndSkillPending = true;
                endSkillTimer = 0f;
                return;
            }

            Invincibility();
        }

        if (recentlyCountered)
        {
            counterCooldownTimer += Time.deltaTime;
            if (counterCooldownTimer >= counterCooldown)
            {
                recentlyCountered = false;
                counterCooldownTimer = 0f;
            }
        }
    }

    public override void Remove()
    {
        base.Remove();
        foxAnimation.DestroyAllTails();
        if (eventFlag)
        {
            player.OnAttackedByEnemy -= HandleAttackEvent;
            eventFlag = false;
            Debug.Log("[FoxSkill] イベント解除");
        }
    }

    protected override bool IsInstantSkill()
    {
        return false;
    }

    private void Invincibility()
    {
        if (healthManager != null)
        {
            healthManager.ActivateInvincibility(maxDuration);
        }
    }

    protected override void BeginSkill()
    {
        base.BeginSkill();
        foxAnimation?.Init(player.transform);
        skillModeManager.SetSwitchMode(false);
        isInCounterMode = true;
        counterCount = 0;
        Invincibility();
        Debug.Log("[FoxSkill] カウン??モ?ド開始");
    }

    protected override void EndSkill()
    {
        base.EndSkill();
        skillModeManager.SetSwitchMode(true);
        isInCounterMode = false;

        foxAnimation?.DestroyAllTails();

        if (healthManager != null)
        {
            healthManager.ActivateInvincibility(0f);
            Debug.Log("[FoxSkill] カウン??モ?ド終了");
        }
    }

    private void HandleAttackEvent()
    {
        if (!isInCounterMode || recentlyCountered) return;

        recentlyCountered = true;
        counterCount += 1;

        Debug.Log("[FoxSkill] カウン??発動！");
        ShootInEightDirections();
        foxAnimation?.RemoveLastTail();
        Debug.Log("Count " + counterCount);

        if (counterCount >= maxCounters)
        {
            // ここも即EndSkillせず遅延（統一）
            isEndSkillPending = true;
            endSkillTimer = 0f;
            return;
        }

        skillTimer = 0f;
    }

    private void ShootInEightDirections()
    {
        if (shooting == null) return;

        int bulletCount = 8;
        float angleStep = 360f / bulletCount;
        float angle = 0f;

        for (int i = 0; i < bulletCount; i++)
        {
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f).normalized;
            shooting.RequestShoot(dir);
            angle += angleStep;
        }
    }
}