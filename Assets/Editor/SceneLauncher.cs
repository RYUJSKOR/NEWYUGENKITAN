using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public class SceneLauncher : EditorWindow
{
    // メニューバーの [Tools] -> [Scene Launcher] でウィンドウを開く
    [MenuItem("Tools/Scene Launcher")]
    public static void ShowWindow()
    {
        GetWindow<SceneLauncher>("Scene Launcher");
    }

    void OnGUI()
    {
        GUILayout.Label("Scenes in Build Settings", EditorStyles.boldLabel);

        // ビルドセッティングに登録されているシーンを取得
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                // パスからシーン名だけを抽出
                string sceneName = Path.GetFileNameWithoutExtension(scene.path);

                // ボタンを表示し、押されたらそのシーンを開く
                if (GUILayout.Button(sceneName))
                {
                    OpenScene(scene.path);
                }
            }
        }
    }

    private void OpenScene(string scenePath)
    {
        // 現在のシーンが保存されていない場合、保存するか聞く（データ消失防止）
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}