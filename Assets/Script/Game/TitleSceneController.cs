using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField] private string videoSceneName = "VideoScene"; // 動画シーン名
    [SerializeField] private float idleTimeToReturn = 5f;          // 無操作で戻る秒数

    private float idleTimer = 0f;

    void Update()
    {
        // 入力があればタイマーをリセット
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            idleTimer = 0f;
        }
        else
        {
            // 入力がない間カウントを進める
            idleTimer += Time.deltaTime;

            if (idleTimer >= idleTimeToReturn)
            {
                // 動画シーンへ戻る
                SceneManager.LoadScene(videoSceneName);
            }
        }
    }
}
