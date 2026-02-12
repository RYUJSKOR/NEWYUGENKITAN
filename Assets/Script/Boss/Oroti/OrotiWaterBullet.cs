using UnityEngine;

public class OrotiWaterBullet : OrotiBulletBase
{
    protected override void OnGroundHit(Collider other)
    {
        Vector3 spawnPos = GetBottomPosition();

        if (setting.remainPrefab != null)
        {
            var obj = Instantiate(
                setting.remainPrefab,
                spawnPos,
                Quaternion.identity
            );

            Destroy(obj, setting.remainTime);
        }

        Destroy(gameObject);
    }
}
