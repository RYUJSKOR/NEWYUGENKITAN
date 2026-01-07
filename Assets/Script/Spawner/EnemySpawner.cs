using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[System.Serializable]
public struct EnemyPrefabMapping
{
    public string enemyType;
    public GameObject prefab;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private float spawnDistanceThreshold = 10f; // スポーンする距離の閾値
    [SerializeField] private bool spawnOnce = true; // 各敵の配置につき一度のみスポーンするかどうか

    [SerializeField] public TextAsset csvFile;
    [SerializeField] public List<EnemyPrefabMapping> prefabMappings;

    private List<Dictionary<string, string>> enemySpawnData;
    private HashSet<string> spawnedEnemyKeys = new HashSet<string>(); 

    void Start()
    {
        enemySpawnData = LoadEnemyDataFromTextAsset(csvFile);
    }

    void Update()
    {
        if (playerObject == null || enemySpawnData == null) return;

        List<Dictionary<string, string>> enemiesToSpawn = new List<Dictionary<string, string>>();
        List<Dictionary<string, string>> spawnedThisFrame = new List<Dictionary<string, string>>();

        foreach (var data in enemySpawnData)
        {
            string enemyType = data.ContainsKey("EnemyType") ? data["EnemyType"] : "";
            float positionX = data.ContainsKey("PositionX") ? float.Parse(data["PositionX"]) : transform.position.x;
            float positionY = data.ContainsKey("PositionY") ? float.Parse(data["PositionY"]) : transform.position.y;
            float positionZ = data.ContainsKey("PositionZ") ? float.Parse(data["PositionZ"]) : transform.position.z;

            Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);
            float distanceToPlayerXY = Vector2.Distance(new Vector2(spawnPosition.x, spawnPosition.y), new Vector2(playerObject.transform.position.x, playerObject.transform.position.y));

            string spawnKey = $"{enemyType}_{positionX}_{positionY}_{positionZ}";

            if (distanceToPlayerXY <= spawnDistanceThreshold && (!spawnOnce || !spawnedEnemyKeys.Contains(spawnKey)))
            {
                enemiesToSpawn.Add(data);
                spawnedThisFrame.Add(data);
            }
        }

        if (enemiesToSpawn.Count > 0)
        {
            SpawnEnemies(enemiesToSpawn);
            foreach (var spawnedData in spawnedThisFrame)
            {
                string spawnedEnemyType = spawnedData.ContainsKey("EnemyType") ? spawnedData["EnemyType"] : "";
                float positionX = spawnedData.ContainsKey("PositionX") ? float.Parse(spawnedData["PositionX"]) : transform.position.x;
                float positionY = spawnedData.ContainsKey("PositionY") ? float.Parse(spawnedData["PositionY"]) : transform.position.y;
                float positionZ = spawnedData.ContainsKey("PositionZ") ? float.Parse(spawnedData["PositionZ"]) : transform.position.z;
                string spawnKey = $"{spawnedEnemyType}_{positionX}_{positionY}_{positionZ}";
                spawnedEnemyKeys.Add(spawnKey);
                enemySpawnData.Remove(spawnedData);
            }
        }
    }

    private List<Dictionary<string, string>> LoadEnemyDataFromTextAsset(TextAsset file)
    {
        List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();

        if (file == null)
        {
            Debug.LogError("TextAsset がアサインされていません。");
            return dataList;
        }

        using (StringReader reader = new StringReader(file.text))
        {
            string headerLine = reader.ReadLine();
            if (headerLine == null)
            {
                Debug.LogError("CSVファイルが空です。");
                return dataList;
            }
            string[] headers = headerLine.Split(',');

            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(',');
                if (values.Length == headers.Length)
                {
                    Dictionary<string, string> data = headers.Zip(values, (header, value) => new { Header = header, Value = value })
                        .ToDictionary(item => item.Header, item => item.Value);
                    dataList.Add(data);
                }
                else
                {
                    Debug.LogWarning($"CSVファイルの行の要素数がヘッダーと一致しません: {line}");
                }
            }
        }
        return dataList;
    }

    private void SpawnEnemies(List<Dictionary<string, string>> dataToSpawn)
    {
        foreach (var data in dataToSpawn)
        {
            string enemyType = data.ContainsKey("EnemyType") ? data["EnemyType"] : "";
            float positionX = data.ContainsKey("PositionX") ? float.Parse(data["PositionX"]) : transform.position.x;
            float positionY = data.ContainsKey("PositionY") ? float.Parse(data["PositionY"]) : transform.position.y;
            float positionZ = data.ContainsKey("PositionZ") ? float.Parse(data["PositionZ"]) : transform.position.z;

            GameObject prefabToSpawn = GetPrefabByType(enemyType);
            if (prefabToSpawn != null)
            {
                Vector3 spawnPosition = new Vector3(positionX, positionY, positionZ);
                GameObject spawnedEnemy = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

                TargetingEnemy targetingEnemy = spawnedEnemy.GetComponent<TargetingEnemy>();
                if (targetingEnemy != null && playerObject != null)
                {
                    targetingEnemy.Target = playerObject;
                }
            }
            else
            {
                Debug.LogError($"Prefabが見つかりません: {enemyType}");
            }
        }
    }

    public GameObject GetPrefabByType(string type)
    {
        // prefabMappingsリストがnull、または空の場合はnullを返す
        if (prefabMappings == null || prefabMappings.Count == 0)
        {
            return null;
        }

        foreach (var mapping in prefabMappings)
        {
            if (mapping.prefab != null && mapping.enemyType == type)
            {
                return mapping.prefab;
            }
        }
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnDistanceThreshold);
    }
}