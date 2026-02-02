using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class OrotiAttackBase : ScriptableObject
{
    // 首の選択方法
    [Header("Neck Selection")]
    public NeckSelectType selectType = NeckSelectType.Random;

    [Tooltip("Random時に使う首の本数")]
    public int useNeckCount = 1;

    [Tooltip("SpecificIDs時に使う首ID")]
    public List<int> specificNeckIds = new();

    // 優先順位
    [Header("Neck Priority")]
    public NeckPriorityType priorityType = NeckPriorityType.None;

    // 攻撃順
    [Header("Attack Order")]
    public NeckAttackOrderType attackOrder = NeckAttackOrderType.Simultaneous;

    [Tooltip("順番攻撃時の待ち時間")]
    public float sequentialInterval = 0.3f;

    [Header("Repeat Rule")]
    [Tooltip("同じ攻撃を連続で使用できるか")]
    public bool allowRepeat = true;

    /// <summary>
    /// 攻撃が実行されたら true
    /// </summary>
    public abstract bool Execute(
        List<OrotiNeck> allNecks,
        Transform player,
        OrotiController controller
    );

    // --------------------
    // 首選択
    // --------------------
    protected List<OrotiNeck> SelectNecks(List<OrotiNeck> allNecks)
    {
        switch (selectType)
        {
            case NeckSelectType.All:
                return new List<OrotiNeck>(allNecks);

            case NeckSelectType.SpecificIDs:
                return allNecks.FindAll(
                    n => specificNeckIds.Contains(n.neckId)
                );

            case NeckSelectType.Random:
            default:
                List<OrotiNeck> pool = new(allNecks);
                List<OrotiNeck> result = new();

                int count = Mathf.Min(useNeckCount, pool.Count);
                for (int i = 0; i < count; i++)
                {
                    int idx = Random.Range(0, pool.Count);
                    result.Add(pool[idx]);
                    pool.RemoveAt(idx);
                }
                return result;
        }
    }

    // --------------------
    // 攻撃可能な首だけ抽出
    // --------------------
    protected List<OrotiNeck> FilterAttackable(List<OrotiNeck> necks)
    {
        return necks.FindAll(n => n.CanAttack);
    }

    // --------------------
    // Player距離による優先順位
    // --------------------
    protected List<OrotiNeck> ApplyPriority(
        List<OrotiNeck> necks,
        Transform player
    )
    {
        if (priorityType == NeckPriorityType.None || player == null)
            return necks;

        return priorityType switch
        {
            NeckPriorityType.NearestToPlayer =>
                necks.OrderBy(n =>
                    n.GetSqrDistanceToPlayer(player)
                ).ToList(),

            NeckPriorityType.FarthestFromPlayer =>
                necks.OrderByDescending(n =>
                    n.GetSqrDistanceToPlayer(player)
                ).ToList(),

            _ => necks
        };
    }

    // --------------------
    // 実行制御（中核）
    // --------------------
    protected bool ExecuteByOrder(
        List<OrotiNeck> necks,
        Transform player,
        OrotiController controller
    )
    {
        if (necks == null || necks.Count == 0)
            return false;

        // Priority 適用
        necks = ApplyPriority(necks, player);

        // Order 適用
        if (attackOrder == NeckAttackOrderType.Simultaneous)
        {
            ExecuteSimultaneous(necks);
        }
        else
        {
            controller.StartCoroutine(
                ExecuteSequential(necks)
            );
        }

        return true;
    }

    // --------------------
    // 同時攻撃
    // --------------------
    protected void ExecuteSimultaneous(List<OrotiNeck> necks)
    {
        foreach (var neck in necks)
            neck.PlayAttack();
    }

    // --------------------
    // 順番攻撃
    // --------------------
    protected IEnumerator ExecuteSequential(List<OrotiNeck> necks)
    {
        foreach (var neck in necks)
        {
            neck.PlayAttack();
            yield return new WaitForSeconds(sequentialInterval);
        }
    }
}