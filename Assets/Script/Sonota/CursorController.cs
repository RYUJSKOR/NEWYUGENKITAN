using UnityEngine;

public class CursorController : MonoBehaviour
{
    public bool isVisible;
    void Start()
    {
        // 初期状態ではカーソルをロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        isVisible = Cursor.visible;
        // Escキーが押されたら
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 現在のロック状態に応じて処理を切り替える
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                // カーソルのロックを解除し、表示する
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // カーソルをロックし、非表示にする
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}