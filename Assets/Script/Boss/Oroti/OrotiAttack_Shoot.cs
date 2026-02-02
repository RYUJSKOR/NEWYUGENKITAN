using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Shoot")]
public class OrotiAttack_Shoot : OrotiAttackBase
{
    public override bool Execute(
            List<OrotiNeck> allNecks,
            Transform player,
            OrotiController controller)
    {
        var selected = SelectNecks(allNecks);
        if (selected.Count == 0)
            return false;

        foreach (var neck in selected)
        {
            neck.PlayShoot();
        }

        return true;
    }
}