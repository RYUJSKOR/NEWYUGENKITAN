using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class OrotiAttackBase : ScriptableObject
{
    // 首の選択方法
    [Header("Neck Selection")]
    public NeckSelectType selectType = NeckSelectType.Random;

    [Header("Random Selection Settings")]
    [Tooltip("Random時に抽出する首の数")]
    public int randomSelectCount = 1;

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

    [Header("Animation Settings")]
    [Tooltip("この攻撃のアニメーション再生秒数")]
    public float animationDuration = 1f;

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
        }

        return new List<OrotiNeck>();
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
    OrotiController controller,
    OrotiAttackType type,
        float animationDuration)
    {
        if (necks == null || necks.Count == 0)
            return false;

        // ① Priority適用
        necks = ApplyPriority(necks, player);

        // ② 上位X体 or ランダムX体抽出
        necks = ApplySubset(necks);

        if (necks == null || necks.Count == 0)
            return false;

        // ③ Order適用
        if (attackOrder == NeckAttackOrderType.Simultaneous)
        {
            ExecuteSimultaneousWithDuration(necks, type, animationDuration);
        }
        else
        {
            controller.StartCoroutine(
                      ExecuteSequential(necks, type, animationDuration));
        }

        return true;
    }

    protected List<OrotiNeck> ApplySubset(List<OrotiNeck> necks)
    {
        if (randomSelectCount <= 0 || necks.Count <= randomSelectCount)
            return necks;

        // Priorityなし → ランダム抽出
        if (priorityType == NeckPriorityType.None)
        {
            List<OrotiNeck> pool = new(necks);
            List<OrotiNeck> result = new();

            for (int i = 0; i < randomSelectCount; i++)
            {
                int idx = Random.Range(0, pool.Count);
                result.Add(pool[idx]);
                pool.RemoveAt(idx);
            }

            return result;
        }

        // Priorityあり → 上位から取得
        return necks.Take(randomSelectCount).ToList();
    }

    // --------------------
    // 同時攻撃
    // --------------------
    protected void ExecuteSimultaneousWithDuration(
        List<OrotiNeck> necks,
        OrotiAttackType type,
        float animDuration
    )
    {
        foreach (var neck in necks)
            neck.PlayAttack(type, animDuration);
    }
    // --------------------
    // 順番攻撃
    // --------------------
    protected IEnumerator ExecuteSequential(
        List<OrotiNeck> necks,
        OrotiAttackType type,
        float animDuration
    )
    {
        foreach (var neck in necks)
        {
            neck.PlayAttack(type, animDuration);
            yield return new WaitForSeconds(sequentialInterval);
        }
    }
}