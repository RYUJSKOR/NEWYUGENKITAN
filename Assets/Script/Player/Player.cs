using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using GLTFast.Schema;
using System.Linq;

/// <summary>
/// どの床に、どの相対位置で乗っていたかを記録する構造体
/// </summary>
public struct SafePositionRecord
{
    public Transform Platform { get; }
    public Vector3 LocalOffset { get; }

    public SafePositionRecord(Transform platform, Vector3 localOffset)
    {
        Platform = platform;
        LocalOffset = localOffset;
    }
}

/// <summary>
/// Unityのインスペクターに表示させるための仮面とゲームオブジェクトのマッピング
/// </summary>
[System.Serializable]
public class MaskMapping
{
    public MaskType type;
    public GameObject maskObject;
}

public class Player : MonoBehaviour
{
    public PlayerStateMachine playerStateMachine;
    private CharacterHealthManager healthManager;
    private Animator animator;
    private CameraShakeManager shakeManager;
    private Rigidbody rb;
    private CapsuleCollider capsuleCollider;
    private DemonAnimation demonAnimation;
    private SEController SE;


    [Header("地面チェック設定")]
    public float groundCheckDistance = 0.6f;
    public LayerMask groundLayer;
    public Vector3 groundCheckOffsetLeft = new Vector3(-0.5f, 0f, 0f);
    public Vector3 groundCheckOffsetRight = new Vector3(0.5f, 0f, 0f);

    [SerializeField, LayerSelector] private int playerLayer;
    [SerializeField, LayerSelector] private int playerinvincibleLayer;
    public event System.Action OnAttackedByEnemy;

    [SerializeField] private List<string> attackTags = new List<string>();

    [SerializeField] protected GameObject piecesPrefab;
    [SerializeField] protected int GomiNum;
    [SerializeField] protected float GomiLifeTime;

    [SerializeField] protected ParticleSystem dustEffect;

    [Header("仮面設定")]
    public List<MaskMapping> maskMappings; // 仮面のリスト

    [Header("復帰設定")]
    [SerializeField, Tooltip("保存する安全な座標の履歴の最大数")]
    private int maxSafePositionHistory = 30;

    [Header("摩擦設定")]
    [SerializeField] private PhysicsMaterial frictionMaterial;     // 停止・しゃがみ時
    [SerializeField] private PhysicsMaterial noFrictionMaterial;   // 移動・ジャンプ中

    // 動的プラットフォーム対応の復帰システム
    private Queue<SafePositionRecord> safePositionHistory = new Queue<SafePositionRecord>();
    private Vector3 lastStaticGroundPosition; // 動的プラットフォームが見つからなかった場合の最終復帰座標

    public Animator GetAnimator() => animator;
    public Rigidbody GetRigidbody => rb;
    public bool IsTouchingLadder { get; private set; }
    public bool IsRecovering { get; private set; }
    public ParticleSystem DustEffect => dustEffect;

