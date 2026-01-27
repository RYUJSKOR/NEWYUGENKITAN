using UnityEngine;

public class OrotiAttackState : StateMachineBehaviour
{
	private OrotiNeck neck;

	override public void OnStateEnter(
		Animator animator,
		AnimatorStateInfo stateInfo,
		int layerIndex)
	{
		neck = animator.GetComponent<OrotiNeck>();
		neck?.EnableDamage();
	}

	override public void OnStateExit(
		Animator animator,
		AnimatorStateInfo stateInfo,
		int layerIndex)
	{
		neck?.DisableDamage();
	}
}
