using UnityEngine;
using System.Collections;

/// <summary>
/// 【最終修正版】プレイヤーの「しゃがみ」と「上から下へのすり抜け」を管理するステート。
/// 外部から参照されるpublicメソッドを追加。
/// </summary>
public class PlayerCrouching : IPlayerState
{
    private Player player;
    private Rigidbody rb;
    private PlayerStateMachine playerStateMachine;
    private CapsuleCollider capsuleCollider;
    private PlayerInputHandler inputHandler;
    private Animator animator;

    private float originalColliderHeight;
    private Vector3 originalColliderCenter;
    private bool isCurrentlyCrouching;
    [SerializeField] private float crouchingColliderHeight = 1.0f;
    [SerializeField] private float crouchSpeed = 10f;

    private Coroutine fallThroughCoroutine;

    // すり抜け時に加える下向きの力
    private float fallForce = 10f;

    /// <summary>
    /// 現在しゃがんでいるかどうかを外部に伝える
    /// </summary>
    public bool GetCrouching() => isCurrentlyCrouching;

    public void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.rb = player.GetComponent<Rigidbody>();
        this.playerStateMachine = playerStateMachine;
        this.capsuleCollider = player.GetComponent<CapsuleCollider>();
        this.inputHandler = playerStateMachine.InputHandler;
        this.animator = player.GetComponentInChildren<Animator>();

        if (this.capsuleCollider != null)
        {
            this.originalColliderHeight = capsuleCollider.height;
            this.originalColliderCenter = capsuleCollider.center;
        }
        animator?.SetBool("IsCrouching", false);
    }

    public void HandleInput()
    {
        // しゃがみボタンが押された最初のフレームに、すり抜けを試みる
        if (inputHandler.IsCrouching && player.IsGrounded() && fallThroughCoroutine == null)
        {
            TryFallThrough();
        }
    }

    public void FixedUpdate()
    {
        // isCurrentlyCrouchingフラグはDoCrouch/DoStand内で更新される
        if (inputHandler.IsCrouching)
        {
            DoCrouch();
        }
        else
        {
            DoStand();
        }
        animator?.SetBool("IsCrouching", isCurrentlyCrouching);
    }

    private void TryFallThrough()
    {
        if (Physics.Raycast(player.transform.position, Vector3.down, out RaycastHit hit, 1f, player.groundLayer))
        {
            Collider hitCollider = hit.collider;
            Transform platformParent = hitCollider.transform.parent;

            // ★修正: GetComponentInChildrenの引数に<OneWayPlatform3D>を追加
            OneWayPlatform3D platformScript = (platformParent != null) ? platformParent.GetComponentInChildren<OneWayPlatform3D>() : null;

            if (platformScript != null)
            {
                // ★修正: 見つけたスクリプトと、当たり判定のあるコライダーをコルーチンに渡す
                fallThroughCoroutine = player.StartCoroutine(FallThroughRoutine(hitCollider, platformScript));
            }
        }
    }

    private IEnumerator FallThroughRoutine(Collider platformCollider, OneWayPlatform3D platformScript)
    {
        // プラットフォームの自動判定をOFFにする
        platformScript.IsManuallyControlled = true;

        // プレイヤーとプラットフォームの当たり判定を無効にする
        Physics.IgnoreCollision(capsuleCollider, platformCollider, true);

        // 現在のY軸の速度をリセットしてから力を加え、自然な落下に繋げる
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        // Impulseモードで瞬間的に力を加える
        rb.AddForce(Vector3.down * fallForce, ForceMode.Impulse);

        // 0.5秒待機
        yield return new WaitForSeconds(0.5f);

        if (capsuleCollider != null && platformCollider != null)
        {
            Physics.IgnoreCollision(capsuleCollider, platformCollider, false);
        }

        // プラットフォームの自動判定をONに戻す
        platformScript.IsManuallyControlled = false;

        fallThroughCoroutine = null;
    }

    private void DoCrouch()
    {
        isCurrentlyCrouching = true;
        if (capsuleCollider == null) return;

        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        rb.angularVelocity = Vector3.zero;

        float newHeight = Mathf.MoveTowards(capsuleCollider.height, crouchingColliderHeight, crouchSpeed * Time.fixedDeltaTime);
        float newCenterY = originalColliderCenter.y - (originalColliderHeight - newHeight) / 2f;
        capsuleCollider.height = newHeight;
        capsuleCollider.center = new Vector3(originalColliderCenter.x, newCenterY, originalColliderCenter.z);
    }

    private void DoStand()
    {
        isCurrentlyCrouching = false;
        if (capsuleCollider == null) return;
        float newHeight = Mathf.MoveTowards(capsuleCollider.height, originalColliderHeight, crouchSpeed * Time.fixedDeltaTime);
        float newCenterY = originalColliderCenter.y - (originalColliderHeight - newHeight) / 2f;
        capsuleCollider.height = newHeight;
        capsuleCollider.center = new Vector3(originalColliderCenter.x, newCenterY, originalColliderCenter.z);
    }
    public void StopCrouch()
    {
        isCurrentlyCrouching = false;
        animator?.SetBool("IsCrouching", false);
    }

    public void Remove()
    {
        if (capsuleCollider != null)
        {
            capsuleCollider.height = originalColliderHeight;
            capsuleCollider.center = originalColliderCenter;
        }
        isCurrentlyCrouching = false;
        animator?.SetBool("IsCrouching", false);
    }

    public void Update() { }
}