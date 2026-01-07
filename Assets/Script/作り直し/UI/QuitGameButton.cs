using UnityEngine;

public class QuitGameButton : MonoBehaviour
{
    // ボタンの On Click () にこれを登録してください
    public void OnClickQuit()
    {
        Debug.Log("ゲーム終了ボタンが押されました。");

#if UNITY_EDITOR
        // Unityエディタ上での実行を停止する（テスト用）
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 本番（ビルド後）のアプリケーションを終了する
            Application.Quit();
#endif
    }
}