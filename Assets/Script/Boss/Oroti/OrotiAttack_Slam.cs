using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Slam")]
public class OrotiAttack_Slam : OrotiAttackBase
{
	public override bool Execute(
		 List<OrotiNeck> allNecks,
		 Transform player)
	{
		var targets = SelectNecks(allNecks);
		if (targets.Count == 0)
			return false;

		foreach (var neck in targets)
		{
			neck.PlayAttack();
		}

		return true;
	}
}