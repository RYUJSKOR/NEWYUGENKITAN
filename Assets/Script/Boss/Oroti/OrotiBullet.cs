using UnityEngine;

public class OrotiBullet : Bullet
{
    private Vector3 direction;
    private float speed;
    private bool homing;
    private Transform target;

    public void Initialize(
        Vector3 dir,
        GameObject owner,
        OrotiBulletSetting setting,
        Transform targetTransform)
    {
        direction = dir.normalized;
        speed = setting.speed;
        homing = setting.homing;
        target = targetTransform;

        SetOwner(owner);
        SetPower(setting.power);
    }

    protected override void Update()
    {
        base.Update();

        if (homing && target != null)
        {
            direction = (target.position - transform.position).normalized;
        }

        transform.position += direction * speed * Time.deltaTime;
    }
}
