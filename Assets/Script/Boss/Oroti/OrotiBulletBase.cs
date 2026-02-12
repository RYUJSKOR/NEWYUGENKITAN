using UnityEngine;

public abstract class OrotiBulletBase : Bullet
{
    protected Vector3 direction;
    protected Transform target;
    protected OrotiBulletSetting setting;

    protected float speed;

    public virtual void Initialize(
        Vector3 dir,
        GameObject owner,
        OrotiBulletSetting setting,
        Transform target)
    {
        this.setting = setting;
        this.target = target;
        direction = dir.normalized;
        speed = setting.speed;

        SetOwner(owner);
        SetPower(setting.power);
        SetLifeTime(setting.lifeTime);
    }

    protected override void Update()
    {
        base.Update();
        Move();
    }

    protected virtual void Move()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        OnHit();
    }

    protected abstract void OnHit();
}