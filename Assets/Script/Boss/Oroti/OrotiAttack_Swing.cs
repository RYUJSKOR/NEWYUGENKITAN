using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Swing")]
public class OrotiAttack_Swing : OrotiAttackBase
{
    [Header("Swing Damage Override")]
    [SerializeField] private float damage = 1f;

    public override bool Execute(
    List<OrotiNeck> allNecks,
    Transform player,
    OrotiController controller
    )
    {
        Debug.Log("OrotiAttack_Swing");
        // 首選択
        var selected = SelectNecks(allNecks);

        // 攻撃可能な首のみ
        selected = FilterAttackable(selected);

        if (selected.Count == 0)
            return false;

        // ダメージを首に設定
        foreach (var neck in selected)
        {
            neck.SetSwingDamage(damage);
        }

        // 同時 or 順番実行
        return ExecuteByOrder(
            selected,
            player,
            controller,
            OrotiAttackType.Swing
        );
    }
}
