using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class BuildReportParser : EditorWindow
{
    [MenuItem("Tools/Build Report To Text")]
    public static void ParseBuildLog()
    {
        // 1. Unityのログパスを特定 (ExpandEnvironmentVariables に修正)
        // Windowsの標準的なパス: %LOCALAPPDATA%\Unity\Editor\Editor.log
        string logPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        logPath = Path.Combine(logPath, "Unity", "Editor", "Editor.log");

        // もし上記で見つからない場合の予備ルート
        if (!File.Exists(logPath))
        {
            logPath = System.Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%\\Unity\\Editor\\Editor.log");
        }

        if (!File.Exists(logPath))
        {
            Debug.LogError($"[Error] Editor.logが見つかりません。パスを確認してください: {logPath}");
            EditorUtility.DisplayDialog("Error", "Editor.logが見つかりません。ビルドを一度完了させてください。", "OK");
            return;
        }

        // 2. ログファイルを読み込む (共有モードでロックを回避)
        string logContent = "";
        try
        {
            using (FileStream fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                logContent = reader.ReadToEnd();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("ログの読み込みに失敗しました: " + e.Message);
            return;
        }

        // 3. ビルドリポートの開始地点を探す
        // "Used Assets and % of total size" という文字列を探します
        string searchKeyword = "Used Assets and % of total size";
        int startIndex = logContent.LastIndexOf(searchKeyword);

        if (startIndex == -1)
        {
            EditorUtility.DisplayDialog("Error", "ログの中にビル드리포트(Build Report)が見つかりません。\nビルドが正常に終了した直後に実行してください。", "OK");
            return;
        }

        string reportSection = logContent.Substring(startIndex);

        // 4. "Assets/" で始まるパスを抽出 (正規表現)
        // スペースが含まれるパスにも対応するため、行末まで取得するように調整
        MatchCollection matches = Regex.Matches(reportSection, @"Assets/.+");

        HashSet<string> builtAssets = new HashSet<string>();
        foreach (Match match in matches)
        {
            string path = match.Value.Trim();

            // パスの後ろのサイズ情報 (例: 10.5 kb  0.1%) を除去
            // 通常、パスとサイズの間に2つ以上のスペースがあることを利用
            int lastSpace = path.LastIndexOf("  ");
            if (lastSpace != -1)
            {
                path = path.Substring(0, lastSpace).Trim();
            }

            if (!string.IsNullOrEmpty(path) && path.StartsWith("Assets/"))
            {
                builtAssets.Add(path);
            }
        }

        // 5. 結果を保存
        string savePath = "BuildIncludedAssets.txt";
        File.WriteAllLines(savePath, builtAssets);

        Debug.Log($"[BuildReport] 抽出完了: {builtAssets.Count} 個のアセットがビルドに含まれています。");
        EditorUtility.DisplayDialog("完了", $"{builtAssets.Count} 個のアセットを BuildIncludedAssets.txt に保存しました。", "OK");
    }
}