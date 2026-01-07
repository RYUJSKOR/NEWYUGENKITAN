using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NohMaskSkill : BulletSkill
{
    private NohMaskAnimation nohMaskAnimation;

    [Header("Damage")]
    [SerializeField] private float totalDamage = 200f;

    public override SkillType SkillType => SkillType.Noh;

    public override void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        base.Init(player, playerStateMachine);
        Debug.Log("NohMaskSkill 初期化完了");

        nohMaskAnimation = player.GetComponent<NohMaskAnimation>();
    }

    public override void HandleInput()
    {
        base.HandleInput();
    }

    protected override void Skill()
    {
        var targets = GetEnemiesInViewLimited(4);

        if (targets.Count == 0)
        {
            Debug.Log("[NohMaskSkill] 画面内に敵がいませんでした。");
            return;
        }

        float splitDamage = totalDamage / targets.Count;

        if (nohMaskAnimation != null)
        {
            nohMaskAnimation.ShowMultiple(targets, () =>
            {
                ApplyDamageToTargets(targets, splitDamage);
            });
        }
        else
        {
            ApplyDamageToTargets(targets, splitDamage);
        }
    }

    private void ApplyDamageToTargets(List<GameObject> targets, float splitDamage)
    {
        foreach (var enemy in targets)
        {
            // まず、敵のコライダーを取得します。
            Collider enemyCollider = enemy.GetComponent<Collider>();

            // コライダーが存在し、かつ有効な（enabled == true）場合のみ、次の処理に進みます。
            if (enemyCollider != null && enemyCollider.enabled)
            {
                var damageable = enemy.GetComponent<CharacterHealthManager>();
                if (damageable != null)
                {
                    damageable.ApplyDamage(splitDamage);
                    Debug.Log($"[NohMaskSkill] {enemy.name} に {splitDamage} ダメージを与えた。");
                }
            }
            else
            {
                // コライダーが無効な場合は、ダメージを与えずにログを出力します（デバッグに便利です）。
                Debug.Log($"[NohMaskSkill] {enemy.name} は攻撃可能な状態ではないため、ダメージを与えませんでした。");
            }
        }
    }
   

    protected override void SubSkill()
    {
        Debug.Log("NohMaskSkill サブスキルは未実装です。");
    }

    public override void Remove()
    {
        base.Remove();
        Debug.Log("[NohMaskSkill] イベント解除");
    }

    protected List<GameObject> GetEnemiesInViewLimited(int count)
    {
        var enemiesInView = EnemyCounter.Instance?.GetEnemiesInView();
        List<GameObject> normalEnemies = enemiesInView != null ? enemiesInView.ToList() : new List<GameObject>();

        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        List<GameObject> bossList = bosses.ToList();

        ShuffleList(bossList);
        ShuffleList(normalEnemies);

        List<GameObject> result = new List<GameObject>();

        int bossCount = Mathf.Min(bossList.Count, count);
        result.AddRange(bossList.Take(bossCount));

        int remaining = count - bossCount;
        if (remaining > 0)
        {
            result.AddRange(normalEnemies.Take(remaining));
        }

        return result;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}