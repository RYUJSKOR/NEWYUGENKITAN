using UnityEngine;
using System.Collections;

/// <summary>
/// プレイヤーが落下した際の復帰処理を行うステート。
/// 安全な場所の上空にテレポートし、一瞬静止（無敵状態）してから自然落下させる。
/// </summary>
public class PlayerFallRecovery : IPlayerState
{
    private Player player;
    private PlayerStateMachine playerStateMachine;
    private Rigidbody rb;
    private CharacterHealthManager healthManager;
    private PlayerInputHandler inputHandler;

    [Header("復帰設定")]
    private float teleportHeightOffset = 2.0f; // 復帰地点のどれだけ上にテレポートするか
    private float damageOnFall = 1f;           // 落下時に受けるダメージ
    private float pauseDuration = 0.3f;        // テレポート後に空中で静止する時間（秒）

    /// <summary>
    /// ステートが開始された時に呼ばれる。テレポートと静止処理を開始する。
    /// </summary>
    public void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;
        this.rb = player.GetRigidbody;
        this.healthManager = player.GetComponent<CharacterHealthManager>();
        this.inputHandler = playerStateMachine.InputHandler;

        // 1. 安全な復帰座標を取得
        Vector3 finalRecoveryPosition = player.GetRecoveryPosition();

        // 2. プレイヤーを復帰座標の上空へテレポートさせる
        Vector3 teleportPosition = finalRecoveryPosition + Vector3.up * teleportHeightOffset;
        player.transform.position = teleportPosition;

        // 3. プレイヤーを空中で完全に静止させる
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (healthManager != null)
        {
            // 4. 落下ダメージを与える
            healthManager.ApplyDamage(damageOnFall, true);

            // ▼▼▼【重要改善点】▼▼▼
            // 5. 静止している時間に合わせて無敵状態にする
            healthManager.ActivateInvincibility(pauseDuration);
        }

        // 6. Playerクラス経由でコルーチンを開始し、後処理を予約する
        player.StartPlayerCoroutine(PauseAndFallRoutine());

        Debug.Log($"[復帰処理] 開始。{teleportPosition}へテレポートし、{pauseDuration}秒間【無敵】で停止します。");
    }

    private IEnumerator PauseAndFallRoutine()
    {
        // pauseDurationで指定した秒数だけ待機
        yield return new WaitForSeconds(pauseDuration);

        // 待機後、物理演算を再度有効にして自然落下させる
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        Debug.Log("[復帰処理] 静止終了。自然落下を開始します。");

        // このステートの役割は終わったので終了させる
        playerStateMachine.DeactivateState(this);
    }

    // このステートがアクティブな間は、入力などを受け付けない
    public void HandleInput() { }
    public void Update() { }
    public void FixedUpdate() { }

    /// <summary>
    /// ステートが終了する時に呼ばれる後処理
    /// </summary>
    public void Remove()
    {
        if (player != null) player.EndRecovery();
        if (inputHandler != null) inputHandler.Enable();
    }
}