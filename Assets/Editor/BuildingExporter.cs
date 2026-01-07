using UnityEngine;
using UnityEditor; // Editor機能を使うために必要
using System.IO;   // ファイル書き出しのために必要
using System.Text; // 文字列結合のために必要
using System.Linq; // Linqを使うために必要

public class BuildingExporter
{
    // Unityのメニューに「Tools/Export Buildings to CSV」という項目を追加する
    [MenuItem("Tools/Export Buildings to CSV")]
    private static void Export()
    {
        // シーンに配置されている "Building" タグが付いたオブジェクトを全て探す
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Building");

        if (buildings.Length == 0)
        {
            Debug.LogWarning("エクスポート対象の建物が見つかりません。'Building' タグが設定されているか確認してください。");
            return;
        }

        // CSVのデータを効率的に作成するためのStringBuilder
        StringBuilder sb = new StringBuilder();

        // CSVのヘッダー行を追加
        sb.AppendLine("BuildingType,PositionX,PositionY,PositionZ,RotX,RotY,RotZ");

        // 見つかった建物の情報を1行ずつCSV形式に変換
        foreach (var building in buildings)
        {
            // Addressableのキーとしてプレハブ名を使うことを想定
            // "(Clone)"という文字が含まれていたら削除する
            string buildingType = building.name.Replace("(Clone)", "");

            Vector3 pos = building.transform.position;
            Vector3 rot = building.transform.eulerAngles;

            // データをカンマ区切りで追加
            string line = $"{buildingType},{pos.x},{pos.y},{pos.z},{rot.x},{rot.y},{rot.z}";
            sb.AppendLine(line);
        }

        // CSVファイルの保存先パスを指定
        // ここでは "Assets/GameObject/BuildingSpawner.csv" に保存する例
        string path = Path.Combine(Application.dataPath, "GameObject", "BuildingSpawner.csv");

        // ファイルに書き出す
        File.WriteAllText(path, sb.ToString());

        // Unityエディタにファイルの変更を認識させる
        AssetDatabase.Refresh();

        Debug.Log($"エクスポート完了: {buildings.Length}個の建物を {path} に書き出しました。");
    }
}