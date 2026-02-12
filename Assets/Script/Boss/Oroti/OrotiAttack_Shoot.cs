using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Shoot")]
public class OrotiAttack_Shoot : OrotiAttackBase
{
	[Header("Shoot Count")]
	[SerializeField] private int bulletsPerNeck = 3;

	[Header("Bullet Interval")]
	[SerializeField] private float bulletInterval = 0.15f;

	[Header("Sequential Interval")]
	[SerializeField] private float shootInterval = 0.4f;

	public override bool Execute(
		List<OrotiNeck> allNecks,
		Transform player,
		OrotiController controller)
	{
		var selected = SelectNecks(allNecks);
		var attackable = FilterAttackable(selected);

		if (attackable.Count == 0)
			return false;

		switch (attackOrder)
		{
			case NeckAttackOrderType.Simultaneous:
				foreach (var neck in attackable)
				{
					neck.PlayShoot(bulletsPerNeck, bulletInterval);
				}
				break;

			case NeckAttackOrderType.Sequential:
				controller.StartSequentialAttack(
					SequentialShootCoroutine(attackable)
				);
				break;
		}

		return true;
	}

	private IEnumerator SequentialShootCoroutine(List<OrotiNeck> necks)
	{
		foreach (var neck in necks)
		{
			if (neck.CanAttack)
				neck.PlayShoot(bulletsPerNeck, bulletInterval);

			yield return new WaitForSeconds(shootInterval);
		}
	}
}