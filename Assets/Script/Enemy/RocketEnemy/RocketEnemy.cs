using UnityEngine;

public class RocketEnemy : TargetingEnemy
{
    private enum State { Spawning, Normal }
    private State currentState = State.Normal;
    private Vector3 spawnTargetPosition;

    [SerializeField] private RocketEnemyMovementConfig rocketMovementConfig;
    [SerializeField] private LayerMask groundLayer;

    private ParticleSystem effect;
    private RocketEnemyMovement enemyMovement;

    void Awake()
    {
        healthManager = GetComponent<CharacterHealthManager>();
        effect = GetComponentInChildren<ParticleSystem>();
        enemyMovement = GetComponent<RocketEnemyMovement>();
        if (enemyMovement == null)
        {
            enemyMovement = gameObject.AddComponent<RocketEnemyMovement>();
        }
    }

    new void Start()
    {
        base.Start();
        if (currentState == State.Normal)
        {
            InitializeNormalBehavior();
        }
    }

    private void InitializeNormalBehavior()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath += Explode;
        }
        if (effect != null)
        {
            effect.Play();
        }

        // AIの有効化をここに移動 
        enemyMovement.enabled = true;
        enemyMovement.Initialize(rocketMovementConfig, TargetObject);
        enemyMovement.SetGroundLayer(groundLayer);
        enemyMovement.SetSpeedModifier(currentSpeedModifier);
    }

    public void StartSpawnSequence(Vector3 targetPosition)
    {
        currentState = State.Spawning;
        spawnTargetPosition = targetPosition;

        // AIを一時的に無効化
        enemyMovement.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (currentState == State.Spawning)
        {
            transform.position = Vector3.MoveTowards(transform.position, spawnTargetPosition, 10f * Time.deltaTime);

            if (Vector3.Distance(transform.position, spawnTargetPosition) < 0.1f)
            {
                currentState = State.Normal;

                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }

                InitializeNormalBehavior();
            }
        }
    }

    public override void ApplySpeedModifier(float modifier)
    {
        base.ApplySpeedModifier(modifier);

        if (enemyMovement != null)
        {
            enemyMovement.SetSpeedModifier(currentSpeedModifier);
        }
    }

    void FixedUpdate()
    {
        if (effect != null && !effect.isStopped)
        {
            effect.Simulate(Time.fixedDeltaTime, true, false, true);
        }
    }
    new private void OnDestroy()
    {
        if (healthManager != null)
        {
            healthManager.OnDeath -= Explode;
        }
        if (EnemyCounter.Instance != null)
        {
            EnemyCounter.Instance.RemoveEnemy(gameObject);
        }
    }
    override protected void Explode()
    {
        for (int i = 0; i < GomiNum; i++)
        {
            GameObject gomi = Instantiate(piecesPrefab, transform.position, Quaternion.identity);
            gomi.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-10f, 10f), Random.Range(10f, 30f), 0.0f));
            Destroy(gomi, GomiLifeTime);
        }
        Destroy(gameObject);
    }
    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0 ||
            collision.gameObject.CompareTag("Player"))
        {
            healthManager.ApplyDamage(healthManager.GetHealth());
        }
    }
}