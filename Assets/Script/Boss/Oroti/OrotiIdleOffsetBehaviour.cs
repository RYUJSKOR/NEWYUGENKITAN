using UnityEngine;

public class OrotiIdleOffsetBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        var neck = animator.GetComponent<OrotiNeck>();
        if (neck == null) return;

        // Åö Idle Ç…ì¸Ç¡ÇΩèuä‘ÇæÇØîΩâf
        animator.speed = neck.IdleSpeed;
    }

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        // Attack ë§Ç…âeãøÇµÇ»Ç¢ÇÊÇ§ñﬂÇ∑
        animator.speed = 1f;
    }
}
