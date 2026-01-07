using UnityEngine;

public class PlayerMoving : IPlayerState
{
    private Player player;
    private PlayerStateMachine playerStateMachine;
    private Animator animator;
    private Rigidbody rb;
    private SideScrollCamera camera;

    [Header("移動設定")]
    public float moveSpeed = 8f;
    public float airControlMultiplier = 1.0f;
    public float groundFriction = 15f;
    public float airFriction = 3f;

    private Vector2 moveInput;
    private bool facingRight = true;
    private float lastXInput = 1f; // デフォルトで右向き

    private ParticleSystem dustEffect;

    public void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;
        rb = player.GetComponent<Rigidbody>();
        animator = player.GetComponent<Animator>();
        camera = GameObject.Find("CameraPivot").GetComponent<SideScrollCamera>();

        // Rigidbody の回転制約（X,Z固定、Y自由）
        // Z軸の位置も固定に加える
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionZ;

        ParticleSystem[] particles = player.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particles)
        {
            if (ps.name == "eff_pfb_DustCloud")
            {
                dustEffect = ps;
                dustEffect.Stop();
                break;
            }
        }
    }

    public void FixedUpdate() { }

    public void HandleInput()
    {
        moveInput = playerStateMachine.MoveInput;

        // 入力があれば最後に押した方向を更新
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            lastXInput = Mathf.Sign(moveInput.x);
        }
    }

    public void Update()
    {
        var crouchState = playerStateMachine.GetState<PlayerCrouching>();
        if (crouchState != null && crouchState.GetCrouching())
        {
            animator.SetBool("IsWalking", false);

            // しゃがみ中も向きを保持
            ForceFacing(lastXInput);
            return;
        }

        Move();
        HandleDustEffect();
    }

    public void Remove()
    {
        animator.SetBool("IsWalking", false);
        if (dustEffect != null && dustEffect.isPlaying) dustEffect.Stop();
    }

    private void Move()
    {
        bool grounded = player.IsGrounded();
        float controlFactor = grounded ? 1f : airControlMultiplier;
        float friction = grounded ? groundFriction : airFriction;

        float xInput = moveInput.x;

        // 移動処理
        if (Mathf.Abs(xInput) < 0.01f)
        {
            if (grounded)
            {
                Vector3 stopVel = new Vector3(0f, rb.linearVelocity.y, 0f);
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, stopVel, friction * Time.deltaTime);
            }
            animator.SetBool("IsWalking", false);

            // 入力なしでも最後の方向で絶対に向きを反映
            ForceFacing(lastXInput);
            return;
        }

        // 横スクロール移動
        Vector3 targetVel = new Vector3(xInput * moveSpeed * controlFactor, rb.linearVelocity.y, 0f);
        rb.linearVelocity = new Vector3(
            Mathf.Lerp(rb.linearVelocity.x, targetVel.x, friction * Time.deltaTime),
            rb.linearVelocity.y,
            0f
        );

        animator.SetBool("IsWalking", true);

        // 入力方向で絶対に反転
        ForceFacing(Mathf.Sign(xInput));
    }

    private void ForceFacing(float dir)
    {
        float targetY = dir > 0 ? -90f : 90f;
        Quaternion targetRot = Quaternion.Euler(0f, targetY, 0f);

        if (Quaternion.Angle(rb.rotation, targetRot) > 0.1f)
        {
            rb.MoveRotation(targetRot);

            // 向きが変わった場合のみカメラ反転
            bool newFacingRight = dir > 0;
            if (newFacingRight != facingRight)
            {
                facingRight = newFacingRight;
                camera.FlipOffsetX();
            }
        }
    }

    private void HandleDustEffect()
    {
        if (dustEffect == null) return;

        // 接地している場合のみ Dust を表示
        bool grounded = player.IsGrounded();

        // Rigidbody の XZ 平面の速度で移動判定
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        bool isMoving = horizontalVel.magnitude > 0.01f;

        if (grounded && isMoving)
        {
            if (!dustEffect.isPlaying)
                dustEffect.Play();   // 移動中は常に表示
        }
        else
        {
            if (dustEffect.isPlaying)
                dustEffect.Stop();   // 空中や停止中は停止
        }
    }
}
