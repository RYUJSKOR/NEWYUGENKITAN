using TMPro;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class FoxBullet : Bullet
{
    private Shooting shooting;

    [Header("ブーメラン設定")]
    [SerializeField] private float returnSpeed = 15f;
    [SerializeField] private float maxDistance = 15f;

    private Vector3 startPosition;
    private bool isReturning = false;
    private Transform ownerTransform;

    private Rigidbody rb;

    new void Start()
    {
        base.Start();

        // Rigidbodyを取得
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody が FoxBullet にアタッチされていません。");
        }

        startPosition = transform.position;
        if (Owner != null)
        {
            ownerTransform = Owner.transform;
            //shooting = Owner.GetComponent<Shooting>();
        }
    }

    new void Update()
    {
        // 戻るフェーズに切り替える判定はそのまま残す
        if (!isReturning && Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            isReturning = true;
        }

        // 弾の進行方向を常に更新
        Vector3 currentDirection = rb.linearVelocity.normalized;

        // X軸を進行方向に向け、Y/Z軸はカメラを向く回転を計算
        if (Camera.main != null && currentDirection != Vector3.zero)
        {
            Vector3 right = currentDirection;
            Vector3 forward = Camera.main.transform.forward;
            forward = Vector3.ProjectOnPlane(forward, right).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;
            Quaternion newRotation = Quaternion.LookRotation(forward, up);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 10f);
        }
    }

    // FixedUpdate で物理演算を制御
    void FixedUpdate()
    {
        if (rb == null) return;

        if (!isReturning)
        {
            // 前進時の速度を設定
            //rb.linearVelocity = transform.forward * shooting.GetBulletSpeed();
        }
        else
        {
            // プレイヤーに向かって戻る
            if (ownerTransform != null)
            {
                Vector3 direction = (ownerTransform.position - transform.position).normalized;
                rb.linearVelocity = direction * returnSpeed;

                // プレイヤーに戻ったら弾を消す
                if (Vector3.Distance(ownerTransform.position, transform.position) < 1f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    protected override void OnTriggerEnter(Collider collision)
    {
        // 無視リストにあるタグならスルー
        if (ignoreTags.Contains(collision.tag) || collision.gameObject == Owner) return;

        // 攻撃処理
        var target = collision.GetComponent<CharacterHealthManager>();
        if (target != null && collision.gameObject != Owner)
        {
            target.ApplyDamage(power); // TakeDamage から ApplyDamage に変更
        }

        // 爆発オブジェクト生成
        if (ExplosionObject != null)
        {
            GameObject obj = Instantiate(ExplosionObject, transform.position, Quaternion.identity);
            Destroy(obj, 2);
        }
    }


    public void Initialize(GameObject owner)
    {
        SetOwner(owner);
        ownerTransform = owner.transform;
        // Rigidbodyがなければ追加（念のため）
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // 物理影響は受けない
            rb.useGravity = false;
        }
    }
}
