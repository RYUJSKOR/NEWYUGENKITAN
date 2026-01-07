using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class UnusedAssetCleaner : EditorWindow
{
    // アセット情報を管理するクラス
    class AssetInfo
    {
        public string path;
        public bool isKeep; // trueなら削除しない
    }

    [MenuItem("Tools/未使用アセットクリーナー(軽量版)")]
    public static void ShowWindow()
    {
        GetWindow<UnusedAssetCleaner>("Cleaner");
    }

    List<AssetInfo> unusedAssets = new List<AssetInfo>();
    Vector2 scrollPos;

    // 軽量化のためのページ管理変数
    int currentPage = 0;
    const int ItemsPerPage = 50; // 1ページに表示する数（これなら重くならない）

    void OnGUI()
    {
        GUILayout.Label("未使用アセットの検索と削除", EditorStyles.boldLabel);

        // 検索ボタン
        if (GUILayout.Button("1. 未使用アセットを検索", GUILayout.Height(30)))
        {
            FindUnusedAssets();
            currentPage = 0; // 検索したら1ページ目に戻す
        }

        if (unusedAssets.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"合計候補: {unusedAssets.Count} 件", EditorStyles.boldLabel);

            // --- 全体操作ボタン ---
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("リスト全体を「保護」")) SetAllKeep(true);
            if (GUILayout.Button("リスト全体を「削除対象」")) SetAllKeep(false);
            GUILayout.EndHorizontal();

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); // 区切り線

            // --- ページ送り機能 (ここが軽量化のキモ) ---
            int maxPage = Mathf.CeilToInt((float)unusedAssets.Count / ItemsPerPage) - 1;
            maxPage = Mathf.Max(0, maxPage);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<< 前へ", GUILayout.Width(80))) currentPage--;
            GUILayout.FlexibleSpace();
            GUILayout.Label($"{currentPage + 1} / {maxPage + 1} ページ", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("次へ >>", GUILayout.Width(80))) currentPage++;
            GUILayout.EndHorizontal();

            // ページ範囲制限
            currentPage = Mathf.Clamp(currentPage, 0, maxPage);

            // --- リスト表示エリア ---
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            // 現在のページに表示すべき範囲だけをループさせる
            int startIndex = currentPage * ItemsPerPage;
            int count = Mathf.Min(ItemsPerPage, unusedAssets.Count - startIndex);

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    int index = startIndex + i;
                    var info = unusedAssets[index];

                    EditorGUILayout.BeginHorizontal();

                    // チェックボックス
                    info.isKeep = EditorGUILayout.ToggleLeft("保護", info.isKeep, GUILayout.Width(50));

                    // パス表示
                    GUILayout.Label(info.path);

                    // 確認ボタン
                    if (GUILayout.Button("確認", GUILayout.Width(45)))
                    {
                        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(info.path);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);
            GUI.backgroundColor = Color.red;
            // ページ関係なく、リスト全体からチェックなしを削除
            if (GUILayout.Button("2. チェックのないファイルをゴミ箱へ移動 (全ページ対象)", GUILayout.Height(40)))
            {
                DeleteUncheckedAssets();
            }
            GUI.backgroundColor = Color.white;
        }
    }

    void SetAllKeep(bool keep)
    {
        foreach (var asset in unusedAssets) asset.isKeep = keep;
    }

    void FindUnusedAssets()
    {
        unusedAssets.Clear();

        string[] allAssets = AssetDatabase.GetAllAssetPaths()
            .Where(path => path.StartsWith("Assets/") && !System.IO.Directory.Exists(path)).ToArray();

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path).ToArray();

        string[] usedAssets = AssetDatabase.GetDependencies(scenes);
        HashSet<string> usedSet = new HashSet<string>(usedAssets);

        foreach (var assetPath in allAssets)
        {
            if (!usedSet.Contains(assetPath))
            {
                unusedAssets.Add(new AssetInfo { path = assetPath, isKeep = false });
            }
        }
    }

    void DeleteUncheckedAssets()
    {
        var assetsToDelete = unusedAssets.Where(a => !a.isKeep).ToList();

        if (assetsToDelete.Count == 0)
        {
            EditorUtility.DisplayDialog("情報", "削除対象のファイルはありません。", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("最終確認",
            $"{assetsToDelete.Count} 個のファイルをゴミ箱に移動します。\n本当によろしいですか？",
            "実行する", "やめる"))
        {
            int deletedCount = 0;
            // プログレスバーを表示（大量削除時のフリーズ防止）
            for (int i = 0; i < assetsToDelete.Count; i++)
            {
                var asset = assetsToDelete[i];
                EditorUtility.DisplayProgressBar("削除中", $"{asset.path} を移動中...", (float)i / assetsToDelete.Count);

                if (AssetDatabase.MoveAssetToTrash(asset.path))
                {
                    deletedCount++;
                }
            }
            EditorUtility.ClearProgressBar();

            FindUnusedAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("完了", $"{deletedCount} 個のファイルをゴミ箱に移動しました。", "OK");
        }
    }
}