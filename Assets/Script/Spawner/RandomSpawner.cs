using UnityEngine;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public Vector3 viewportOffset; // Viewport 座標のオフセット
        [Range(0f, 1f)] public float spawnProbability;
    }

    [SerializeField] private List<EnemySpawnInfo> enemySpawnInfos;
    [SerializeField] private int maxEnemiesInView = 5;
    [SerializeField] private float spawnInterval = 3f;
    private float nextSpawnTime;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera がシーンに存在しません！");
        }
        nextSpawnTime = Time.time + spawnInterval;
    }

    void Update()
    {
        if (EnemyCounter.Instance == null || mainCamera == null) return;

        if (Time.time >= nextSpawnTime && EnemyCounter.Instance.EnemyCountInView < maxEnemiesInView)
        {
            SpawnRandomEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void SpawnRandomEnemy()
    {
        if (enemySpawnInfos.Count == 0)
        {
            Debug.LogWarning("生成する敵の情報が設定されていません。");
            return;
        }

        float randomIndex = Random.Range(0f, CalculateTotalProbability());
        EnemySpawnInfo selectedEnemyInfo = GetEnemyToSpawn(randomIndex);

        if (selectedEnemyInfo.enemyPrefab == null)
        {
            Debug.LogWarning($"敵Prefabが設定されていません: {selectedEnemyInfo.enemyPrefab?.name}");
            return;
        }

        Vector3 spawnViewportPosition = new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane + 10f) + selectedEnemyInfo.viewportOffset;
        Vector3 spawnPosition = mainCamera.ViewportToWorldPoint(spawnViewportPosition);
        Quaternion spawnRotation = Quaternion.identity;

        // Z軸を固定する場合など、必要に応じて調整
        spawnPosition.z = 0f;

        GameObject spawnedEnemy = Instantiate(selectedEnemyInfo.enemyPrefab, spawnPosition, spawnRotation);

        // ターゲット設定
        TargetingEnemy targetingComponent = spawnedEnemy.GetComponent<TargetingEnemy>();
        if (targetingComponent != null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetingComponent.Target = player;
                Debug.Log($"{spawnedEnemy.name} にターゲット (Player) を設定しました。");
            }
            else
            {
                Debug.LogWarning($"{spawnedEnemy.name} は TargetingEnemy コンポーネントを持っていますが、Player タグのオブジェクトが見つかりません。");
            }
        }
    }

    float CalculateTotalProbability()
    {
        float totalProbability = 0f;
        foreach (var enemyInfo in enemySpawnInfos)
        {
            totalProbability += enemyInfo.spawnProbability;
        }
        return totalProbability;
    }

    EnemySpawnInfo GetEnemyToSpawn(float randomValue)
    {
        float cumulativeProbability = 0f;
        foreach (var enemyInfo in enemySpawnInfos)
        {
            cumulativeProbability += enemyInfo.spawnProbability;
            if (randomValue <= cumulativeProbability)
            {
                return enemyInfo;
            }
        }
        return enemySpawnInfos[0]; // 念のため
    }
}