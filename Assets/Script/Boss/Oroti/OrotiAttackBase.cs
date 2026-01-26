using System.Collections.Generic;
using UnityEngine;

public abstract class OrotiAttackBase : ScriptableObject
{
	[Header("Neck Selection")]
	public NeckSelectType selectType = NeckSelectType.Random;

	[Tooltip("Random‚Ég‚¤ñ‚Ì–{”")]
	public int useNeckCount = 1;

	[Tooltip("SpecificIDs‚Ég‚¤ñID")]
	public List<int> specificNeckIds = new();

	public abstract bool Execute(
		List<OrotiNeck> allNecks,
		Transform player
	);

	protected List<OrotiNeck> SelectNecks(List<OrotiNeck> allNecks)
	{
		if (allNecks == null || allNecks.Count == 0)
			return new();

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
}