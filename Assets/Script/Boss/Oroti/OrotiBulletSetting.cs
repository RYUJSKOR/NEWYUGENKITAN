using UnityEngine;

[CreateAssetMenu(menuName = "Oroti/Bullet Setting")]
public class OrotiBulletSetting : ScriptableObject
{
    public float speed = 8f;
    public float power = 1f;
    public bool homing;
}