using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LightweightSelectiveCleaner : EditorWindow
{
    private string targetFolder = "Assets/";
    private string startLetter = "";
    private int maxFiles = 300;

    [MenuItem("Tools/Precision Texture Scan")]
    public static void ShowWindow()
    {
        // ウィンドウを表示
        GetWindow<LightweightSelectiveCleaner>("精密スキャン");
    }

    void OnGUI()
    {
        GUILayout.Label("テクスチャ精密スキャン設定 (メモリ節約モード)", EditorStyles.boldLabel);

        // 検索対象のフォルダを指定
        targetFolder = EditorGUILayout.TextField("スキャン対象フォルダ", targetFolder);
        EditorGUILayout.HelpBox("例: Assets/MyAssets/ (空欄の場合は全Assetsが対象)", MessageType.Info);

        // 開始アルファベットを指定 (例: Aから始まるファイルのみ)
        startLetter = EditorGUILayout.TextField("開始文字 (任意)", startLetter);
        EditorGUILayout.HelpBox("例: 'A' と入力すると、Aから始まるファイルのみを検証します", MessageType.Info);

        // 一度に処理する最大ファイル数
        maxFiles = EditorGUILayout.IntField("最大スキャン数", maxFiles);
        EditorGUILayout.HelpBox("PCの負荷を抑えるため、300〜500程度を推奨します", MessageType.Warning);

        if (GUILayout.Button("スキャン開始", GUILayout.Height(40)))
        {
            ExecuteSelectiveScan();
        }
    }

    private void ExecuteSelectiveScan()
    {
        // 1. 全シーンの依存関係を抽出 (どのエセットが使われているか把握)
        string[] allScenePaths = AssetDatabase.FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath).ToArray();

        Debug.Log($"[1/3] {allScenePaths.Length} 個のシーンから使用中のアセットを抽出中...");

        HashSet<string> usedAssets = new HashSet<string>();
        foreach (string scenePath in allScenePaths)
        {
            // シーンに関連付いている全てのアセットをハッシュセットに登録
            string[] dependencies = AssetDatabase.GetDependencies(scenePath, true);
            foreach (string dep in dependencies) usedAssets.Add(dep);
        }

        // 2. 指定された条件（フォルダ、開始文字、最大数）に合うテクスチャを抽出
        string[] allTextures = AssetDatabase.FindAssets("t:Texture", new[] { targetFolder.TrimEnd('/') })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => {
                string fileName = Path.GetFileName(path);

                // 開始文字のフィルタリング
                if (!string.IsNullOrEmpty(startLetter) && !fileName.StartsWith(startLetter, System.StringComparison.OrdinalIgnoreCase))
                    return false;

                // 実行時に自動参照されるフォルダは除外
                if (path.Contains("/Resources/") || path.Contains("/Editor/")) return false;

                return true;
            })
            .Take(maxFiles) // 指定した数だけ取り出す
            .ToArray();

        Debug.Log($"[2/3] 条件に一致する {allTextures.Length} 個のテクスチャを検証中...");

        // 3. 未使用のテクスチャを特定
        List<string> unusedTextures = new List<string>();
        foreach (string texPath in allTextures)
        {
            if (!usedAssets.Contains(texPath))
            {
                unusedTextures.Add(texPath);
            }
        }

        // 4. 結果をテキストファイルに書き出し
        string reportPath = "UnusedTextures_Partial_Report.txt";
        File.WriteAllLines(reportPath, unusedTextures);

        Debug.Log($"[3/3] 完了！ 未使用: {unusedTextures.Count}個 / 調査対象: {allTextures.Length}個");

        // 完了ダイアログの表示
        EditorUtility.DisplayDialog("スキャン完了",
            $"{allTextures.Length}個の検証対象のうち、{unusedTextures.Count}個の未使用テクスチャを特定しました。\n詳細は UnusedTextures_Partial_Report.txt を確認してください。", "OK");
    }
}