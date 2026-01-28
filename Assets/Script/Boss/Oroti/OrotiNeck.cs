using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrotiNeck : MonoBehaviour
{
	[Header("Neck ID")]
	public int neckId;

	private Animator animator;
	private OrotiDamageDealer dealer;

	private static readonly int AttackTrigger = Animator.StringToHash("Attack");

	private void Awake()
	{
		animator = GetComponent<Animator>();
		dealer = GetComponentInChildren<OrotiDamageDealer>();
	}

	public void PlayAttack()
	{
		dealer.DisableDamage();
		animator.ResetTrigger(AttackTrigger);
		animator.SetTrigger(AttackTrigger);
	}

	// Animator StateMachineBehaviour ‚©‚çŒÄ‚Î‚ê‚é
	public void EnableDamage()
	{
		dealer.EnableDamage();
	}

	public void DisableDamage()
	{
		dealer.DisableDamage();
	}
}
