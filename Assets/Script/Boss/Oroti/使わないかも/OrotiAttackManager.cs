using System.Collections.Generic;
using UnityEngine;

public class OrotiAttackManager : MonoBehaviour
{
    [SerializeField] private List<OrotiAttackBase> allAttacks;

    public List<OrotiAttackBase> GetAvailableAttacks(float hpPercent)
    {
        int count = 4;

        if (hpPercent <= 0.75f) count = 6;
        if (hpPercent <= 0.4f) count = 8;

        count = Mathf.Min(count, allAttacks.Count);
        return allAttacks.GetRange(0, count);
    }
}
