using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Threading.Tasks;

public class BuildingSpawner : MonoBehaviour
{
     // CSVデ??（TextAssetでInspectorにアサイン）
    [SerializeField] private TextAsset csvFile;

    private async void Start()
    {
        var buildingDataList = LoadBuildingDataFromCSV(csvFile);
        await SpawnBuildingsAsync(buildingDataList);
    }

    // CSVを読み込んで建物情報をリスト化
    private List<Dictionary<string, string>> LoadBuildingDataFromCSV(TextAsset file)
    {
        List<Dictionary<string, string>> dataList = new();

        if (file == null)
        {
            Debug.LogError("CSVフ?イルが指定されていません。");
            return dataList;
        }

        using StringReader reader = new(file.text);
        string headerLine = reader.ReadLine();
        if (headerLine == null) return dataList;

        string[] headers = headerLine.Split(',');

        while (reader.Peek() >= 0)
        {
            string line = reader.ReadLine();
            string[] values = line.Split(',');
            if (values.Length == headers.Length)
            {
                var entry = headers.Zip(values, (h, v) => new { h, v })
                    .ToDictionary(pair => pair.h, pair => pair.v);
                dataList.Add(entry);
            }
        }

        return dataList;
    }

    // 非同期で建物をロ?ド、配置
    private async Task SpawnBuildingsAsync(List<Dictionary<string, string>> dataList)
    {
        foreach (var data in dataList)
        {
            string key = data["BuildingType"];
            float x = float.Parse(data["PositionX"]);
            float y = float.Parse(data["PositionY"]);
            float z = float.Parse(data["PositionZ"]);

            Vector3 spawnPos = new Vector3(x, y, z);

            float rotX = data.ContainsKey("RotX") ? float.Parse(data["RotX"]) : 0f;
            float rotY = data.ContainsKey("RotY") ? float.Parse(data["RotY"]) : 0f;
            float rotZ = data.ContainsKey("RotZ") ? float.Parse(data["RotZ"]) : 0f;

            Quaternion spawnRot = Quaternion.Euler(rotX, rotY, rotZ);

            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(key);
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Instantiate(handle.Result, spawnPos, spawnRot);
                Debug.Log($"建物 {key} を配置しました @ {spawnPos}");
            }
            else
            {
                Debug.LogError($"Addressables ロ?ド失敗: {key}");
            }
        }
    }
}
