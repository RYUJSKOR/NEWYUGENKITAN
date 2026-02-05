using UnityEngine;

public class OrotiShootBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
    {
        var neck = animator.GetComponent<OrotiNeck>();
        var controller = Object.FindAnyObjectByType<OrotiController>();

        if (neck == null || controller == null) return;

        neck.SpawnBullet(
            controller.PlayerTransform,
            controller.GetPhase()
        );
    }
}
