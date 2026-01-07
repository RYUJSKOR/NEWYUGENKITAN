using UnityEngine;

public class PlayerClimbingLadder : IPlayerState
{
    private Player player;
    private PlayerStateMachine playerStateMachine;
    private Rigidbody rb;

    // --- インスペクターで調整可能な設定 ---
    private float climbSpeed = 5f;          // 梯子を登る（上下）速度
    private float moveOnLadderSpeed = 4f;   // 梯子を横移動する速度
    private float jumpOffForce = 15f;         // 梯子からジャンプする際の力

    public void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;
        this.rb = player.GetComponent<Rigidbody>();

        // 梯子を登り始めるとき、重力を無効化する
        rb.useGravity = false;
        // 上昇中の速度を一旦リセット
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, 0);
    }

    public void HandleInput() { } // このステートではUpdateとFixedUpdateで入力処理を行う

    public void Update()
    {
        // --- 梯子から離れる条件をチェック ---

        // ジャンプ、ダッシュ、または梯子に触れていない場合は、このステートを終了する
        if (playerStateMachine.JumpPressed || playerStateMachine.DashPressed || !player.IsTouchingLadder)
        {
            // もしジャンプで離れる場合は、ジャンプの初速を与える
            if (playerStateMachine.JumpPressed)
            {
                // Y方向の速度をリセットしてからジャンプ力を加える
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
                rb.AddForce(Vector3.up * jumpOffForce, ForceMode.Impulse);
            }

            playerStateMachine.DeactivateState(this);
            return;
        }
    }

    public void FixedUpdate()
    {
        // 上下と左右の入力を取得
        float verticalInput = playerStateMachine.VerticalInput;
        float horizontalInput = playerStateMachine.HorizontalInput;

        // 入力に基づいて速度を決定
        rb.linearVelocity = new Vector3(horizontalInput * moveOnLadderSpeed, verticalInput * climbSpeed, 0);
    }

    public void Remove()
    {
        // このステートが終わる時、必ず重力を元に戻す
        rb.useGravity = true;
    }
}