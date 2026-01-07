using UnityEngine;

public class TargetingEnemy : EnemyBase
{
    [Header("Target")]
    [SerializeField] protected GameObject TargetObject;

    public GameObject Target
    {
        get { return TargetObject; }
        set { TargetObject = value; }
    }
}