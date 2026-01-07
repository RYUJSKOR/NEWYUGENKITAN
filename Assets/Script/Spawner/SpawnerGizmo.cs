// SpawnerGizmo.cs (ターゲットの線表示を削除)
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// このスクリプトがアタッチされたオブジェクトの、すべての子オブジェクトに含まれる
/// 各種スポナーのギズモをまとめて描画します。
/// </summary>
[ExecuteInEditMode]
public class SpawnerGizmo : MonoBehaviour
{
    // 子に含まれる全てのスポナーコンポーネントを保持する配列
    private EnemySpawner[] enemySpawners;
    private RandomSpawner[] randomSpawners;
    private LoopSpawner[] loopSpawners;

    private void OnEnable()
    {
        FindAllSpawnersInChildren();
    }

    private void OnHierarchyChange()
    {
        FindAllSpawnersInChildren();
    }

    private void FindAllSpawnersInChildren()
    {
        enemySpawners = GetComponentsInChildren<EnemySpawner>(true);
        randomSpawners = GetComponentsInChildren<RandomSpawner>(true);
        loopSpawners = GetComponentsInChildren<LoopSpawner>(true);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (enemySpawners != null)
        {
            foreach (var spawner in enemySpawners)
            {
                if (spawner != null) DrawEnemySpawnerGizmos(spawner);
            }
        }

        if (randomSpawners != null)
        {
            foreach (var spawner in randomSpawners)
            {
                if (spawner != null) DrawRandomSpawnerGizmos(spawner);
            }
        }

        if (loopSpawners != null)
        {
            foreach (var spawner in loopSpawners)
            {
                if (spawner != null) DrawLoopSpawnerGizmos(spawner);
            }
        }
    }

    private void DrawEnemySpawnerGizmos(EnemySpawner spawner)
    {
        if (spawner.csvFile == null)
        {
            Handles.color = Color.red;
            Handles.Label(spawner.transform.position + Vector3.up, "[!] EnemySpawner:\nCSV File is not assigned.");
            return;
        }

        string[] lines = spawner.csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        string[] headers = lines[0].Trim().Split(',');
        int typeIndex = System.Array.IndexOf(headers, "EnemyType");
        int xIndex = System.Array.IndexOf(headers, "PositionX");
        int yIndex = System.Array.IndexOf(headers, "PositionY");
        int zIndex = System.Array.IndexOf(headers, "PositionZ");

        if (typeIndex == -1 || xIndex == -1 || yIndex == -1 || zIndex == -1) return;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Trim().Split(',');
            if (values.Length != headers.Length) continue;

            try
            {
                string enemyType = values[typeIndex];
                float x = float.Parse(values[xIndex]);
                float y = float.Parse(values[yIndex]);
                float z = float.Parse(values[zIndex]);
                Vector3 spawnPos = new Vector3(x, y, z);

                GameObject prefab = spawner.GetPrefabByType(enemyType);
                if (prefab != null)
                {
                    Handles.color = Color.cyan;
                    Handles.Label(spawnPos + Vector3.up * 0.6f, $"Spawns: {enemyType}");

                    MeshFilter mf = prefab.GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        Gizmos.color = new Color(0.0f, 0.8f, 1.0f, 0.4f);
                        Gizmos.DrawMesh(mf.sharedMesh, spawnPos, prefab.transform.rotation, prefab.transform.lossyScale);
                    }
                    else
                    {
                        Gizmos.color = new Color(0.0f, 0.8f, 1.0f, 0.7f);
                        Gizmos.DrawCube(spawnPos, Vector3.one * 0.5f);
                    }
                }
                else
                {
                    Handles.color = Color.red;
                    Handles.Label(spawnPos + Vector3.up * 0.6f, $"[!] Prefab Not Found: {enemyType}");
                    Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.5f);
                    Gizmos.DrawCube(spawnPos, Vector3.one * 0.5f);
                }
            }
            catch (System.Exception) { /* パース失敗した行は無視 */ }
        }
    }

    private void DrawRandomSpawnerGizmos(RandomSpawner spawner)
    {
        Handles.color = Color.yellow;
        Handles.Label(spawner.transform.position + Vector3.up * 0.5f, "Random Spawner");
        Gizmos.DrawIcon(spawner.transform.position, "d_ToolHandleLocal", true);
    }

    private void DrawLoopSpawnerGizmos(LoopSpawner spawner)
    {
        GameObject spawnObject = spawner.SpawnObject;

        if (spawnObject == null)
        {
            Handles.color = Color.red;
            Handles.Label(spawner.transform.position + Vector3.up, "[!] LoopSpawner:\nSpawnObject is not assigned.");
            Gizmos.DrawIcon(spawner.transform.position, "d_RotateTool", true);
            return;
        }

        Handles.color = Color.green;
        Handles.Label(spawner.transform.position + Vector3.up * 0.5f, "Loop Spawner");

        MeshFilter mf = spawnObject.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.4f);
            Gizmos.DrawMesh(mf.sharedMesh, spawner.transform.position, spawnObject.transform.rotation, spawnObject.transform.lossyScale);
        }
        else
        {
            Gizmos.color = new Color(0.5f, 1f, 0.5f, 0.7f);
            Gizmos.DrawCube(spawner.transform.position, Vector3.one);
        }

        // ▼▼▼ ターゲットへの線を描画していた以下のブロックを削除しました ▼▼▼
        /*
        if (targetObject != null)
        {
            Handles.color = Color.red;
            Handles.DrawDottedLine(spawner.transform.position, targetObject.transform.position, 5.0f);
            Handles.Label(targetObject.transform.position, "TARGET");
        }
        */
    }
#endif
}