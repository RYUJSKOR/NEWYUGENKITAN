using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class OrotiAttackBase : ScriptableObject
{
	[Header("Neck Selection")]
	public NeckSelectType selectType = NeckSelectType.Random;

	[Tooltip("Random時に使う首の本数")]
	public int useNeckCount = 1;

	[Tooltip("SpecificIDs時に使う首ID")]
	public List<int> specificNeckIds = new();

    [Header("Attack Order")]
    public NeckAttackOrderType attackOrder = NeckAttackOrderType.Simultaneous;

    [Tooltip("順番攻撃時の待ち時間")]
    public float sequentialInterval = 0.3f;

    /// <summary>
    /// 攻撃が実際に行われた場合 true
    /// </summary>
    public abstract bool Execute(
            List<OrotiNeck> allNecks,
            Transform player,
            OrotiController controller   // 明示的依存
        );

    protected List<OrotiNeck> SelectNecks(List<OrotiNeck> allNecks)
    {
        switch (selectType)
        {
            case NeckSelectType.All:
                return new(allNecks);

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

    protected List<OrotiNeck> FilterAttackable(List<OrotiNeck> necks)
    {
        return necks.FindAll(n => n.CanAttack);
    }

    protected void ExecuteSimultaneous(List<OrotiNeck> necks)
    {
        foreach (var neck in necks)
            neck.PlayAttack();
    }

    protected IEnumerator ExecuteSequential(List<OrotiNeck> necks)
    {
        foreach (var neck in necks)
        {
            neck.PlayAttack();
            yield return new WaitForSeconds(sequentialInterval);
        }
    }
}