    private void OnEnable()
    {
        if (BossGameManager.Instance != null)
        {
            BossGameManager.Instance.RegisterPlayer(this);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        healthManager = GetComponent<CharacterHealthManager>();
        animator = GetComponent<Animator>();
        shakeManager = UnityEngine.Camera.main.GetComponent<CameraShakeManager>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        demonAnimation = GetComponent<DemonAnimation>();
        SE = GetComponent<SEController>();

        playerStateMachine = GetComponent<PlayerStateMachine>();

        if (healthManager != null)
        {
            healthManager.OnDeath += PlayerDeath;
            healthManager.OnDamageTaken += PlayerDamage;
        }

        playerStateMachine.Init(this);

        // 初期位置をフォールバック用に保存
        lastStaticGroundPosition = transform.position;

        if (BossGameManager.Instance != null && BossGameManager.Instance.HasSavedData)
        {
            healthManager.SetHealth(BossGameManager.Instance.SavedPlayerHealth);
            Debug.Log("保存されたプレイヤーの体力を復元しました: " + healthManager.GetHealth());
        }
    }

    void Update()
    {
        playerStateMachine.StateSetting();

        // 垂直方向の速度が -1.0f より大きい（＝急降下していない）時だけ足場を記憶する
        if (!IsRecovering && rb.linearVelocity.y > -1.0f && TryGetStableGround(out Transform groundTransform))
        {
            Transform platformTransform; // 最終的に「足場」として記憶するTransform

            // ▼▼▼【重要改善点】足場か、通常の地面かを自動で判断するロジック ▼▼▼
            if (groundTransform.parent != null && groundTransform.parent.GetComponent<PlatformController>() != null)
            {
                // 親がいて、PlatformControllerを持っている場合（＝壊れる足場）は、親を足場とする
                platformTransform = groundTransform.parent;
            }
            else
            {
                // それ以外の場合（＝通常の地面）は、検知した地面そのものを足場とする
                platformTransform = groundTransform;
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

            // 最後に記録したプラットフォームと比較し、変わった場合のみログを出力
            if (safePositionHistory.Count == 0 || safePositionHistory.Last().Platform != platformTransform)
            {
                Debug.Log($"[FallRecoveryDebug] 新しい安全な足場を記憶しました: {platformTransform.name}");
            }

            // 床のTransformからの相対位置を計算
            Vector3 localOffset = platformTransform.InverseTransformPoint(transform.position);
            // どの床か、どの位置かを記録してキューに追加
            safePositionHistory.Enqueue(new SafePositionRecord(platformTransform, localOffset));

            // キューのサイズが最大値を超えたら、一番古いものから削除
            while (safePositionHistory.Count > maxSafePositionHistory)
            {
                safePositionHistory.Dequeue();
            }

            // もし乗っている床が静的なら（特定のコンポーネントを持たないなら）
            // 最終手段用の座標を更新する
            if (platformTransform.GetComponent<ObjectListRotator>() == null && platformTransform.GetComponent<PlatformController>() == null)
            {
                lastStaticGroundPosition = transform.position;
            }
        }

        UpdateColliderMaterial();
    }

    /// <summary>
    /// 摩擦の切り替え処理
    /// </summary>
    private void UpdateColliderMaterial()
    {
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        bool isCrouching = playerStateMachine.IsCrouching; // ← StateMachine 経由で参照

        if (!IsGrounded())
        {
            // 空中は摩擦なし
            capsuleCollider.sharedMaterial = noFrictionMaterial;
        }
        else if (isMoving && !isCrouching)
        {
            // 地上で移動中かつしゃがんでいない → 摩擦なし
            capsuleCollider.sharedMaterial = noFrictionMaterial;
        }
        else
        {
            // 停止 or しゃがみ中 → 摩擦あり
            capsuleCollider.sharedMaterial = frictionMaterial;
        }
    }

    /// <summary>
    /// 落下復帰用の座標を取得する（動的プラットフォーム対応）
    /// </summary>
    /// <returns>安全な復帰先のワールド座標</returns>
    public Vector3 GetRecoveryPosition()
    {
        // ▼▼▼ ログ追加 ▼▼▼
        Debug.Log($"[FallRecoveryDebug] GetRecoveryPositionが呼ばれました。履歴の数: {safePositionHistory.Count}");

        var history = safePositionHistory.ToArray();
        for (int i = history.Length - 1; i >= 0; i--)
        {
            var record = history[i];

            // ▼▼▼ ログ追加 ▼▼▼
            string platformName = record.Platform != null ? record.Platform.name : "null(破壊済み)";
            Debug.Log($"[FallRecoveryDebug] --- 履歴 {i} をチェック中: Platform = {platformName} ---");


            if (record.Platform == null || !record.Platform.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[FallRecoveryDebug] この足場は無効なためスキップします。");
                continue;
            }

            // ▼▼▼ ログ追加 ▼▼▼
            bool hasPlatformController = record.Platform.TryGetComponent<PlatformController>(out var platform);
            Debug.Log($"[FallRecoveryDebug] PlatformControllerを持っているか？ -> {hasPlatformController}");

            if (hasPlatformController && platform.IsBroken)
            {
                // ▼▼▼ ログ追加 ▼▼▼
                Debug.LogWarning($"[FallRecoveryDebug] PlatformControllerを発見。IsBroken = {platform.IsBroken}。この足場は破壊されているためスキップします。");
                continue;
            }

            // ▼▼▼ ログ追加 ▼▼▼
            Debug.Log($"[FallRecoveryDebug] ★★★ 有効な復帰先を発見しました: {record.Platform.name} ★★★");
            return record.Platform.TransformPoint(record.LocalOffset);
        }

        Debug.LogWarning("[FallRecoveryDebug] 有効な動的プラットフォームが見つかりませんでした。静的な最終地点に復帰します。");
        return lastStaticGroundPosition;
    }

    /// <summary>
    /// 指定された種類の仮面を有効化し、それ以外をすべて無効化する
    /// </summary>
    public void SetActiveMask(MaskType typeToShow)
    {
        if (maskMappings == null) return;

        foreach (var mapping in maskMappings)
        {
            if (mapping.maskObject != null)
            {
                bool isActive = mapping.type == typeToShow;
                mapping.maskObject.SetActive(isActive);
            }
        }
    }

    public bool IsGrounded()
    {
        if (rb.linearVelocity.y > 0.1f) return false;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null) return false;

        Vector3 point1 = transform.position + capsule.center + Vector3.up * (capsule.height / 2 - capsule.radius);
        Vector3 point2 = transform.position + capsule.center + Vector3.down * (capsule.height / 2 - capsule.radius);
        float radius = capsule.radius * 0.95f;
        Vector3 direction = Vector3.down;
        float maxDistance = groundCheckDistance;

        return Physics.CapsuleCast(point1, point2, radius, direction, maxDistance, groundLayer);
    }

    /// <summary>
    /// 安定した地面に立っているかチェックし、その地面のTransformを返す
    /// </summary>
    private bool TryGetStableGround(out Transform groundTransform)
    {
        groundTransform = null;
        float safeAreaWidth = transform.localScale.x * 0.1f;
        Vector3 boxCenter = transform.position + new Vector3(0, -0.1f, 0);
        Vector3 halfExtents = new Vector3(safeAreaWidth, 0.1f, transform.localScale.z * 0.2f);
        float maxDistance = groundCheckDistance;

        if (Physics.BoxCast(boxCenter, halfExtents, Vector3.down, out RaycastHit hit, transform.rotation, maxDistance, groundLayer))
        {
            groundTransform = hit.transform;
            return true;
        }

        return false;
    }

    // (これ以降のメソッドは変更ありません)

    private void OnDrawGizmos() { }
    private void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius) { }
    private void PlayerDeath()
    {
        playerStateMachine.InputHandler.Disable();
        Invoke(nameof(DisablePlayer), 3f);
    }
    private void DisablePlayer()
    {
        gameObject.SetActive(false);
    }
    private void PlayerDamage()
    {
        gameObject.layer = playerinvincibleLayer;
        StartCoroutine(ResetLayerAfterInvincibility());
        shakeManager.TriggerShake(0.15f, 0.25f);
        SE.Play("Player.Damage");
    }
    private IEnumerator ResetLayerAfterInvincibility()
    {
        yield return new WaitForSeconds(healthManager.GetInvincibleDuration());
        gameObject.layer = playerLayer;
        Debug.Log("無敵状態終了");
    }
    private void OnDestroy()
    {
        healthManager.OnDeath -= PlayerDeath;
        healthManager.OnDamageTaken -= PlayerDamage;
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[Player]当たった");
        bool isEnemy = collision.gameObject.GetComponent<EnemyBase>() != null;


        if (isEnemy)
        {
            OnAttackedByEnemy?.Invoke();
            Debug.Log("通知");
        }
    }
    public PlayerStateMachine GetPlayerStateMachine() => playerStateMachine;
    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        foreach (string tag in attackTags)
        {
            if (go.CompareTag(tag))
            {
                OnAttackedByEnemy?.Invoke();
                Debug.Log($"通知: {go.name} (Tag: {tag})");
                return;
            }
        }

