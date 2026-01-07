using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 毎回Camera.mainを呼ぶのは少し負荷があるので、最初にキャッシュしておきます
        mainCamera = Camera.main;
    }

    // Updateの後に実行されるため、カメラの動きに追従する処理に適しています
    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // オブジェクトの向きをカメラの向きと完全に同じにする
            transform.rotation = mainCamera.transform.rotation;

            // もしモデルが90度回転しているなどで向きが合わない場合は、
            // 以下のようにオフセットをかけることで調整できます。
            // transform.rotation = mainCamera.transform.rotation * Quaternion.Euler(0, 180, 0);
        }
    }
}