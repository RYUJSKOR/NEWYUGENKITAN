using UnityEngine;

[CreateAssetMenu(fileName = "HomingEnemyConfig", menuName = "Enemy/HomingEnemyConfig", order = 1)]
public class HomingEnemyConfig : ScriptableObject
{
    [Header("UŒ‚ŠÖŒW")]
    public GameObject BulletObject;
    public float BulletSpeed;
    public float LifeTime;

    [Header("ˆÚ“®ŠÖŒW")]
    public float JumpPower;
}
