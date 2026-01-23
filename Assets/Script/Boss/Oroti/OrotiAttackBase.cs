using System.Collections.Generic;
using UnityEngine;

public abstract class OrotiAttackBase : ScriptableObject
{
    [SerializeField] protected int useNeckCount = 1;
    public int UseNeckCount => useNeckCount;

    public abstract void Execute(
        List<OrotiNeck> necks,
        Transform player
    );
}