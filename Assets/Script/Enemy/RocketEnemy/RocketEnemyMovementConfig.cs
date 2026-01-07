using UnityEngine;

[CreateAssetMenu(fileName = "RocketEnemyMovementConfig", menuName = "Enemy/RocketEnemyMovementConfig")]
public class RocketEnemyMovementConfig : ScriptableObject
{
    public float hoverSpeed = 1f;
    public float attackDelay = 2f;
    public float attackSpeed = 15f;
    public float detectionRange = 2f;
}
