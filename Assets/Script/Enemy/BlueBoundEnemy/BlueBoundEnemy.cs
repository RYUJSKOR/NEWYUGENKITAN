using UnityEngine;

public class BlueBoundEnemy : TargetingEnemy
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;

    [SerializeField] private BlueBoundMovementConfig config;

    private BlueBoundEnemyMovement movement;
    private SEController SE;

    new void Start()
    {
        base.Start();

        movement = GetComponent<BlueBoundEnemyMovement>();
        if (movement == null)
        {
            movement = gameObject.AddComponent<BlueBoundEnemyMovement>();
        }

        movement.SetTargetObject(TargetObject);
        movement.groundCheck = groundCheck;
        movement.groundLayer = groundLayer;
        movement.wallLayer = wallLayer;

        if (config != null)
        {
            movement.jumpForce = config.jumpForce;
            movement.moveForce = config.moveForce;
            movement.crouchDuration = config.crouchDuration;
            movement.crouchScaleY = config.crouchScaleY;
            movement.wallDetectionDistance = config.wallDetectionDistance;
        }

        // ↓ --- ここから追加 ---
        // ゲーム開始時に、インスペクターで設定された初期速度を移動コンポーネントに伝える
        if (movement != null)
        {
            movement.SetSpeedModifier(currentSpeedModifier);
        }
        // ↑ --- ここまで追加 ---

        if (healthManager != null)
        {
            healthManager.OnDeath += OnDeath;
        }

        SE = GetComponent<SEController>();
    }

    // ↓ --- ここから追加 ---
    /// <summary>
    /// 外部から呼ばれ、速度倍率を移動コンポーネントに伝える
    /// </summary>
    public override void ApplySpeedModifier(float modifier)
    {
        base.ApplySpeedModifier(modifier);

        if (movement != null)
        {
            movement.SetSpeedModifier(currentSpeedModifier);
        }
    }
    // ↑ --- ここまで追加 ---

    private void OnDeath()
    {
        // --- 1. 移動機能と物理演算の停止 ---
        if (movement != null)
        {
            movement.enabled = false;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 爆발이나 이동 관성이 남지 않도록 물리挙動を静止させる
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // --- 2. 外見と当たり判定の非表示・無効化 ---
        // SE再生中にオブジェクトが完全に消えないよう、見た目だけを先に消す
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }

        // 死亡後にプレイヤーと衝突しないように設定
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        // --- 3. 演出実行とSE再生 ---
        Explode(); // 既存の爆発演出

        float soundDuration = 1.0f; // デフォルトの待機時間
        if (SE != null)
        {
            // AudiostockなどのSE再生時間を取得
            soundDuration = SE.Play("Enemy.blueBoundEnemyDie");
        }

        // --- 4. SE再生完了後に破棄 ---
        // 音が途切れるのを防ぐため、再生完了を待ってからGameObjectを削除する
        Destroy(gameObject, soundDuration > 0 ? soundDuration : 0.1f);
    }
}