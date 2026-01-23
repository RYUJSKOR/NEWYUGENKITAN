using System.Collections.Generic;
using UnityEngine;

public class OrotiNeck : MonoBehaviour
{
    [SerializeField] private NeckAttackType neckType;
    public NeckAttackType Type => neckType;

    private Animator animator;
    private OrotiAttackRunner runner;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        runner = new OrotiAttackRunner(
            animator,
            GetComponentInChildren<OrotiDamageDealer>()
        );
    }

    private void Update()
    {
        runner.Tick();
    }

    public void PlayAttack(string anim, float start, float end)
    {
        runner.Play(anim, start, end);
    }
}
