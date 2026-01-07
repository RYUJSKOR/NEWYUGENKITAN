using UnityEngine;

public class Enemy : EnemyBase
{
    [SerializeField] private float shootInterval = 2f; // 2•b‚²‚Æ‚É”­ŽË
    private float shootTimer = 0f;

    private Shooting shooting;

    new void Start()
    {
        base.Start();
        shooting = GetComponent<Shooting>();
    }

    new void Update()
    {
        if (shooting == null) return;

        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval)
        {
            shooting.RequestShoot();
            shootTimer = 0f;
        }
    }
}
