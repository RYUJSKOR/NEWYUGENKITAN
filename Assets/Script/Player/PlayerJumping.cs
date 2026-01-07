using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PlayerJumping : IPlayerState
{
    private Player player;
    private PlayerStateMachine playerStateMachine;
    private Rigidbody rb;
    private Animator animator;
    private DemonAnimation demonAnimation;

   [Header("ジャンプ設定")]
    public float jumpForce = 21.5f;

    [Header("重力補正")]
    public float fallMultiplier = 2.7f;
    public float ascentMultiplier = 2.7f;

    private bool isJumping = false;

    public void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;
        rb = player.GetComponent<Rigidbody>();
        animator = player.GetComponent<Animator>();
        demonAnimation = player.GetComponent<DemonAnimation>();

        // 重力の強さを設定
        Physics.gravity = new Vector3(0, -20f, 0);
        isJumping = false;

        animator?.SetBool("IsJumping", false);
    }

    public void FixedUpdate()
    {
        // このステートではFixedUpdateで何もしない
    }

    public void HandleInput()
    {
        var crouchState = playerStateMachine.GetState<PlayerCrouching>();
        if (!isJumping && playerStateMachine.JumpPressed && player.IsGrounded())
        {
            if (crouchState != null && crouchState.GetCrouching())
            {
                crouchState.StopCrouch();
            }

            if (crouchState == null || !crouchState.GetCrouching())
            {
                Jump(jumpForce);
                demonAnimation.OnJumpStart();
                isJumping = true;

                animator.SetTrigger("IsJumping");
            }
        }
    }

    public void Update()
    {
        if (!isJumping) return;

        // 接地した瞬間にジャンプ終了 → 硬直を無くす
        if (player.IsGrounded())
        {
            isJumping = false;
            animator.SetBool("IsJumping", false);
            playerStateMachine.DeactivateState(this);
            return;
        }

        demonAnimation.UpdateJumpTrail(isJumping, player.transform);

        // 空中にいる間だけジャンプ補正を行う
        JumpCorrection();
    }

    public void Remove()
    {
        isJumping = false;
        animator.SetBool("IsJumping", false);
    }

    private void Jump(float force)
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = force;
        rb.linearVelocity = velocity;
        animator.SetBool("IsJumping", true);
    }

    private void JumpCorrection()
    {
        // Grounded の場合は補正不要
        if (player.IsGrounded()) return;

        if (rb.linearVelocity.y < 0)
        {
            // 落下を早める（キビキビ感）
            rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1f) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0)
        {
            if (playerStateMachine.JumpReleased)
            {
                // ジャンプボタンを離したら上昇速度を減少（ショートジャンプ）
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f, rb.linearVelocity.z);
            }
            else
            {
                // 上昇中の補正
                rb.linearVelocity += Vector3.up * Physics.gravity.y * (ascentMultiplier - 1f) * Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// トランポリンから通知されたときの処理
    /// </summary>
    public void OnTrampolineBounce(float bounceForce)
    {
        Jump(bounceForce); // 通常ジャンプより強めの力でジャンプ

        if (!isJumping)
        {
            isJumping = true;
            playerStateMachine.ActivateState(this);
        }

        animator.SetBool("IsJumping", true);
        animator.SetTrigger("IsJumping");
    }

}
