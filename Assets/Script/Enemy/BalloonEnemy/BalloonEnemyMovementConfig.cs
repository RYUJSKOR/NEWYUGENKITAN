using UnityEngine;

[CreateAssetMenu(fileName = "BalloonEnemyConfig", menuName = "Enemy/BalloonEnemyConfig", order = 1)]
public class BalloonEnemyMovementConfig : ScriptableObject
{
    public Vector3 moveDirection = Vector3.forward;
    public float moveSpeed = 2f;
    public GameObject bullet;
    [TagSelector]
    public string[] hitIgnoreTags;

    public BalloonEnemy.PatternType pattern = BalloonEnemy.PatternType.Hexagon;

    public float explosionScaleMultiplier = 2f;
    public float explosionDuration = 0.3f;
    public float peakScaleFactor = 1.1f;
    public float peakDuration = 0.05f;
    public float destroyDelay = 0.2f;

    public float floatStrength = 0.5f;
    public float floatSpeed = 1f;
}
