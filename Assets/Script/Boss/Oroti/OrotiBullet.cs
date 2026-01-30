using UnityEngine;

public class OrotiBullet : Bullet
{
    [SerializeField] private float speed = 8f;

    private Vector3 direction;

    public void Initialize(
        Vector3 dir,
        GameObject owner,
        float powerOverride)
    {
        direction = dir.normalized;

        SetOwner(owner);
        SetPower(powerOverride);
    }

    protected override void Update()
    {
        base.Update();

        transform.position += direction * speed * Time.deltaTime;
    }
}
