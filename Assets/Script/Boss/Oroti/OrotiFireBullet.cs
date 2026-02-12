using UnityEngine;

public class OrotiFireBullet : OrotiBulletBase
{
    protected override void OnHit()
    {
        Explosion();
        Destroy(gameObject);
    }

    private void Explosion()
    {
        if (setting.explosionEffect != null)
        {
            Instantiate(
                setting.explosionEffect,
                transform.position,
                Quaternion.identity
            );
        }

        var hits = Physics.OverlapSphere(
            transform.position,
            setting.explosionRadius
        );

        foreach (var hit in hits)
        {
            var health = hit.GetComponent<CharacterHealthManager>();
            if (health != null)
            {
                health.ApplyDamage(setting.power);
            }
        }
    }
}
