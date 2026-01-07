using UnityEngine;

public class BoneEyeEnemy : MonoBehaviour
{
    [SerializeField] private int GomiNum;
    [SerializeField] private GameObject piecesPrefab;
    private BossController ownerBoss;
    private RocketEnemyMovement movementAI; // AIスクリプトへの参照

    private void Awake()
    {
        // 自身のAIコンポーネントを取得しておく
        movementAI = GetComponent<RocketEnemyMovement>();
    }

    public void Init(BossController boss)
    {
        if (boss == null)
        {
            Destroy(gameObject);
            return;
        }

        ownerBoss = boss;
        ownerBoss.OnDeathSequenceStart += StopBehavior; // 合図1：行動停止
        ownerBoss.OnBossDefeated += SelfDestruct;     // 合図2：自爆

        Debug.Log(gameObject.name + " がボス " + ownerBoss.name + " の監視を開始しました。");
    }

    /// <summary>
    /// 合図1：全ての行動を停止する
    /// </summary>
    private void StopBehavior()
    {
        Debug.Log(gameObject.name + " が行動を停止します。");
        // AIスクリプトを無効化して動きを止める
        if (movementAI != null)
        {
            movementAI.enabled = false;
        }
        // 物理的な動きも止める
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// 合図2：自爆する
    /// </summary>
    private void SelfDestruct()
    {
        Debug.Log("ボスに合わせて " + gameObject.name + " は自爆します。");
        for (int i = 0; i < GomiNum; i++)
        {
            GameObject gomi = Instantiate(piecesPrefab, transform.position, Quaternion.identity);
            gomi.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-10f, 10f), Random.Range(10f, 30f), 0.0f));
            Destroy(gomi,1);
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (ownerBoss != null)
        {
            // 2つのイベントの購読を解除する
            ownerBoss.OnDeathSequenceStart -= StopBehavior;
            ownerBoss.OnBossDefeated -= SelfDestruct;
        }
    }
}