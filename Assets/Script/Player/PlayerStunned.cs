using UnityEngine;

public class PlayerStunned : IPlayerState
{
    private Player player;
    private PlayerStateMachine playerStateMachine;
    private Rigidbody rb;

    private float stunTimer;
    private float stunDuration;

    // プルプルさせるための設定
    private float trembleIntensity = 0.05f; // 震えの強さ
    private float trembleSpeed = 30.0f;    // 震えの速さ
    private float trembleTimer;
    private Vector3 originalPosition;

    // コンストラクタでスタン時間を設定
    public PlayerStunned(float duration)
    {
        this.stunDuration = duration;
    }

    public void Init(Player player, PlayerStateMachine playerStateMachine)
    {
        this.player = player;
        this.playerStateMachine = playerStateMachine;
        this.rb = player.GetComponent<Rigidbody>();
        this.stunTimer = stunDuration;

        // スタン開始時の処理
        Debug.Log($"スタン開始！ 持続時間: {stunDuration}秒");

        // 入力を無効化
        playerStateMachine.InputHandler.Disable();

        // 水平方向の動きを止める
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        // プルプル用の初期位置を保存
        originalPosition = player.transform.localPosition;
        trembleTimer = 0f;
    }

    public void Update()
    {
        // タイマーを減らす
        stunTimer -= Time.deltaTime;

        // プルプル処理
        if (trembleTimer >= 0)
        {
            trembleTimer += Time.deltaTime * trembleSpeed;
            float xOffset = Mathf.Sin(trembleTimer) * trembleIntensity;
            float yOffset = Mathf.Cos(trembleTimer * 1.5f) * trembleIntensity * 0.7f; // Y軸方向は少し弱めに
            player.transform.localPosition = originalPosition + new Vector3(xOffset, yOffset, 0f); // Z軸は固定
        }

        // タイマーが0になったらスタン状態を終了する
        if (stunTimer <= 0)
        {
            player.transform.localPosition = originalPosition; // 念のため元の位置に戻す
            playerStateMachine.DeactivateState(this);
        }
    }

    public void Remove()
    {
        // スタン終了時の処理
        Debug.Log("スタン終了！");

        // 入力を有効化
        playerStateMachine.InputHandler.Enable();

        // プルプルを停止し、元の位置に戻す（念のため）
        player.transform.localPosition = originalPosition;

    }

    public void FixedUpdate() { }
    public void HandleInput() { }
}