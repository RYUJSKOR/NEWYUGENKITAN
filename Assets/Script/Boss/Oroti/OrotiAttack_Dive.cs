using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Dive")]
public class OrotiAttack_Dive : OrotiAttackBase
{
    [Header("Sequential Interval")]
    [SerializeField] private float diveInterval = 0.4f;

    public override bool Execute(
        List<OrotiNeck> allNecks,
        Transform player,
        OrotiController controller)
    {
        // ‡@ ƒOƒ‹[ƒv‘I‘ğ
        var selected = SelectNecks(allNecks);

        // ‡A UŒ‚‰Â”\‚Èñ‚Ì‚İ
        var attackable = FilterAttackable(selected);

        if (attackable.Count == 0)
            return false;

        // ‡B Priority“K—p
        attackable = ApplyPriority(attackable, player);

        // ‡C Subset“K—p
        attackable = ApplySubset(attackable);

        if (attackable.Count == 0)
            return false;

        // ‡D Às
        switch (attackOrder)
        {
            case NeckAttackOrderType.Simultaneous:
                foreach (var neck in attackable)
                {
                    neck.StartDive();
                }
                break;

            case NeckAttackOrderType.Sequential:
                controller.StartSequentialAttack(
                    SequentialDiveCoroutine(attackable)
                );
                break;
        }

        return true;
    }

    private IEnumerator SequentialDiveCoroutine(List<OrotiNeck> necks)
    {
        foreach (var neck in necks)
        {
            if (neck.CanAttack)
                neck.StartDive();

            yield return new WaitForSeconds(diveInterval);
        }
    }
}
