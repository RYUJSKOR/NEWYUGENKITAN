using UnityEngine;

public class OrotiThunderBullet : OrotiBulletBase
{
    protected override void Move()
    {
        if (target != null)
        {
            Vector3 toTarget = (target.position - transform.position).normalized;

            direction = Vector3.Lerp(
                direction,
                toTarget,
                Time.deltaTime * setting.homingStrength
            ).normalized;
        }

        base.Move();
    }

    protected override void OnPlayerHit(Collider other)
    {
        Destroy(gameObject);
    }
}
