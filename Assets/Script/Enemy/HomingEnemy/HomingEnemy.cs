using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(EnemyAttack))]
[RequireComponent(typeof(Animator))]
public class HomingEnemy : TargetingEnemy
{
    [Header("Detection Settings")]
    [Tooltip("この半径内にターゲットが入ると発見します")]
    [SerializeField] private float detectionRadius = 15f;

    [Header("Configuration")]
    [SerializeField] private HomingEnemyConfig config;

    [Header("Attack Settings")]
    [SerializeField] private float attackInterval = 2.5f;

    [Header("Attack Visuals")]
    [Tooltip("攻撃中に非表示にするオブジェクト（例：武器など）")]
    [SerializeField] private GameObject objectToHideDuringAttack;

    // HomingEnemy.cs の変数宣言エリアに追加
    [Header("Rotation Settings")]
    [Tooltip("ターゲットの方向を向く速さ")]
    [SerializeField] private float rotationSpeed = 5f;

    // --- Private Variables ---
    private EnemyAttack attackModule;
    private Animator animator;
    private float attackTimer;
    private bool isTargetDetected = false; // 発見状態を管理するフラグ

    // Animatorのパラメータハッシュ
    private static readonly int RiseTriggerHash = Animator.StringToHash("Rise");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int TakeDamageTriggerHash = Animator.StringToHash("TakeDamage");
    private static readonly int TargetLostTriggerHash = Animator.StringToHash("TargetLost");

    // --- Unity Lifecycle Methods ---
    new void Start()
    {
        base.Start();

        animator = GetComponent<Animator>();
        attackModule = GetComponent<HomingEnemyAttack>();

        if (TargetObject == null)
        {
            Debug.LogWarning("TargetObjectが設定されていません。索敵機能が動作しません。", gameObject);
        }

        if (config == null)
        {
            Debug.LogError("HomingEnemyConfigが設定されていません。", gameObject);
            this.enabled = false;
            return;
        }

        if (attackModule != null)
        {
            attackModule.Initialize(config);
        }
        else
        {
            Debug.LogError("EnemyAttackコンポーネントが見つかりません。", gameObject);
            this.enabled = false;
            return;
        }

        if (healthManager != null)
        {
            healthManager.OnDeath += Dead;
            healthManager.OnDamageTaken += OnDamaged; // 必要に応じてコメント解除
        }
    }

    void Update()
    {
        // ターゲットが設定されていなければ何もしない
        if (TargetObject == null) return;

        // ターゲットと自分自身の距離を計算
        float distanceToTarget = Vector3.Distance(transform.position, TargetObject.transform.position);

        // 距離が索敵半径内かどうかで処理を分岐
        if (distanceToTarget <= detectionRadius)
        {
            // --- 発見状態の処理 ---

            // もしまだ発見状態でなければ、発見した瞬間の処理を行う
            if (!isTargetDetected)
            {
                isTargetDetected = true;
                Debug.Log("ターゲットを発見！");

                // 攻撃モジュールにターゲットを渡す
                attackModule.SetTarget(TargetObject);

                // Riseアニメーションを再生
                animator.SetTrigger(RiseTriggerHash);

                // 攻撃タイマーをリセット
                attackTimer = attackInterval;
            }

            FaceTarget(); // ターゲットの方向を向く処理

            // 発見後の攻撃行動
            AttackBehavior();
        }
        else
        {
            // --- 未発見状態の処理 ---

            // もし直前まで発見していたなら、「見失った」瞬間の処理を行う
            if (isTargetDetected)
            {
                isTargetDetected = false;
                Debug.Log("ターゲットを見失いました。");

                // 攻撃モジュールのターゲットを解除
                attackModule.SetTarget(null);

                // Animator Controller側で、Idle_FoundからIdle_NotFoundに戻るためのトリガーを引く
                animator.SetTrigger(TargetLostTriggerHash);
            }
        }
    }

    /// <summary>
    /// ターゲットに対する攻撃行動
    /// </summary>
    private void AttackBehavior()
    {
        if (!isTargetDetected) return; // 見失っている場合は攻撃しない
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Rise")) return;

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            animator.SetTrigger(AttackTriggerHash);
        }
    }

    // --- Public & Animation Event Methods ---
    public void OnDamaged()
    {
        Debug.Log("タメージ受けちゃったよぉ！");
        animator.SetTrigger(TakeDamageTriggerHash);
    }

    public void AnimationTrigger_Attack()
    {
        Debug.Log("アニメーションイベントが呼ばれました！弾を発射します。");

        if (attackModule != null)
        {
            attackModule.PerformAttack();
        }
    }

    // --- Private Methods ---
    private void Dead()
    {
        GameObject pieces = Instantiate(piecesPrefab, transform.position, Quaternion.identity);
        Destroy(pieces, GomiLifeTime);
        Destroy(gameObject);
        DropItem();
    }

    // --- Editor Gizmos ---
    // Sceneビューで索敵半径を視覚的に確認するための処理
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    /// <summary>
    /// 攻撃開始時にアニメーションイベントから呼び出す
    /// </summary>
    public void HideObjectForAttack()
    {
        if (objectToHideDuringAttack != null)
        {
            objectToHideDuringAttack.SetActive(false);
        }
    }

    /// <summary>
    /// 攻撃終了時にアニメーションイベントから呼び出す
    /// </summary>
    public void ShowObjectAfterAttack()
    {
        if (objectToHideDuringAttack != null)
        {
            objectToHideDuringAttack.SetActive(true);
        }
    }

    /// <summary>
    /// ターゲットの方向を滑らかにY軸だけで向く処理
    /// </summary>
    private void FaceTarget()
    {
        // ターゲットへの方向ベクトルを計算
        Vector3 directionToTarget = TargetObject.transform.position - transform.position;

        // Y軸の回転のみに限定するため、上下の高さの差は無視する
        directionToTarget.y = 0;

        // その方向を向くための回転（Quaternion）を計算
        // directionToTargetがゼロベクトル（真上や真下にいる場合など）だとエラーになるため、ゼロでない場合のみ回転する
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // 現在の角度からターゲットの角度まで、滑らかに回転させる
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

}