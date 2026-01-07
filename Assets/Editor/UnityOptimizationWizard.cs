using UnityEditor;
using UnityEngine;
using System.Linq;

public class UnityOptimizationWizard : EditorWindow
{
    // --- Texture Optimization Settings ---
    private int _maxTextureSize = 2048;
    private TextureImporterFormat _desktopFormat = TextureImporterFormat.DXT5;
    private TextureImporterFormat _androidFormat = TextureImporterFormat.ETC2_RGBA8;
    private TextureImporterFormat _iosFormat = TextureImporterFormat.PVRTC_RGBA4;

    // --- Animator Optimization Settings ---
    private AnimatorCullingMode _animatorCullingMode = AnimatorCullingMode.CullUpdateTransforms;

    // --- Quality Settings ---
    private ShadowResolution _shadowResolution = ShadowResolution.Medium;
    private int _pixelLightCount = 2;
    private bool _enableRealtimeGI = false;
    private bool _enableBakedGI = true;

    [MenuItem("Tools/Optimization/? Unity Optimization Wizard")]
    public static void ShowWindow()
    {
        GetWindow<UnityOptimizationWizard>("? Optimization Wizard");
    }

    private void OnGUI()
    {
        GUILayout.Label("? Unity Optimization Wizard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("このツールは、Unity プロジェクトとエディタのパフォーマンスを向上させるための一般的な最適化設定を一括で適用します。", MessageType.Info);

        // --- Texture Optimization ---
        EditorGUILayout.Space();
        GUILayout.Label("1. Texture Optimization", EditorStyles.largeLabel);
        _maxTextureSize = EditorGUILayout.IntSlider("Max Texture Size", _maxTextureSize, 512, 4096);
        _desktopFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Desktop Format", _desktopFormat);
        _androidFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("Android Format", _androidFormat);
        _iosFormat = (TextureImporterFormat)EditorGUILayout.EnumPopup("iOS Format", _iosFormat);

        if (GUILayout.Button("Apply Texture Optimization to All Textures"))
        {
            ApplyTextureOptimizationForAll();
        }

        // --- GPU Instancing ---
        EditorGUILayout.Space();
        GUILayout.Label("2. GPU Instancing", EditorStyles.largeLabel);
        EditorGUILayout.HelpBox("対応するマテリアルで GPU Instancing を有効にし、ドローコールを削減します。", MessageType.Info);
        if (GUILayout.Button("Enable GPU Instancing for All Materials"))
        {
            EnableInstancingForAllMaterialsInProject();
        }

        // --- Static Batching ---
        EditorGUILayout.Space();
        GUILayout.Label("3. Static Batching", EditorStyles.largeLabel);
        EditorGUILayout.HelpBox("選択中のオブジェクトとその子オブジェクトを Static Batching の対象にします。動かないオブジェクトに推奨。", MessageType.Info);
        if (GUILayout.Button("Set Selected Objects as Static Batching Target"))
        {
            SetSelectedObjectsStaticForBatching();
        }

        // --- Animator Optimization ---
        EditorGUILayout.Space();
        GUILayout.Label("4. Animator Culling Mode", EditorStyles.largeLabel);
        _animatorCullingMode = (AnimatorCullingMode)EditorGUILayout.EnumPopup("Culling Mode", _animatorCullingMode);
        EditorGUILayout.HelpBox("ビューポート外のアニメーターのパフォーマンスを最適化します。通常は 'Cull Update Transforms' を推奨。", MessageType.Info);
        if (GUILayout.Button("Apply Culling Mode to All Animators in Scene"))
        {
            ApplyAnimatorCullingMode();
        }

        // --- Quality Settings for Build ---
        EditorGUILayout.Space();
        GUILayout.Label("5. Quality Settings (for Build)", EditorStyles.largeLabel);
        EditorGUILayout.HelpBox("現在のプラットフォームの Quality Settings を最適化します。ビルド後のパフォーマンスに影響します。", MessageType.Info);

        _shadowResolution = (ShadowResolution)EditorGUILayout.EnumPopup("Shadow Resolution", _shadowResolution);
        _pixelLightCount = EditorGUILayout.IntSlider("Pixel Light Count", _pixelLightCount, 0, 4);
        _enableRealtimeGI = EditorGUILayout.Toggle("Enable Realtime GI", _enableRealtimeGI);
        _enableBakedGI = EditorGUILayout.Toggle("Enable Baked GI", _enableBakedGI);

        if (GUILayout.Button("Apply Recommended Quality Settings"))
        {
            ApplyRecommendedQualitySettings();
        }
    }

    // --- Optimization Methods ---

    private void ApplyTextureOptimizationForAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;

            importer.maxTextureSize = _maxTextureSize;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.mipmapEnabled = true; // ミップマップは基本的に有効にしておく方がGPUキャッシュ効率が良い

            // 各プラットフォームの設定
            TextureImporterPlatformSettings desktopSettings = importer.GetPlatformTextureSettings("Standalone");
            desktopSettings.overridden = true;
            desktopSettings.format = _desktopFormat;
            importer.SetPlatformTextureSettings(desktopSettings);

            TextureImporterPlatformSettings androidSettings = importer.GetPlatformTextureSettings("Android");
            androidSettings.overridden = true;
            androidSettings.format = _androidFormat;
            importer.SetPlatformTextureSettings(androidSettings);

            TextureImporterPlatformSettings iosSettings = importer.GetPlatformTextureSettings("iPhone");
            iosSettings.overridden = true;
            iosSettings.format = _iosFormat;
            importer.SetPlatformTextureSettings(iosSettings);

            importer.SaveAndReimport();
        }
        AssetDatabase.Refresh();
        Debug.Log("? Optimization Wizard: プロジェクト内の全てのテクスチャを最適化しました。");
    }

    private void EnableInstancingForAllMaterialsInProject()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material != null) // シェーダーがインスタンシングに対応しているかどうかのチェックは不要、あるいはより高度なチェックが必要
            {
                if (material.enableInstancing == false)
                {
                    material.enableInstancing = true;
                    EditorUtility.SetDirty(material); // 変更を保存
                }
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("? Optimization Wizard: プロジェクト内の全てのマテリアルで GPU Instancing を有効にしました。");
    }

    private void SetSelectedObjectsStaticForBatching()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("? Optimization Wizard: オブジェクトが選択されていません。");
            return;
        }

        foreach (GameObject go in Selection.gameObjects)
        {
            SetGameObjectAndChildrenStatic(go);
        }
        Debug.Log("? Optimization Wizard: 選択中のオブジェクトとその子オブジェクトが Static Batching の対象に設定されました。");
    }

    private void SetGameObjectAndChildrenStatic(GameObject obj)
    {
        if (obj == null) return;

        // StaticEditorFlags.BatchingStatic のみ設定
        GameObjectUtility.SetStaticEditorFlags(obj, GameObjectUtility.GetStaticEditorFlags(obj) | StaticEditorFlags.BatchingStatic);

        foreach (Transform child in obj.transform)
        {
            SetGameObjectAndChildrenStatic(child.gameObject);
        }
    }

    private void ApplyAnimatorCullingMode()
    {
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators)
        {
            if (animator.cullingMode != _animatorCullingMode)
            {
                animator.cullingMode = _animatorCullingMode;
                EditorUtility.SetDirty(animator); // シーン内の変更を保存
            }
        }
        Debug.Log($"? Optimization Wizard: シーン内の全てのアニメーターの Culling Mode を {_animatorCullingMode} に設定しました。");
    }

    private void ApplyRecommendedQualitySettings()
    {
        // 現在アクティブなビルドターゲットのQuality Settingを取得
        int currentQualityLevel = QualitySettings.GetQualityLevel();
        QualitySettings.SetQualityLevel(currentQualityLevel); // 現在のクオリティレベルを再設定して変更を適用

        QualitySettings.shadowResolution = _shadowResolution;
        QualitySettings.pixelLightCount = _pixelLightCount;

        // 他にも調整できる設定がたくさんあります
        // 例: QualitySettings.shadowDistance = 50;
        // QualitySettings.antiAliasing = 2; // 2x MSAA

        Debug.Log("? Optimization Wizard: 現在のプラットフォームの Quality Settings を推奨設定に更新しました。");
    }
}