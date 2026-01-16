using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AssetCleaner : EditorWindow
{
    private Vector2 scrollPosition;
    private Dictionary<string, bool> assetsToDelete = new Dictionary<string, bool>();
    private Dictionary<string, bool> scenesToInclude = new Dictionary<string, bool>();

    // --- [기존 설정 유지] ---
    private bool includeScripts = true;
    private bool includeTextures = true;
    private bool includeMaterials = true;
    private bool includeAudio = true;
    private bool includePrefabs = true;
    private bool createBackupBeforeDelete = true;
    private bool deleteEmptyFolders = true;
    private string backupFolderPath = "Assets/_DeletedAssetsBackup";
    private GUIStyle redXStyle;
    private bool selectAll = true;
    private bool sceneFoldout = true;

    // --- [신규 추가: 경량화 및 리포트 필터] ---
    private string nameFilter = "";      // 名前フィルタ (例: A)
    private int maxDisplayCount = 300;   // エディタに表示する最大数 (メモリ保護)
    private bool generateTextReport = true; // 検索時にテキストリポートを出力するか

    [MenuItem("Tools/Asset Cleaner")]
    public static void ShowWindow()
    {
        GetWindow<AssetCleaner>("Asset Cleaner");
    }

    private void OnEnable()
    {
        redXStyle = new GUIStyle();
        redXStyle.normal.textColor = Color.red;
        redXStyle.fontSize = 12;
        redXStyle.fontStyle = FontStyle.Bold;
        RefreshSceneList();
    }

    private string GetCurrentScriptPath()
    {
        var scriptGUID = AssetDatabase.FindAssets("t:Script AssetCleaner").FirstOrDefault();
        return scriptGUID != null ? AssetDatabase.GUIDToAssetPath(scriptGUID) : null;
    }

    private void RefreshSceneList()
    {
        scenesToInclude.Clear();
        string currentScene = EditorSceneManager.GetActiveScene().path;
        if (!string.IsNullOrEmpty(currentScene)) scenesToInclude[currentScene] = true;

        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scenesToInclude.ContainsKey(scene.path))
                scenesToInclude[scene.path] = scene.enabled;
        }

        // Assetsフォルダ内のすべてのシーンを検索 (Packages除外)
        string[] allScenes = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        foreach (string guid in allScenes)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            if (!scenesToInclude.ContainsKey(scenePath)) scenesToInclude[scenePath] = false;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Asset Cleaner (Performance Optimized)", EditorStyles.boldLabel);

        // 1. シーン選択 (기존 기능 유지)
        EditorGUILayout.Space();
        sceneFoldout = EditorGUILayout.Foldout(sceneFoldout, "Scenes to Preserve:", true);
        if (sceneFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All")) foreach (var k in scenesToInclude.Keys.ToList()) scenesToInclude[k] = true;
            if (GUILayout.Button("Deselect All")) foreach (var k in scenesToInclude.Keys.ToList()) scenesToInclude[k] = false;
            if (GUILayout.Button("Refresh")) RefreshSceneList();
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(120));
            foreach (var scene in scenesToInclude.ToList())
            {
                scenesToInclude[scene.Key] = EditorGUILayout.ToggleLeft(scene.Key, scene.Value);
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        // 2. 検索フィルタ & 表示制限 (신규 추가)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Search & Performance Options:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        nameFilter = EditorGUILayout.TextField("Name Filter (e.g. A)", nameFilter);
        maxDisplayCount = EditorGUILayout.IntField("Max Display Count", maxDisplayCount);
        generateTextReport = EditorGUILayout.Toggle("Generate Text Report", generateTextReport);

        EditorGUILayout.BeginHorizontal();
        includeScripts = EditorGUILayout.ToggleLeft("Scripts", includeScripts, GUILayout.Width(70));
        includeTextures = EditorGUILayout.ToggleLeft("Textures", includeTextures, GUILayout.Width(80));
        includeMaterials = EditorGUILayout.ToggleLeft("Materials", includeMaterials, GUILayout.Width(80));
        includeAudio = EditorGUILayout.ToggleLeft("Audio", includeAudio, GUILayout.Width(60));
        includePrefabs = EditorGUILayout.ToggleLeft("Prefabs", includePrefabs, GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        // 3. クリーニングオプション (기존 기능 유지)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cleaning Options:", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        createBackupBeforeDelete = EditorGUILayout.Toggle("Create Backup Before Deleting", createBackupBeforeDelete);
        deleteEmptyFolders = EditorGUILayout.Toggle("Delete Empty Folders", deleteEmptyFolders);
        if (createBackupBeforeDelete) backupFolderPath = EditorGUILayout.TextField("Backup Folder", backupFolderPath);
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Find Unused Assets", GUILayout.Height(30))) FindUnusedAssets();

        // 4. アセットリスト表示 (개수 제한 적용)
        DrawAssetList();
    }

    private void DrawAssetList()
    {
        if (assetsToDelete.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Found {assetsToDelete.Count} items (Showing top {maxDisplayCount})", EditorStyles.boldLabel);
            if (GUILayout.Button(selectAll ? "Deselect All" : "Select All", GUILayout.Width(100)))
            {
                selectAll = !selectAll;
                foreach (var asset in assetsToDelete.Keys.ToList()) assetsToDelete[asset] = selectAll;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            int displayed = 0;
            foreach (var assetEntry in assetsToDelete.ToList())
            {
                if (displayed >= maxDisplayCount) break;

                EditorGUILayout.BeginHorizontal();
                assetsToDelete[assetEntry.Key] = EditorGUILayout.Toggle(assetEntry.Value, GUILayout.Width(20));
                Object assetObject = AssetDatabase.LoadAssetAtPath<Object>(assetEntry.Key);
                EditorGUILayout.ObjectField(assetObject, typeof(Object), false);
                if (GUILayout.Button("✕", redXStyle, GUILayout.Width(20))) assetsToDelete[assetEntry.Key] = false;
                EditorGUILayout.EndHorizontal();
                displayed++;
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Marked Assets", GUILayout.Height(30))) DeleteMarkedAssets();
            GUI.backgroundColor = Color.white;
        }
    }

    private void FindUnusedAssets()
    {
        assetsToDelete.Clear();
        HashSet<string> usedAssets = new HashSet<string>();
        string currentScriptPath = GetCurrentScriptPath();

        foreach (var sceneEntry in scenesToInclude.Where(s => s.Value))
        {
            string[] dependencies = AssetDatabase.GetDependencies(sceneEntry.Key, true);
            foreach (string d in dependencies) usedAssets.Add(d);
        }

        string[] allAssets = AssetDatabase.GetAllAssetPaths();
        List<string> fullUnusedList = new List<string>();

        foreach (string asset in allAssets)
        {
            if (!asset.StartsWith("Assets/") || asset.EndsWith(".unity") || asset == currentScriptPath) continue;
            if (asset.Contains("/Resources/") || asset.Contains("/Editor/")) continue;

            // 이름 필터
            if (!string.IsNullOrEmpty(nameFilter) && !Path.GetFileName(asset).StartsWith(nameFilter, System.StringComparison.OrdinalIgnoreCase)) continue;

            bool shouldCheck = false;
            string ext = Path.GetExtension(asset).ToLower();
            if (includeScripts && ext == ".cs") shouldCheck = true;
            else if (includeTextures && (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".tga")) shouldCheck = true;
            else if (includeMaterials && ext == ".mat") shouldCheck = true;
            else if (includeAudio && (ext == ".mp3" || ext == ".wav" || ext == ".ogg")) shouldCheck = true;
            else if (includePrefabs && ext == ".prefab") shouldCheck = true;

            if (shouldCheck && !usedAssets.Contains(asset))
            {
                fullUnusedList.Add(asset);
                // UI 표시용 딕셔너리에는 제한된 수만큼만 담음 (메모리 보호)
                if (assetsToDelete.Count < maxDisplayCount + 200)
                    assetsToDelete.Add(asset, true);
            }
        }

        // 텍스트 리포트 생성 (신규)
        if (generateTextReport && fullUnusedList.Count > 0)
        {
            string reportPath = "UnusedAssets_Full_Report.txt";
            File.WriteAllLines(reportPath, fullUnusedList);
            Debug.Log($"[AssetCleaner] Full report saved to: {Path.GetFullPath(reportPath)}");
        }
    }

    private void DeleteMarkedAssets()
    {
        int markedCount = assetsToDelete.Count(x => x.Value);
        if (markedCount == 0) return;

        if (EditorUtility.DisplayDialog("Confirm Delete", $"{markedCount} assets will be deleted. Proceed?", "Delete", "Cancel"))
        {
            AssetDatabase.StartAssetEditing();
            foreach (var asset in assetsToDelete.Where(x => x.Value).ToList())
            {
                if (createBackupBeforeDelete) CreateBackup(asset.Key);
                AssetDatabase.DeleteAsset(asset.Key);
                assetsToDelete.Remove(asset.Key);
            }
            AssetDatabase.StopAssetEditing();

            if (deleteEmptyFolders) DeleteEmptyFolders("Assets");
            AssetDatabase.Refresh();
        }
    }

    private void DeleteEmptyFolders(string startPath)
    {
        if (!Directory.Exists(startPath)) return;
        foreach (var dir in Directory.GetDirectories(startPath)) DeleteEmptyFolders(dir);
        if (startPath != "Assets" && Directory.GetFiles(startPath).Length == 0 && Directory.GetDirectories(startPath).Length == 0)
        {
            Directory.Delete(startPath);
            if (File.Exists(startPath + ".meta")) File.Delete(startPath + ".meta");
        }
    }

    private void CreateBackup(string assetPath)
    {
        try
        {
            if (!Directory.Exists(backupFolderPath)) Directory.CreateDirectory(backupFolderPath);
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string targetPath = Path.Combine(backupFolderPath, timestamp, assetPath.Replace("Assets/", ""));
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
            File.Copy(assetPath, targetPath, true);
        }
        catch (System.Exception e) { Debug.LogError($"Backup failed for {assetPath}: {e.Message}"); }
    }
}