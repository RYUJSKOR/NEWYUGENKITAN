using UnityEngine;

public interface IBulletFactory
{
    GameObject CreateBullet(GameObject prefab, Vector3 position, Vector3 direction, Shooting shooter);
}