        if (other.CompareTag("Ladder"))
        {
            IsTouchingLadder = true;
            Debug.Log("梯子に触れた");
        }

        if (other.CompareTag("Goal"))
        {
            Debug.Log("ゴール！遷移を開始します。");

            // 1. ゲームクリアの内部処理（タイマーストップやスコア保存など）
            // ※GameManager.Clear() の中で「古いシーン遷移」をしていないか確認してください！
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Clear();
            }

            // 2. 新しいシステムでシーン遷移（フェードアウト -> Loading -> 次のシーン）
            if (SceneFlowController.Instance != null)
            {
                // "Result" の部分は、実際のリザルトシーンの名前に書き換えてください
                SceneFlowController.Instance.RequestScene("Result");
            }
            else
            {
                Debug.LogError("SceneFlowControllerが見つかりません！GameManagerオブジェクトを確認してください。");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            IsTouchingLadder = false;
            Debug.Log("梯子から離れた");
        }
    }
    public void StartFallRecovery()
    {
        // ▼▼▼ ログ追加 ▼▼▼
        Debug.Log("[FallRecoveryDebug] 落下復帰処理を開始します。");
        playerStateMachine.ActivateState(new PlayerFallRecovery());
    }

    public void EndRecovery()
    {
        IsRecovering = false;
    }
    public void TriggerStun(float duration)
    {
        if (playerStateMachine.IsStateActive<PlayerStunned>()) return;
        playerStateMachine.ActivateState(new PlayerStunned(duration));
    }

    // PlayerFallRecoveryからコルーチンを呼び出すためのヘルパー
    public void StartPlayerCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}