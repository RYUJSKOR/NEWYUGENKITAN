// ファイル名: EnemySpawnerGizmoDrawer.cs

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[RequireComponent(typeof(EnemySpawner))]
public class EnemySpawnerGizmoDrawer : MonoBehaviour
{
    private EnemySpawner spawner;

    private void OnDrawGizmos()
    {
        if (spawner == null)
        {
            spawner = GetComponent<EnemySpawner>();
        }

        if (spawner == null || spawner.csvFile == null)
        {
            return;
        }

        var enemyDataList = LoadEnemyDataFromTextAssetForGizmos(spawner.csvFile);

        foreach (var data in enemyDataList)
        {
            if (!data.TryGetValue("EnemyType", out string enemyType) ||
                !float.TryParse(data.GetValueOrDefault("PositionX", "0"), out float x) ||
                !float.TryParse(data.GetValueOrDefault("PositionY", "0"), out float y) ||
                !float.TryParse(data.GetValueOrDefault("PositionZ", "0"), out float z))
            {
                continue;
            }

            Vector3 spawnPosition = new Vector3(x, y, z);
            GameObject prefab = spawner.GetPrefabByType(enemyType);

            // ケース1: プレハブ自体が見つからない（インスペクターの設定ミスなど）
            if (prefab == null)
            {
                Gizmos.color = new Color(1, 0, 0, 0.4f); // 半透明の赤
                Gizmos.DrawSphere(spawnPosition, 0.5f);
                Handles.Label(spawnPosition + Vector3.up, enemyType + "\n(Prefab Not Found)");
                continue;
            }

            MeshFilter meshFilter = prefab.GetComponentInChildren<MeshFilter>();

            // ケース2: プレハブにメッシュが見つかった（成功！）
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // まずはプレハブのスケールを基本とする
                Vector3 finalScale = prefab.transform.localScale;

                // もし敵が "BalloonEnemy" なら、ギズモのスケールを上書きする
                if (enemyType == "BalloonEnemy")
                {
                    // ここでギズモとして表示したいサイズを直接指定します。
                    // (1, 1, 1) を基準にお好みの大きさに調整してください。
                    finalScale = new Vector3(40, 40, 40);
                }

                if(enemyType == "KarakasaGhost")
                {
                    // ここでギズモとして表示したいサイズを直接指定します。
                    // (1, 1, 1) を基準にお好みの大きさに調整してください。
                    finalScale = new Vector3(40, 40, 40);
                }

                // 最終決定したスケール(finalScale)でギズモを描画する
                Gizmos.color = GetEnemyColor(enemyType);
                Gizmos.DrawMesh(meshFilter.sharedMesh, spawnPosition, prefab.transform.rotation, finalScale);

                Handles.Label(spawnPosition + Vector3.up * 0.5f, enemyType);
            }
            // ケース3: プレハブはあるが、メッシュがない
            else
            {
                Gizmos.color = new Color(1, 1, 0, 0.4f); // 半透明の黄
                Gizmos.DrawSphere(spawnPosition, 0.5f);
                Handles.Label(spawnPosition + Vector3.up, enemyType + "\n(No Mesh)");
            }
        }
    }

    private Color GetEnemyColor(string enemyType)
    {
        // 敵の種類ごとの色定義
        var enemyColors = new Dictionary<string, Color>
        {
            { "RoketEnemy", new Color(1, 0, 0, 0.5f) },
            { "BalloonEnemy", new Color(0, 0, 1, 0.5f) },
            { "KarakasaGhost", new Color(0, 1, 1, 0.5f) },
            { "BlueEnemy", new Color(0.5f, 0.5f, 1, 0.5f) },
            { "BlueBoundEnemy", new Color(0, 1, 0, 0.5f) },
            { "FallEnemy", new Color(1, 0.5f, 0, 0.5f) },
            { "HomingEnemy", new Color(1, 0, 1, 0.5f) }
        };
        return enemyColors.ContainsKey(enemyType) ? enemyColors[enemyType] : new Color(1, 1, 1, 0.5f); // デフォルトは白
    }

    private List<Dictionary<string, string>> LoadEnemyDataFromTextAssetForGizmos(TextAsset csvFile)
    {
        var dataList = new List<Dictionary<string, string>>();
        using (var reader = new StringReader(csvFile.text))
        {
            string headerLine = reader.ReadLine();
            if (headerLine == null) return dataList;

            string[] headers = headerLine.Split(',');
            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');
                if (values.Length == headers.Length)
                {
                    var data = headers.Zip(values, (h, v) => new { Header = h.Trim(), Value = v.Trim() })
                                      .ToDictionary(item => item.Header, item => item.Value);
                    dataList.Add(data);
                }
            }
        }
        return dataList;
    }
}
#endif