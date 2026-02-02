using UnityEngine;
using System.Collections;

public class BlueBoundEnemyMovement : MonoBehaviour
{
    // --- Publicな変数やプロパティ ---
    [Header("Jump Settings")]
    public float jumpForce = 10f;
    public float moveForce = 3f;
    public float crouchDuration = 0.4f;

    [Header("Crouch Settings")]
    public float crouchScaleY = 0.5f;

    [Header("Wall Detection")]
    public float wallDetectionDistance = 1f;

    [Header("State Durations")]
    public float idleDuration = 1.5f;
    public float stuckDuration = 1.0f;

    // --- コンポーネントへの参照 ---
    public Rigidbody Rb { get; private set; }
    public GameObject TargetObject { get; private set; }
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    public Vector3 OriginalScale { get; private set; }

    // --- 速度制御 ---
    public float CurrentSpeedModifier { get; private set; } = 1.0f;

    // --- ステート管理 ---
    private BaseState currentState;
    public IdleState idleState;
    public PreparingState preparingState;
    public JumpingState jumpingState;
    public StuckRecoveryState stuckRecoveryState;
    private SEController SE;

    // --- 内部変数 ---
    private float defaultMaxAngularVelocity;

    // --- ギズモ表示用のプロパティ ---
    public bool IsCurrentlyGrounded { get; private set; }


    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        OriginalScale = transform.localScale;

        // ゲーム開始時の最大回転速度を記憶しておく
        defaultMaxAngularVelocity = Rb.maxAngularVelocity;

        idleState = new IdleState(this);
        preparingState = new PreparingState(this);
        jumpingState = new JumpingState(this);
        stuckRecoveryState = new StuckRecoveryState(this);
    }

    void Start()
    {
        currentState = idleState;
        currentState.EnterState();
        SE = GetComponent<SEController>();
    }

    void Update()
    {
        if (TargetObject == null) return;
        IsCurrentlyGrounded = IsGrounded();
        currentState?.UpdateState();
    }

    void FixedUpdate()
    {
        // 現在のステートのFixedUpdateStateを呼び出す
        currentState?.FixedUpdateState();
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            if (Application.isPlaying && IsCurrentlyGrounded) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }

    /// <summary>
    /// 外部から速度倍率を設定し、回転速度にも反映させる
    /// </summary>
    public void SetSpeedModifier(float modifier)
    {
        CurrentSpeedModifier = modifier;

        // 速度倍率に合わせて、最大回転速度も変更する
        if (Rb != null)
        {
            Rb.maxAngularVelocity = defaultMaxAngularVelocity * CurrentSpeedModifier;
        }
    }

    public void ChangeState(BaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public IEnumerator CrouchCoroutine()
    {
        transform.localScale = new Vector3(OriginalScale.x, crouchScaleY, OriginalScale.z);
        yield return new WaitForSeconds(crouchDuration / CurrentSpeedModifier);
        transform.localScale = OriginalScale;
        PerformJump();
    }

    /// <summary>
    /// ジャンプの勢いが常に一定になるように修正
    /// </summary>
    private void PerformJump()
    {
        if (TargetObject == null) { ChangeState(idleState); return; }
        Vector3 toPlayer = (TargetObject.transform.position - transform.position).normalized;
        toPlayer.y = 0f;

        if (Physics.Raycast(transform.position, toPlayer, wallDetectionDistance, wallLayer))
        {
            ChangeState(stuckRecoveryState);
        }
        else
        {
            Vector3 direction = (toPlayer * moveForce + Vector3.up * jumpForce).normalized;
            float totalJumpPower = jumpForce;
            Rb.linearVelocity = Vector3.zero;
            Rb.AddForce(direction * totalJumpPower * CurrentSpeedModifier, ForceMode.VelocityChange);
            Rb.useGravity = false;
            ChangeState(jumpingState);
            SE.Play("Enemy.BoundJump");
        }
    }

    public bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics.CheckSphere(groundCheck.position, 0.2f, groundLayer);
    }

    public void SetTargetObject(GameObject target) { TargetObject = target; }
    public void SetMovementConfig(BlueBoundMovementConfig config) { /* ... */ }
}