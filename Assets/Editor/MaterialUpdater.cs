using UnityEditor;
using UnityEngine;

public class MaterialUpdater : EditorWindow
{
	// ★ 更新したいシェーダーの名前をここに正確に入力 ★
	private const string SHADER_NAME = "Custom/URPToonCodeShader_Advanced_MultiLight";
	// (もし "RadiusHack" の方を使っているなら、そちらの名前に変えてください)

	private const string PROPERTY_NAME = "_Smoothness";
	private const float NEW_VALUE = 0.4f;

	[MenuItem("Tools/Update Toon Materials _Smoothness")]
	public static void UpdateMaterials()
	{
		if (!EditorUtility.DisplayDialog(
			"マテリアル一括更新",
			$"プロジェクト内の '{SHADER_NAME}' を使用している全マテリアルの\n" +
			$"'{PROPERTY_NAME}' の値を {NEW_VALUE} に更新します。\n\n" +
			"この操作は元に戻せません (Undo不可)。\nよろしいですか？",
			"実行", "キャンセル"))
		{
			return;
		}

		// プロジェクト内の全マテリアルのGUIDを検索
		string[] guids = AssetDatabase.FindAssets("t:Material");
		int count = 0;

		foreach (string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);
			Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

			if (mat != null && mat.shader.name == SHADER_NAME)
			{
				// プロパティが存在するか確認してからセット
				if (mat.HasProperty(PROPERTY_NAME))
				{
					mat.SetFloat(PROPERTY_NAME, NEW_VALUE);
					EditorUtility.SetDirty(mat); // 変更を保存
					count++;
				}
			}
		}

		AssetDatabase.SaveAssets(); // プロジェクト全体のアセット変更を保存
		AssetDatabase.Refresh();

		Debug.Log($"[MaterialUpdater] {count} 個のマテリアルの '{PROPERTY_NAME}' を {NEW_VALUE} に更新しました。");
	}
}