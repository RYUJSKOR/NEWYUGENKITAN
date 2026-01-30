using UnityEngine;

public class BalloonEnemy : EnemyBase
{
    [SerializeField] private BalloonEnemyMovementConfig config;
    [SerializeField] private Shooting shootingComponent;
    private BalloonEnemyMovement movement;
    private Rigidbody rb;
    private bool isDead = false;

    // --- 追加ここから ---
    [Header("Lifetime Settings")]
    [Tooltip("この秒数が経過すると、敵は爆発せずに消滅します。")]
    [SerializeField] private float lifetime = 10f; // 敵が消えるまでの時間（秒）
    // --- 追加ここまで ---


    [Header("Audio Settings")]
    [SerializeField] private AudioClip inflateSound; // 膨らむ音
    [SerializeField] private AudioClip explodeSound; // 爆発音
    private AudioSource audioSource; // 音を再生するコンポーネント
    private SEController SE;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<BalloonEnemyMovement>();
        healthManager = GetComponent<CharacterHealthManager>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
        }
    }

    new void Start()
    {
        if (movement == null)
        {
            movement = gameObject.AddComponent<BalloonEnemyMovement>();
        }

        if (config == null)
        {
            Debug.LogError("[BalloonEnemy] Config が設定されていません！");
            enabled = false;
            return;
        }

        if (movement != null)
        {
            movement.Initialize(config);
        }

        if (shootingComponent != null && ShootingManager.Instance != null)
        {
            shootingComponent.SetBulletByName(config.bullet.name);
        }

        if (healthManager != null)
        {
            healthManager.OnDeath += OnDeath;
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        SE = GetComponent<SEController>();

        // --- 追加ここから ---
        // lifetime秒後にVanishメソッドを呼び出すように予約する
        Invoke("Vanish", lifetime);
        // --- 追加ここまで ---
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (config == null) return;

        foreach (string tag in config.hitIgnoreTags)
        {
            if (collision.gameObject.CompareTag(tag))
                return;
        }

        if (healthManager != null)
        {
            healthManager.ApplyDamage(healthManager.GetHealth());
        }
        else
        {
            Explode();
        }
    }

    private void OnDeath()
    {
        // --- 変更ここから ---
        // Invokeで予約したVanishが呼ばれないようにキャンセルする
        CancelInvoke("Vanish");
        // --- 変更ここまで ---
        Explode();
    }

    new private void OnDestroy()
    {
        base.OnDestroy();

        if (healthManager != null)
        {
            healthManager.OnDeath -= OnDeath;
        }
    }


    override protected void Explode()
    {
        if (isDead) return;
        isDead = true;

        CancelInvoke("Vanish");

        if (rb != null) rb.linearVelocity = Vector3.zero;
        if (movement != null) movement.Freeze();

        // 既存のAudioSource（膨らむ音など）は止める
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // 膨らむ瞬間の演出音があれば再生
        if (inflateSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(inflateSound);
        }

        Vector3 finalScale = transform.localScale * config.explosionScaleMultiplier;
        Vector3 peakScale = finalScale * config.peakScaleFactor;

        // 膨らむアニメーション開始
        LeanTween.scale(gameObject, peakScale, config.explosionDuration)
            .setOnComplete(() =>
            {
                LeanTween.delayedCall(config.peakDuration, () =>
                {
                    LeanTween.scale(gameObject, finalScale, 0.1f)
                        .setOnComplete(() =>
                        {
                            // --- ここから死亡確定時の処理 ---

                            // 1. 弾を発射
                            EmitBulletsInPattern(config.pattern);

                            // 2. 見た目と当たり判定を先に消す (Visuals OFF)
                            MeshRenderer[] allRenderers = GetComponentsInChildren<MeshRenderer>();
                            foreach (MeshRenderer renderer in allRenderers)
                            {
                                renderer.enabled = false;
                            }

                            var collider = GetComponent<Collider>();
                            if (collider != null) collider.enabled = false;
                            if (rb != null) rb.isKinematic = true;

                            // 3. 爆発SEを再生し、長さを取得 (Play Sound & Get Duration)
                            float soundDuration = 0.0f;

                            // SEController優先
                            if (SE != null)
                            {
                                soundDuration = SE.Play("Enemy.BalloonDie");
                            }
                            // SEControllerがない場合はAudioClipを使用
                            else if (explodeSound != null && audioSource != null)
                            {
                                audioSource.PlayOneShot(explodeSound);
                                soundDuration = explodeSound.length;
                            }

                            // 4. 音が鳴り終わるまで待ってから完全に削除 (Destroy Delay)
                            // soundDurationが0の場合は最低0.5秒待つ
                            Destroy(gameObject, soundDuration > 0 ? soundDuration : 0.5f);
                        });
                });
            });
    }

    // --- 追加ここから ---
    /// <summary>
    /// 敵が爆発せずに消える処理
    /// </summary>
    private void Vanish()
    {
        // 既に死んでいる（爆発処理中など）場合は何もしない
        if (isDead) return;
        isDead = true;

        // 物理的な挙動や移動を止める
        if (movement != null) movement.Freeze();
        if (rb != null)
        {
            rb.isKinematic = true; // 物理演算を無効化
            rb.linearVelocity = Vector3.zero;
        }

        // 当たり判定を無くす
        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // 音を止める
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // 0.5秒かけて徐々に小さくなり、完了後にGameObjectを破壊する
        float vanishDuration = 0.5f;
        LeanTween.scale(gameObject, Vector3.zero, vanishDuration)
            .setEase(LeanTweenType.easeInBack) // 少し吸い込まれるような動きで消える
            .setOnComplete(() =>
            {
                Destroy(gameObject);
            });
    }
    // --- 追加ここまで ---

    private void EmitBulletsInPattern(PatternType pattern)
    {
        if (shootingComponent == null) return;

        int bulletCount = (pattern == PatternType.Hexagon) ? 6 : 10;
        float angleStep = 360f / bulletCount;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            if (pattern == PatternType.Star && i % 2 == 1)
                angle += angleStep / 2f;

            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
            shootingComponent.RequestShoot(dir);
        }
    }

    public enum PatternType { Hexagon, Star }
}