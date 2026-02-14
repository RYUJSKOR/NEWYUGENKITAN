using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Dive")]
public class OrotiAttack_Dive : OrotiAttackBase
{
    public override bool Execute(
            List<OrotiNeck> allNecks,
            Transform player,
            OrotiController controller)
    {
        // --------------------------------------------------
        // ① SelectType に基づいて首グループを選択
        // --------------------------------------------------
        var selected = SelectNecks(allNecks);

        // --------------------------------------------------
        // ② 現在攻撃可能な首のみ抽出
        //     (クールダウン中や死亡中を除外)
        // --------------------------------------------------
        var attackable = FilterAttackable(selected);

        if (attackable.Count == 0)
            return false;

        // --------------------------------------------------
        // ③ Priority適用
        //     ・NearestToPlayer
        //     ・Order
        //     などで並び替え
        // --------------------------------------------------
        attackable = ApplyPriority(attackable, player);

        // --------------------------------------------------
        // ④ RandomSelectCount適用
        //     並び替え後の上からX体抽出
        // --------------------------------------------------
        attackable = ApplySubset(attackable);

        if (attackable.Count == 0)
            return false;

        // --------------------------------------------------
        // ⑤ 攻撃順タイプに応じて実行
        // --------------------------------------------------
        switch (attackOrder)
        {
            // 同時に潜る
            case NeckAttackOrderType.Simultaneous:
                foreach (var neck in attackable)
                {
                    neck.PlayDive();
                }
                break;

            // 順番に潜る
            case NeckAttackOrderType.Sequential:
                controller.StartSequentialAttack(
                    SequentialDiveCoroutine(attackable)
                );
                break;
        }

        return true;
    }

    /// <summary>
    /// 順番に潜るコルーチン
    /// </summary>
    private IEnumerator SequentialDiveCoroutine(List<OrotiNeck> necks)
    {
        foreach (var neck in necks)
        {
            // 攻撃可能か最終チェック
            if (neck.CanAttack)
                neck.PlayDive();

            // 次の首までの間隔
            yield return new WaitForSeconds(0.4f);
        }
    }
}
