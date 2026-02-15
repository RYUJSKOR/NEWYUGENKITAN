using UnityEngine;


/// <summary>
/// Dive専用ネック側で
/// アニメ終了時に通常首を復帰させる
/// </summary>
public class DiveReturnHandler : MonoBehaviour
{
    private OrotiNeck owner;
    private Vector3 startPos;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float moveDuration = 2f;

    private Vector3 moveDirection;
    private float moveTimer;
    private bool isMoving;

    private OrotiDamageDealer dealer;

    public void SetOwner(OrotiNeck neck)
    {
        owner = neck;
    }

    private void Awake()
    {
        // 初期座標を保存
        startPos = transform.position;

        // ダメージDealerを取得
        dealer = GetComponent<OrotiDamageDealer>();
        if (dealer != null)
        {
            dealer.DisableDamage();
        }
    }

    // Dive開始時に呼ぶ
    public void StartDiveMove()
    {
        moveDirection = transform.forward;
        moveTimer = 0f;
        isMoving = true;

        // ダメージ判定有効化
        if (dealer != null)
            dealer.EnableDamage();
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveDuration)
        {
            isMoving = false;

            // 移動終了時にダメージ無効化
            if (dealer != null)
                dealer.DisableDamage();
        }
    }

    // Diveアニメ終了時（AnimationEventで呼ぶ）
    public void OnDiveFinished()
    {
        // 通常首を復帰
        if (owner != null)
        {
            owner.RestoreFromDive();
        }

        // Dive専用首を元の位置に戻す
        transform.position = startPos;
        gameObject.SetActive(false);
    }
}
