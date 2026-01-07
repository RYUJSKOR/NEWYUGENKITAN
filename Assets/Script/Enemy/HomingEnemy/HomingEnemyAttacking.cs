using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class HomingEnemyAttack : EnemyAttack
{
    private Bullet bullet;

    private GameObject TargetObject;

    /// <summary>
    /// HomingEnemyConfigに基づいて、発射する弾を設定します。
    /// </summary>
    public override void Initialize(ScriptableObject config)
    {
        // 受け取ったconfigをHomingEnemyConfigとしてキャスト
        var homingConfig = config as HomingEnemyConfig;
        if (homingConfig == null)
        {
            Debug.LogError("渡されたConfigがHomingEnemyConfigではありません。", this);
            return;
        }

        // Configで指定された弾のプレハブをShootingコンポーネントに設定
        if (homingConfig.BulletObject != null)
        {
            bullet = homingConfig.BulletObject.GetComponent<HomingEnemyBullet>();
            shooting.SetBulletByName(homingConfig.BulletObject.name);
            shooting.SetBulletSpeed(homingConfig.BulletSpeed);
            bullet.SetOwner(gameObject);
            bullet.SetLifeTime(homingConfig.LifeTime);
        }
        else
        {
            Debug.LogWarning("Configで弾が設定されていません。", this);
        }
    }

    /// <summary>
    /// 射撃システムに発射リクエストを送ります。
    /// </summary>
    public override void PerformAttack()
    {
        if (shooting == null || TargetObject == null)
        {
            return;
        }

        // ターゲットへの方向を計算
        Vector3 directionToTarget = (TargetObject.transform.position - transform.position).normalized;

        // 計算した方向で射撃をリクエストする
        shooting.RequestShoot(directionToTarget);
    }

    public override void SetTarget(GameObject target)
    {
        TargetObject = target;
    }
}