using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Bullet Setting")]
public class OrotiBulletSetting : ScriptableObject
{
    [Header("Base")]
    public float speed = 8f;
    public float power = 1f;
    public float lifeTime = 5f;

    [Header("Evolution")]
    public BulletEvolutionType evolutionType;

    [Header("Explosion")]
    public float explosionRadius = 4f;
    public float explosionPower = 1.5f;
    public GameObject explosionEffect;

    [Header("Homing")]
    public float homingStrength = 0f;

    [Header("Remain")]
    public GameObject remainPrefab;
    public float remainTime = 3f;
}

public enum BulletEvolutionType
{
    None,
    Explosion,
    Homing,
    Remain
}
