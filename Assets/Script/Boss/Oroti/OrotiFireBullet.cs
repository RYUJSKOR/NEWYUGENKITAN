using UnityEngine;

public class OrotiFireBullet : OrotiBulletBase
{
    protected override void OnGroundHit(Collider other)
    {
        Explosion();
        Destroy(gameObject);
    }

    private void Explosion()
    {
        Vector3 spawnPos = GetBottomPosition();

        if (setting.explosionEffect != null)
        {
            var fx = Instantiate(
                setting.explosionEffect,
                spawnPos,
                Quaternion.identity
            );
            Destroy(fx, 2f);
        }

        Collider[] hits = Physics.OverlapSphere(
            spawnPos,
            setting.explosionRadius
        );

        foreach (var hit in hits)
        {
            if (hit.gameObject == Owner) continue;

            var health = hit.GetComponent<CharacterHealthManager>();
            if (health != null)
            {
                health.ApplyDamage(setting.power);
            }
        }
    }
}
