using UnityEngine;

public class BonePile : MonoBehaviour, ISpawner
{
    [Header("設定")]
    [SerializeField] private int bonesNeededToSpawn = 10;
    [SerializeField] private GameObject boneEnemyPrefab;
    [SerializeField] private Vector3 growthPerBone = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("登場演出の設定")]
    [SerializeField] private float spawnHeight = 10f; // 敵が上昇する高さ
    [Tooltip("0だと真上、1だと完全に中央に移動します")]
    [SerializeField, Range(0f, 1f)] private float inwardMovementFactor = 0.7f; // 内側への移動割合

    private int currentBoneCount = 1;

    public void AddBone()
    {
        currentBoneCount++;
        transform.localScale += growthPerBone;
        if (currentBoneCount >= bonesNeededToSpawn)
        {
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (boneEnemyPrefab != null)
        {
            GameObject enemyInstance = Instantiate(boneEnemyPrefab, transform.position, Quaternion.identity);
            OnEnemySpawned(enemyInstance);
            Destroy(gameObject);
        }
    }

    public void OnEnemySpawned(GameObject enemyInstance)
    {
        RocketEnemy enemy = enemyInstance.GetComponent<RocketEnemy>();
        if (enemy != null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                enemy.Target = playerObject;
                BoneEyeEnemy boneEyeEnemy = enemy.GetComponent<BoneEyeEnemy>();
                BossController bossController = FindAnyObjectByType<BossController>();
                if(boneEyeEnemy != null && bossController != null)
                {
                    boneEyeEnemy.Init(bossController);
                }
            }
            else
            {
                Debug.LogError("Playerタグのオブジェクトが見つかりません！");
            }


            // 1. 現在のX座標に(1 - 割合)を掛けることで、中央(X=0)に近づける
            float targetX = transform.position.x * (1 - inwardMovementFactor);
            // 2. Y座標は指定した高さに設定
            float targetY = transform.position.y + spawnHeight;

            // 3. 計算した目標地点を敵に教える
            Vector3 targetPos = new Vector3(targetX, targetY, transform.position.z);
            enemy.StartSpawnSequence(targetPos);
        }
    }
}