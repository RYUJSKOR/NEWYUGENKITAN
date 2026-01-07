using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class AutoChunker : MonoBehaviour
{
    [Header("チャンク設定")]
    [Tooltip("1チャンクのサイズ")]
    public float chunkSize = 50f;

    [Header("対象オブジェクト")]
    [Tooltip("ここに振り分けたいオブジェクトの親オブジェクトを指定")]
    public Transform objectsParent;

    [Header("ギズモ（仕切り線）設定")]
    public bool showGizmos = true;
    public Color gizmoColor = new Color(0, 1, 1, 0.25f);
    public float gizmoHeight = 20f;

    [ContextMenu("Generate Chunks")]
    private void GenerateChunks()
    {
        if (objectsParent == null)
        {
            Debug.LogError("対象の親オブジェクトが指定されていません！");
            return;
        }

        string chunkContainerName = "[GENERATED_CHUNKS]";
        GameObject chunkContainer = GameObject.Find(chunkContainerName);
        if (chunkContainer == null)
        {
            chunkContainer = new GameObject(chunkContainerName);
        }

        // 既存のチャンクを名前から読み取り、辞書に事前登録しておく
        Dictionary<Vector2Int, Transform> chunkParents = new Dictionary<Vector2Int, Transform>();
        foreach (Transform existingChunk in chunkContainer.transform)
        {
            string[] nameParts = existingChunk.name.Split('_');
            if (nameParts.Length == 3 && nameParts[0] == "Chunk")
            {
                try
                {
                    int x = int.Parse(nameParts[1]);
                    int z = int.Parse(nameParts[2]);
                    Vector2Int coords = new Vector2Int(x, z);
                    if (!chunkParents.ContainsKey(coords))
                    {
                        chunkParents.Add(coords, existingChunk);
                    }
                }
                catch { /* 名前の解析に失敗したものは無視 */ }
            }
        }


        List<Transform> children = new List<Transform>();
        foreach (Transform child in objectsParent)
        {
            children.Add(child);
        }

        int sortedObjectCount = 0;
        foreach (Transform child in children)
        {
            Vector3 position = child.position;
            int x = Mathf.FloorToInt(position.x / chunkSize);
            int z = Mathf.FloorToInt(position.z / chunkSize);
            Vector2Int coords = new Vector2Int(x, z);

            // 辞書にチャンクがなければ（つまりシーンに存在しなければ）新規作成
            if (!chunkParents.ContainsKey(coords))
            {
                string chunkName = $"Chunk_{x}_{z}";
                GameObject newChunk = new GameObject(chunkName);
                newChunk.transform.SetParent(chunkContainer.transform);
                chunkParents.Add(coords, newChunk.transform);
                Debug.Log($"新しいチャンク {chunkName} を作成しました。");
            }

            child.SetParent(chunkParents[coords]);
            sortedObjectCount++;
        }

        if (sortedObjectCount > 0)
        {
            Debug.Log($"チャンクの生成が完了しました。{sortedObjectCount}個のオブジェクトを振り分けました。");
        }
        else
        {
            Debug.Log("振り分ける対象のオブジェクトが見つかりませんでした。");
        }
    }

    [ContextMenu("Rebuild All Chunks")]
    private void RebuildAllChunks()
    {
        // 重要な操作なので、実行前に確認ダイアログを表示
        if (!EditorUtility.DisplayDialog("チャンクの再構築",
            "すべての既存チャンクを解体し、現在のチャンクサイズで再生成します。\n" +
            "よろしいですか？ (この操作はUndoできます)", "はい", "いいえ"))
        {
            return;
        }

        // 1. 既存の全オブジェクトを一旦objectsParentに戻す
        string chunkContainerName = "[GENERATED_CHUNKS]";
        GameObject chunkContainer = GameObject.Find(chunkContainerName);
        if (chunkContainer != null)
        {
            // ループのために一時リストを作成
            List<Transform> allChunks = chunkContainer.transform.Cast<Transform>().ToList();

            foreach (Transform chunk in allChunks)
            {
                List<Transform> childrenToMove = chunk.Cast<Transform>().ToList();
                foreach (Transform child in childrenToMove)
                {
                    // Undoを記録しながら親子関係を変更
                    Undo.SetTransformParent(child, objectsParent, "Un-chunk Objects");
                }
                // 空になったチャンクを削除
                Undo.DestroyObjectImmediate(chunk.gameObject);
            }
        }

        Debug.Log("既存チャンクをすべて解体しました。新しいチャンクを生成します...");

        // 2. 通常のチャンク生成処理を実行
        GenerateChunks();
    }

    [ContextMenu("Consolidate Duplicate Chunks")]
    private void ConsolidateDuplicateChunks()
    {
        string chunkContainerName = "[GENERATED_CHUNKS]";
        GameObject chunkContainer = GameObject.Find(chunkContainerName);
        if (chunkContainer == null)
        {
            Debug.LogWarning("整理対象の[" + chunkContainerName + "]オブジェクトが見つかりません。");
            return;
        }

        // 座標ごとに重複したチャンクをグループ化する
        var chunkGroups = chunkContainer.transform.Cast<Transform>()
            .Select(t => new { transform = t, baseName = t.name.Split(' ')[0] }) // "Chunk_0_0 (1)" -> "Chunk_0_0"
            .GroupBy(x => x.baseName)
            .Where(g => g.Count() > 1); // 重複しているグループのみを対象

        int consolidatedGroupCount = 0;
        foreach (var group in chunkGroups)
        {
            // グループの最初のチャンクをマスターとして、そこに他の中身を全部移す
            Transform masterChunk = group.First().transform;
            Debug.Log($"チャンク「{group.Key}」を{masterChunk.name}に統合します...");

            // 2番目以降の重複チャンクをループ
            foreach (var duplicate in group.Skip(1))
            {
                Transform duplicateChunk = duplicate.transform;
                // 重複チャンクの中身をマスターに移動（安全のためリスト化してからループ）
                List<Transform> childrenToMove = new List<Transform>();
                foreach (Transform child in duplicateChunk)
                {
                    childrenToMove.Add(child);
                }
                foreach (Transform child in childrenToMove)
                {
                    child.SetParent(masterChunk, true); // ワールド座標を維持して移動
                }
                // 中身が空になった重複チャンクを削除
                Undo.DestroyObjectImmediate(duplicateChunk.gameObject);
            }
            consolidatedGroupCount++;
        }

        if (consolidatedGroupCount > 0)
        {
            Debug.Log($"重複した{consolidatedGroupCount}個のチャンクグループを整理しました。");
        }
        else
        {
            Debug.Log("重複しているチャンクは見つかりませんでした。");
        }
    }


    // ... (OnDrawGizmosSelectedの中身は変更なし) ...
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Vector3 size = new Vector3(chunkSize, gizmoHeight, chunkSize);

        int gridRange = 10;
        for (int x = -gridRange; x < gridRange; x++)
        {
            for (int z = -gridRange; z < gridRange; z++)
            {
                Vector3 center = new Vector3(
                    (x * chunkSize) + (chunkSize / 2),
                    transform.position.y,
                    (z * chunkSize) + (chunkSize / 2)
                );
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}