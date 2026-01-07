using System.Collections.Generic;

public static class StageLoaderInfo
{
    // 選択されたステ?ジのシ?ン名（SceneLoaderで使用）
    public static string SelectedStageSceneName;

    // Addressablesからロ?ドすべきキ?一覧
    public static string[] AddressKeysToLoad;

    // ステ?ジ名ごとに対応するロ?ド対象のリ??スキ?を登?
    private static readonly Dictionary<string, string[]> resourceMap = new()
    {
        { "Stage1", new[] { "lantern", "yuukaku" } },
        //{ "Stage2", new[] { "WallB", "TreeC", "EnemyA" } },
        //{ "Stage3", new[] { "BridgeX", "WaterY" } },
        // 必要なステ?ジをここに追加
    };

    // ステ?ジ名に応じてリ??スキ?を設定する関数
    public static void SetResourcesForStage(string stageName)
    {
        SelectedStageSceneName = stageName;

        if (resourceMap.ContainsKey(stageName))
        {
            AddressKeysToLoad = resourceMap[stageName];
        }
        else
        {
            AddressKeysToLoad = new string[0]; // 該当なし → ロ?ドするものがない
        }
    }
}
