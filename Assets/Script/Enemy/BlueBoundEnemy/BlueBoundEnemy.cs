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
        if (movement != null)
        {
            movement.enabled = false;
        }
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }
        Explode();
        Destroy(gameObject);
    }
}