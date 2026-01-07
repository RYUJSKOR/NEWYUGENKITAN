using UnityEngine;

public class DefaultBulletFactory : IBulletFactory
{
    public GameObject CreateBullet(GameObject prefab, Vector3 position, Vector3 direction, Shooting shooter)
    {
        var bullet = Object.Instantiate(prefab, position, Quaternion.LookRotation(direction));
        if (bullet.TryGetComponent(out Bullet bulletScript))
        {
            bulletScript.SetOwner(shooter.gameObject);
            bulletScript.SetIgnoreTags(shooter.GetIgnoreTags());
        }

        if (bullet.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = direction * shooter.GetBulletSpeed();
        }

        // もし弾がホーミング弾なら、発射主からターゲット情報を取得して設定する
        if (bullet.TryGetComponent(out HomingEnemyBullet homingBullet))
        {
            // 発射主（shooter）がターゲットを持っているか確認 (TargetingEnemyを継承しているか)
            if (shooter.TryGetComponent(out TargetingEnemy targetingEnemy))
            {
                homingBullet.SetTarget(targetingEnemy.Target);
            }

            // HomingEnemyBulletに、発射主の弾速を設定する
            homingBullet.SetSpeed(shooter.GetBulletSpeed());
        }

        return bullet;
    }
}