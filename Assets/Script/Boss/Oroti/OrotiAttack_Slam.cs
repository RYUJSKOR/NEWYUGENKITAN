using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Slam")]
public class OrotiAttack_Slam : OrotiAttackBase
{
    public override bool Execute(
            List<OrotiNeck> allNecks,
            Transform player,
            OrotiController controller)
    {
        var selected = SelectNecks(allNecks);
        var attackable = FilterAttackable(selected);

        if (attackable.Count == 0)
            return false;

        if (attackOrder == NeckAttackOrderType.Simultaneous)
        {
            ExecuteSimultaneous(attackable);
        }
        else
        {
            controller.StartSequentialAttack(
                ExecuteSequential(attackable)
            );
        }

        return true;
    }
}