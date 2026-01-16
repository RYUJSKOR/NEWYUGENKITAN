using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ビルド完了時にUnityのデータベースから直接依存関係を抽出するクラス
public class BuildDependencyExtractor : IPostprocessBuildWithReport
{
    // 実行順序の指定
    public int callbackOrder => 0;

    // ビルド終了時に自動的に呼び出されるメソッド
    public void OnPostprocessBuild(BuildReport report)
    {
        Debug.Log("<color=yellow>[BuildReport]</color> ビルド終了を確認。依存関係の直接追跡を開始します...");

        // 重複を避けるためにHashSetを使用
        HashSet<string> usedAssets = new HashSet<string>();

        // 1. Build Profiles (EditorBuildSettings) に登録され、有効になっているシーンを取得
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToList();

        if (scenes.Count == 0)
        {
            Debug.LogWarning("[BuildReport] 有効なシーンがBuild Settingsに見つかりません。");
        }

        // 2. 各シーンが参照しているすべてのエセット（依存関係）を再帰的に抽出
        foreach (var scenePath in scenes)
        {
            // 第二引数を true にすることで、間接的な参照（マテリアルが使うテクスチャ等）もすべて取得
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            foreach (var dep in dependencies)
            {
                usedAssets.Add(dep);
            }
        }

        // 3. Resourcesフォルダ内のすべてのエセットを追加（ビルド時に強制的に含まれるため）
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (var path in allAssetPaths)
        {
            // Resourcesフォルダ内にあるか確認
            if (path.Contains("/Resources/"))
            {
                usedAssets.Add(path);

                // Resources内のエセットが参照している他のエセットも追加
                string[] resDeps = AssetDatabase.GetDependencies(path, true);
                foreach (var rd in resDeps)
                {
                    usedAssets.Add(rd);
                }
            }
        }

        // 4. 抽出結果をアルファベット順に並べ替えて保存
        string savePath = "BuildIncludedAssets_Direct.txt";
        File.WriteAllLines(savePath, usedAssets.OrderBy(s => s));

        Debug.Log($"<color=cyan>[BuildReport]</color> 抽出完了！ 合計 {usedAssets.Count} 個の有効なエセットを特定しました。");

        // 完了メッセージの表示
        EditorUtility.DisplayDialog("追跡完了",
            $"合計 {usedAssets.Count} 個のビルド済みエセットリストを作成しました。\n\n" +
            "プロジェクトルートの BuildIncludedAssets_Direct.txt を確認してください。", "OK");

        // リポートファイルをメモ帳で開く
        System.Diagnostics.Process.Start("notepad.exe", savePath);
    }
}