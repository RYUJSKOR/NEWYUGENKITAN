using UnityEngine;

public abstract class OrotiBulletBase : Bullet
{
    protected Vector3 direction;
    protected Transform target;
    protected OrotiBulletSetting setting;

    protected float speed;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;
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
        if (other.gameObject.layer == LayerMask.NameToLayer("Boss"))
            return;

        base.OnTriggerEnter(other);

        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            OnPlayerHit(other);
        }
        else if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            OnGroundHit(other);
        }
    }
    protected virtual void OnPlayerHit(Collider other)
    {
        // 通常ダメージ（Bullet側で処理される想定）
        Destroy(gameObject);
    }

    protected virtual void OnGroundHit(Collider other)
    {
        Destroy(gameObject);
    }

    protected Vector3 GetBottomPosition()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            Bounds bounds = col.bounds;

            return new Vector3(
                transform.position.x,
                bounds.min.y,
                transform.position.z
            );
        }

        return transform.position;
    }
}