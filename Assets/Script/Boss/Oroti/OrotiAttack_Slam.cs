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
        // 首選択
        var selected = SelectNecks(allNecks);

        // 攻撃可能チェック
        var attackable = FilterAttackable(selected);

        // Priority + Order 実行
        return ExecuteByOrder(
            attackable,
            player,
            controller
        );
    }
}