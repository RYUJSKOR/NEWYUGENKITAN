using UnityEngine;
using UnityEngine.InputSystem.XR;

public class PlayerDash : IPlayerState
{
    private Player player;
    private PlayerStateMachine PlayerStateMachine;
    private Rigidbody rb;
    private Animator animator;
    private SEController SEcontroller;

    [Header("ダッシュ設定")]
    public float dashSpeed = 15f;       // ダッシュのスピード
    public float dashDuration = 0.2f;   // ダッシュの継続時間（短めにすると "すっと" 感が出る）
    public float dashCooldown = 0.5f;   // クールダウン時間
    public float dashMaxDistance = 5f;  // ダッシュ最大距離（進みすぎ防止）

    private bool isDashing = false;
    private float dashStartTime = 0f;
    private float nextDashTime = 0f;
    private Vector3 dashDirection;
    private Vector3 dashStartPos;
    private bool isAirDashUsed = false;

    public void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.PlayerStateMachine = playerStateMachine;
        rb = player.GetComponent<Rigidbody>();
        animator = player.GetComponent<Animator>();
        animator?.SetBool("IsDashing", false);
		SEcontroller = player.GetComponent<SEController>();
	}

    public void FixedUpdate()
    {
        if (player.IsGrounded())
        {
            isAirDashUsed = false;
        }
    }

    public void HandleInput()
    {
        if (PlayerStateMachine.DashPressed && !PlayerStateMachine.IsCrouching &&
            Time.time >= nextDashTime && !isDashing)
        {
            float moveInput = PlayerStateMachine.HorizontalInput;

            if (moveInput != 0f)
            {
                if (player.IsGrounded() || !isAirDashUsed)
                {
                    if (!player.IsGrounded())
                        isAirDashUsed = true;

                    animator.SetBool("IsDashing", true);
					SEcontroller?.Play("Player.Dash");
					dashDirection = new Vector3(moveInput, 0f, 0f).normalized;
                    isDashing = true;
                    dashStartTime = Time.time;
                    nextDashTime = Time.time + dashCooldown;
                    dashStartPos = player.transform.position;

                    // ダッシュ中は重力を切る（水平専用）
                    rb.useGravity = false;
                }
            }
        }
    }

    public void Update()
    {
        if (isDashing)
        {
            float traveled = Vector3.Distance(dashStartPos, player.transform.position);

            // ダッシュ継続中かつ距離が制限内なら水平移動
            if (Time.time < dashStartTime + dashDuration && traveled < dashMaxDistance)
            {
                rb.linearVelocity = dashDirection * dashSpeed;
            }
            else
            {
                EndDash();
            }
        }
    }

    private void EndDash()
    {
        isDashing = false;
        rb.useGravity = true; // 重力復帰

        // ダッシュ後の横速度を調整（余韻を少しだけ残す）
        float horizontalSpeed = rb.linearVelocity.x;
        float maxPostDashSpeed = 8f; // ダッシュ後の最大横速度
        if (Mathf.Abs(horizontalSpeed) > maxPostDashSpeed)
            horizontalSpeed = Mathf.Sign(horizontalSpeed) * maxPostDashSpeed;

        rb.linearVelocity = new Vector3(horizontalSpeed, rb.linearVelocity.y, 0f);

        animator.SetBool("IsDashing", false);
        PlayerStateMachine.ActivateState(new PlayerMoving());
    }

    public void Remove()
    {
        isDashing = false;
        rb.useGravity = true; // 念のため復帰
        animator.SetBool("IsDashing", false);
    }
}
