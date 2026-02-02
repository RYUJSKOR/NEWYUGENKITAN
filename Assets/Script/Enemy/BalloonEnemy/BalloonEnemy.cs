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

		// --- 1. 物理干渉の即時排除 ---
		// 壁に張り付くバグを防ぐため、爆発処理が始まった瞬間にコライダを無効化します
		var collider = GetComponent<Collider>();
		if (collider != null) collider.enabled = false;

		// --- 2. 物理挙動の停止 ---
		if (rb != null)
		{
			rb.linearVelocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
			rb.isKinematic = true; // 物理演算を完全に無視して、その場に固定する
		}

		if (movement != null) movement.Freeze();

		// --- 3. 既存オーディオの停止と膨張音 ---
		if (audioSource != null) audioSource.Stop();

		if (inflateSound != null && audioSource != null)
		{
			audioSource.PlayOneShot(inflateSound);
		}

		Vector3 finalScale = transform.localScale * config.explosionScaleMultiplier;
		Vector3 peakScale = finalScale * config.peakScaleFactor;

		// --- 4. 膨張アニメーション ---
		LeanTween.scale(gameObject, peakScale, config.explosionDuration)
			.setOnComplete(() =>
			{
				LeanTween.delayedCall(config.peakDuration, () =>
				{
					LeanTween.scale(gameObject, finalScale, 0.1f)
						.setOnComplete(() =>
						{
							// --- 5. 爆発時の処理 ---

							// 弾幕パターン発射
							EmitBulletsInPattern(config.pattern);

							// 見た目を消す（コライダは既に消えているのでMeshだけ非表示にする）
							MeshRenderer[] allRenderers = GetComponentsInChildren<MeshRenderer>();
							foreach (MeshRenderer renderer in allRenderers)
							{
								renderer.enabled = false;
							}

							// --- 6. SE再生とオブジェクト破棄 ---
							float soundDuration = 0.5f; // デフォルトの待機時間（万が一音が鳴らなくても消えるように）

							try
							{
								// SEControllerがあれば再生を試みる
								if (SE != null)
								{
									float duration = SE.Play("Enemy.BalloonDie");
									// 正常に再生された場合のみ待機時間を更新
									if (duration > 0) soundDuration = duration;
								}
								// SEControllerがない場合、AudioClipを使用
								else if (explodeSound != null && audioSource != null)
								{
									audioSource.PlayOneShot(explodeSound);
									soundDuration = explodeSound.length;
								}
							}
							catch (System.Exception e)
							{
								// SE再生でエラーが起きてもゲーム進行を止めないためのログ
								Debug.LogWarning("[BalloonEnemy] SE Play Failed: " + e.Message);
							}

							// 音の長さ分待ってから破壊（SEエラーでも必ず実行される）
							Destroy(gameObject, soundDuration);
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