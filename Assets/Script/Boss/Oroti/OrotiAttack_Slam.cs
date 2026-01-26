using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Attack/Slam")]
public class OrotiAttack_Slam : OrotiAttackBase
{
    [Header("Allowed Neck")]
    [SerializeField] private NeckAttackType allowedNeck = NeckAttackType.Slam;

    [Header("Animation")]
    [SerializeField] private string animName = "Oroti_Slam";

    [Header("Damage Timing")]
    [SerializeField, Range(0f, 1f)]
    private float damageStart = 0.4f;

    [SerializeField, Range(0f, 1f)]
    private float damageEnd = 0.5f;

    public override void Execute(
        List<OrotiNeck> necks,
        Transform player)
    {
        foreach (var neck in necks)
        {
            // ‘Î‰ž‚µ‚Ä‚¢‚È‚¢Žñ‚Í–³Ž‹
            if (neck.Type != allowedNeck)
                continue;

            neck.PlayAttack(
                animName,
                damageStart,
                damageEnd
            );
        }
    }
}