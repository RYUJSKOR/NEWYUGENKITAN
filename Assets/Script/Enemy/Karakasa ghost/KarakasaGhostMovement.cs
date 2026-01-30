using UnityEngine;
using System.Collections;

public class KarakasaGhostMovement : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float bounceUpForce = 10f;
    [SerializeField] private float floatFallSpeed = 1f;
    [SerializeField] private float swingSpeed = 0.5f;
    [SerializeField] private float swingRange = 0.2f;
    private float swingTimer = 0f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpInterval = 2f;
    [SerializeField] private float crouchDuration = 0.4f;
    private float nextJumpTime;
    private TargetingEnemy targetingEnemy;
    private GroundCheck groundChecker;
    private Vector3 originalScale;
    private SEController SE;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchScaleY = 0.5f;

    private bool isFalling = true;
    private bool hasBounced = false;
    private bool isChargingJump = false;
    private bool justJumped = false;
    private float justJumpedTime = 0f;
    [SerializeField] private float groundedIgnoreTime = 0.4f;

    // ↓ --- ここから追加 ---
    private float currentSpeedModifier = 1.0f; // 現在の速度倍率
    // ↑ --- ここまで追加 ---

    public bool IsActuallyGrounded =>
        groundChecker != null && groundChecker.IsGrounded && !JustJumped();

    private void Start()
    {
        SE = GetComponent<SEController>();
    }

    public bool JustJumped()
    {
        // ジャンプ直後の地面無視時間も速度の影響を受けるようにする
        return justJumped && (Time.time - justJumpedTime < groundedIgnoreTime / currentSpeedModifier);
    }

    // ↓ --- ここから追加 ---
    /// <summary>
    /// 外部から速度倍率を設定するためのメソッド
    /// </summary>
    public void SetSpeedModifier(float modifier)
    {
        currentSpeedModifier = modifier;
    }
    // ↑ --- ここまで追加 ---

    public void Initialize(TargetingEnemy enemy)
    {
        targetingEnemy = enemy;
        isFalling = true;
        hasBounced = false;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = false;
            }
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // nextJumpTimeの初期設定も速度倍率を考慮
        nextJumpTime = Time.time + (jumpInterval / currentSpeedModifier);

        groundChecker = GetComponentInChildren<GroundCheck>();
        originalScale = transform.localScale;
        isChargingJump = false;
    }

    void FixedUpdate()
    {
        if (targetingEnemy == null) return;

        if (isFalling && !hasBounced)
        {
            // 落下速度に倍率を適用
            rb.linearVelocity = Vector3.down * floatFallSpeed * currentSpeedModifier;
            rb.angularVelocity = Vector3.zero;

            // 揺れの速度とタイミングに倍率を適用
            swingTimer += Time.deltaTime * swingSpeed * currentSpeedModifier;
            float horizontalOffset = Mathf.Sin(swingTimer) * swingRange;
            rb.linearVelocity = new Vector3(horizontalOffset, rb.linearVelocity.y, rb.linearVelocity.z);
        }
        else if (IsActuallyGrounded && targetingEnemy?.Target != null)
        {
            // 次のジャンプまでの時間も速度倍率の影響を受ける
            if (!isChargingJump && Time.time >= nextJumpTime)
            {
                StartCoroutine(JumpSequence());
            }
        }
    }

    private IEnumerator JumpSequence()
    {
        isChargingJump = true;
        rb.linearVelocity = Vector3.zero;

        Vector3 crouchScale = new Vector3(originalScale.x, crouchScaleY, originalScale.z);
        float heightDiff = (originalScale.y - crouchScaleY) / 2f;
        transform.localScale = crouchScale;
        transform.position -= new Vector3(0f, heightDiff, 0f);

        // しゃがみ時間に倍率を適用 (遅いほど長く待つ)
        yield return new WaitForSeconds(crouchDuration / currentSpeedModifier);

        transform.position += new Vector3(0f, heightDiff * 1.1f, 0f); // 少し余裕
        transform.localScale = originalScale;
        transform.position += new Vector3(0f, 0.1f, 0f);

        JumpTowardsTarget();

        isChargingJump = false;
        // 次のジャンプまでのインターバルに倍率を適用
        nextJumpTime = Time.time + (jumpInterval / currentSpeedModifier);
    }

    private void JumpTowardsTarget()
    {
        if (targetingEnemy.Target == null) return;

        Vector3 toTarget = (targetingEnemy.Target.transform.position - transform.position);
        toTarget.y = 0f;
        Vector3 jumpDir = (toTarget.normalized + Vector3.up).normalized;

        rb.linearVelocity = Vector3.zero;
        // ジャンプ力に倍率を適用
        rb.AddForce(jumpDir * jumpForce * currentSpeedModifier, ForceMode.VelocityChange);

        SE.Play("Enemy.KasaJump");

        justJumped = true;
        justJumpedTime = Time.time;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFalling && !hasBounced && collision.gameObject.CompareTag("Ground"))
        {
            isFalling = false;
            hasBounced = true;
            // バウンド力に倍率を適用
            rb.linearVelocity = Vector3.up * bounceUpForce * currentSpeedModifier;
            // Invokeの遅延時間にも倍率を適用
            Invoke(nameof(StartFloatingFall), 0.2f / currentSpeedModifier);
        }
    }

    private void StartFloatingFall()
    {
        isFalling = true;
        hasBounced = false;
        swingTimer = 0f;
        isChargingJump = false;
    }

    public void SetConfig(KarakasaMovementConfig config)
    {
        if (config == null) return;
        bounceUpForce = config.bounceUpForce;
        floatFallSpeed = config.floatFallSpeed;
        swingSpeed = config.swingSpeed;
        swingRange = config.swingRange;
        jumpForce = config.jumpForce;
        jumpInterval = config.jumpInterval;
        crouchDuration = config.crouchDuration;
        crouchScaleY = config.crouchScaleY;
        groundedIgnoreTime = config.groundedIgnoreTime;
    }
}