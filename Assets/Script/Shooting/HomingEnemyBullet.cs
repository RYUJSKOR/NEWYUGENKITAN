using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingEnemyBullet : Bullet
{
    private GameObject targetObject;
    private Rigidbody rb;

    [Header("弾の性能")]
    [SerializeField] private float homingStrength = 20f; // 追尾の強さ

    private float speed; // 弾の速さを保持する変数

    new void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
    }

    // 外部から速度を設定するための public メソッド
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void FixedUpdate()
    {
        if (targetObject == null)
        {
            // ターゲットがいない場合は、設定された速度で直進し続けるように調整
            if (rb.linearVelocity.magnitude > 0)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * speed;
            }
            return; // ターゲットがいない場合は直進
        }

        // 速度が非常に遅い場合は何もしない（エラー防止）
        if (speed < 0.1f)
        {
            return;
        }

        // --- 速度を維持する新しい追尾ロジック ---
        // ターゲットへの方向ベクトル
        Vector3 targetDirection = (targetObject.transform.position - transform.position).normalized;

        // 現在の進行方向からターゲットの方向へ、homingStrengthに応じて向きを変える
        // 第2引数で目標の速度ベクトル（方向はターゲット、大きさは保持しているspeed）を指定する
        Vector3 newVelocity = Vector3.RotateTowards(
            rb.linearVelocity,
            targetDirection * speed, // 目標の速度ベクトル
            homingStrength * Time.fixedDeltaTime,
            0.0f
        );

        // 計算された新しい速度をRigidbodyに適用
        rb.linearVelocity = newVelocity;

        // 弾の向きを進行方向に合わせる
        if (rb.linearVelocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    public void SetTarget(GameObject target)
    {
        targetObject = target;
    }
}