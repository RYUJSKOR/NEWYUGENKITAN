using UnityEngine;

public class OrotiAttackRunner : MonoBehaviour
{
    private Animator animator;
    private OrotiDamageDealer dealer;

    private float damageStart;
    private float damageEnd;
    private bool running;

    public OrotiAttackRunner(Animator animator, OrotiDamageDealer dealer)
    {
        this.animator = animator;
        this.dealer = dealer;
    }

    public void Play(string animName, float start, float end)
    {
        damageStart = start;
        damageEnd = end;
        running = true;

        animator.Play(animName, 0, 0f);
    }

    public void Tick()
    {
        if (!running) return;

        var info = animator.GetCurrentAnimatorStateInfo(0);
        float t = info.normalizedTime;

        if (t >= damageStart && t <= damageEnd)
            dealer.EnableDamage();
        else
            dealer.DisableDamage();

        if (t >= 1f)
        {
            dealer.DisableDamage();
            running = false;
        }
    }

    public bool IsRunning => running;
}
