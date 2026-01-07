using UnityEngine;
using System.Collections.Generic;

public class WorldStreamer : MonoBehaviour
{
    [Header("プレイヤー")]
    public Transform player;

    [Header("チャンク設定")]
    public float chunkSize = 50f;

    [Tooltip("横方向（X軸）の表示距離（チャンク数）")]
    public int viewDistanceX = 3; // 例: 左右に3チャンクずつ
    [Tooltip("奥行方向（Z軸）の表示距離（チャンク数）")]
    public int viewDistanceZ = 1; // 例: 前後に1チャンクずつ

    [Header("管理するチャンクの親オブジェクト")]
    public Transform chunksParent;

    private Vector2Int currentPlayerChunk;
    private Dictionary<Vector2Int, GameObject> allChunks = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        InitializeChunks();
        UpdateChunks();
    }

    void Update()
    {
        Vector2Int playerChunk = GetChunkCoordinatesFromPosition(player.position);

        if (playerChunk != currentPlayerChunk)
        {
            currentPlayerChunk = playerChunk;
            UpdateChunks();
        }
    }

    void InitializeChunks()
    {
        if (chunksParent == null)
        {
            Debug.LogError("Chunks Parentが設定されていません！", this);
            return;
        }

        foreach (Transform chunkTransform in chunksParent)
        {
            Vector2Int? coords = GetChunkCoordinatesFromName(chunkTransform.name);

            if (coords.HasValue)
            {
                if (allChunks.ContainsKey(coords.Value))
                {
                    Debug.LogWarning($"チャンク座標 {coords.Value} が重複しています。{chunkTransform.name}は無視されます。", chunkTransform);
                    continue;
                }
                allChunks.Add(coords.Value, chunkTransform.gameObject);
                chunkTransform.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"オブジェクト名「{chunkTransform.name}」からチャンク座標を読み取れませんでした。このオブジェクトは無視されます。", chunkTransform);
            }
        }
        currentPlayerChunk = GetChunkCoordinatesFromPosition(player.position);
    }

    void UpdateChunks()
    {
        foreach (var chunk in allChunks)
        {
            Vector2Int chunkCoords = chunk.Key;
            GameObject chunkObject = chunk.Value;

            int distanceX = Mathf.Abs(chunkCoords.x - currentPlayerChunk.x);
            int distanceZ = Mathf.Abs(chunkCoords.y - currentPlayerChunk.y); // 座標のyがZ軸に対応

            // XとZで個別の表示距離を使って判定する
            if (distanceX <= viewDistanceX && distanceZ <= viewDistanceZ)
            {
                chunkObject.SetActive(true);
            }
            else
            {
                chunkObject.SetActive(false);
            }
        }
    }

    Vector2Int GetChunkCoordinatesFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);
        return new Vector2Int(x, z);
    }

    Vector2Int? GetChunkCoordinatesFromName(string name)
    {
        string[] parts = name.Split('_');
        if (parts.Length == 3 && parts[0] == "Chunk")
        {
            try
            {
                int x = int.Parse(parts[1]);
                int z = int.Parse(parts[2]);
                return new Vector2Int(x, z);
            }
            catch (System.FormatException)
            {
                return null;
            }
        }
        return null;
    }
}