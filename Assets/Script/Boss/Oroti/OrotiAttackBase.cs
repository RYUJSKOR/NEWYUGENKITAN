using UnityEngine;

public abstract class OrotiAttackBase : ScriptableObject
{
    public abstract void Execute(OrotiNeck neck, Transform player);
}

