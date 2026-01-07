using UnityEngine;

public class RocketEnemyMovement : MonoBehaviour
{
    private enum EnemyState { Spawning, Hovering, Charging, Attacking }
    private EnemyState currentState = EnemyState.Hovering;
    private Vector3 spawnTargetPosition;

    [SerializeField] private float hoverSpeed = 1f;
    [SerializeField] private float attackDelay = 2f;
    [SerializeField] private float attackSpeed = 15f;
    [SerializeField] private float detectionRange = 2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody rb;
    private float attackTimer = 0f;
    private Vector3 initialPosition;
    private GameObject TargetObject;

    private float currentSpeedModifier = 1.0f;
    private float chargeWiggleTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
    }

    public void StartSpawnSequence(Vector3 targetPosition)
    {
        currentState = EnemyState.Spawning;
        spawnTargetPosition = targetPosition;
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Spawning:
                HandleSpawning();
                break;
            case EnemyState.Hovering:
                HandleHovering();
                break;
            case EnemyState.Charging:
                HandleCharging();
                break;
        }
    }

    private void HandleSpawning()
    {
        transform.position = Vector3.MoveTowards(transform.position, spawnTargetPosition, hoverSpeed * 2f * Time.deltaTime);

        if (Vector3.Distance(transform.position, spawnTargetPosition) < 0.1f)
        {
            currentState = EnemyState.Hovering;
            rb.isKinematic = false;
            rb.useGravity = true;
            initialPosition = transform.position;
        }
    }

    // ▼▼▼ このメソッドの中身を修正 ▼▼▼
    private void HandleHovering()
    {
        Hover();

        // ターゲットがnullでないことを確認
        if (TargetObject == null) return;

        // ★★★ 修正箇所 ★★★
        // 画面の中心ではなく、ターゲット（プレイヤー）との距離を計算する
        float distanceToTarget = Vector3.Distance(transform.position, TargetObject.transform.position);

        // ターゲットが検知範囲内に入ったら攻撃準備開始
        if (distanceToTarget <= detectionRange)
        {
            currentState = EnemyState.Charging;
            attackTimer = 0f;
            chargeWiggleTimer = 0f;
            Debug.Log(gameObject.name + "がプレイヤーを検知しました。攻撃準備に入ります。");
        }
    }

    private void HandleCharging()
    {
        attackTimer += Time.deltaTime * currentSpeedModifier;
        chargeWiggleTimer += Time.deltaTime * currentSpeedModifier;
        ChargeWiggle();

        if (attackTimer >= attackDelay)
        {
            AttackTarget();
        }
    }

    #region 既存メソッド
    public void SetSpeedModifier(float modifier)
    {
        currentSpeedModifier = modifier;
    }
    void Hover()
    {
        Vector3 hoverDirection = new Vector3(Mathf.Sin(Time.time * 2f), Mathf.Sin(Time.time * 3f), 0f);
        rb.linearVelocity = hoverDirection.normalized * hoverSpeed * currentSpeedModifier;
    }
    void AttackTarget()
    {
        currentState = EnemyState.Attacking;
        Vector3 direction = (TargetObject.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * attackSpeed * currentSpeedModifier;
    }
    void ChargeWiggle()
    {
        float frequency = 15f;
        float amplitude = 10f;
        float wiggleY = Mathf.Sin(chargeWiggleTimer * frequency) * amplitude;
        if (wiggleY >= 0f)
        {
            Vector3 toInitial = (initialPosition - transform.position).normalized;
            toInitial.y = 0f;
            Vector3 moveDirection = (toInitial + Vector3.up).normalized;
            rb.linearVelocity = moveDirection * Mathf.Abs(wiggleY) * currentSpeedModifier;
        }
        else
        {
            if (TargetObject != null)
            {
                Vector3 toTarget = (TargetObject.transform.position - transform.position).normalized;
                toTarget.y = 0f;
                Vector3 moveDirection = (toTarget + Vector3.down).normalized;
                rb.linearVelocity = moveDirection * Mathf.Abs(wiggleY) * currentSpeedModifier;
            }
            else
            {
                rb.linearVelocity = new Vector3(0f, wiggleY, 0f) * currentSpeedModifier;
            }
        }
    }
    public void Initialize(RocketEnemyMovementConfig config, GameObject target)
    {
        if (config == null || target == null) return;
        hoverSpeed = config.hoverSpeed;
        attackDelay = config.attackDelay;
        attackSpeed = config.attackSpeed;
        detectionRange = config.detectionRange;
        TargetObject = target;
    }
    public void SetGroundLayer(LayerMask layer) { groundLayer = layer; }
    // GetCameraCenterWorldPositionは不要になったため削除
    #endregion
}