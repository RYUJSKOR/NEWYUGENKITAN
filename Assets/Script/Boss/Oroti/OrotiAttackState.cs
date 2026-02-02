using UnityEngine;

public class OrotiAttackState : StateMachineBehaviour
{
    private OrotiNeck neck;
    private bool damageDisabled;

    [Header("Attack End Timing")]
    [Range(0f, 1f)]
    [SerializeField] private float damageEndNormalizedTime = 0.4f;

    override public void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        neck = animator.GetComponent<OrotiNeck>();
        damageDisabled = false;

        neck?.EnableDamage();
    }

    override public void OnStateUpdate(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        if (neck == null || damageDisabled)
            return;

        // normalizedTime ‚Í 1.0 ‚ğ’´‚¦‚é‚Ì‚Å % ‚ÅŒ©‚é
        float t = stateInfo.normalizedTime % 1f;

        if (t >= damageEndNormalizedTime)
        {
            neck.DisableDamage();
            damageDisabled = true;
        }
    }

    override public void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        // •ÛŒ¯i“r’†‘JˆÚ‘Îôj
        neck?.DisableDamage();
    }
}
