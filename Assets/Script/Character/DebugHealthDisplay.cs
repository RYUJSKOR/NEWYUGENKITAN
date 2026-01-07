using TMPro;
using UnityEngine;

public class DebugHealthDisplay : MonoBehaviour
{
    private CharacterHealthManager healthManager;
    private TextMeshPro textMeshPro;
    private Camera mainCamera;

    [SerializeField] private float yOffset = 1.0f; // 頭上に表示するオフセット

#if UNITY_EDITOR
    private void OnValidate()
    {
        // TextMeshProが存在しない場合は自動的に追加する（エディタ上のみ）
        if (GetComponentInChildren<TMPro.TextMeshPro>() == null)
        {
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.up * yOffset;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one * 0.1f; // サイズ調整

            textMeshPro = textObj.AddComponent<TMPro.TextMeshPro>();
            textMeshPro.alignment = TMPro.TextAlignmentOptions.Center;
            textMeshPro.fontSize = 36;
            textMeshPro.color = Color.green;
            textMeshPro.text = "---";
        }
        else
        {
            textMeshPro = GetComponentInChildren<TMPro.TextMeshPro>();
            textMeshPro.transform.localPosition = Vector3.up * yOffset;
        }
    }
#endif

    void Start()
    {
        healthManager = GetComponent<CharacterHealthManager>();
        if (healthManager == null)
        {
            Debug.LogError("CharacterHealthManager コンポーネントが見つかりません！", this);
            enabled = false;
            return;
        }

        // TextMeshProを子オブジェクトから探す
        textMeshPro = GetComponentInChildren<TMPro.TextMeshPro>();
        if (textMeshPro == null)
        {
            // TextMeshProが存在しない場合は動的に作成
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(transform);
            textObj.transform.localPosition = Vector3.up * yOffset;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one * 0.1f; // サイズ調整

            textMeshPro = textObj.AddComponent<TMPro.TextMeshPro>();
            textMeshPro.alignment = TMPro.TextAlignmentOptions.Center;
            textMeshPro.fontSize = 36;
            textMeshPro.color = Color.green;
            textMeshPro.text = healthManager.GetHealth().ToString("F1"); // 初期体力表示
        }

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("MainCamera がシーンに存在しません！", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (healthManager != null && textMeshPro != null && mainCamera != null)
        {
            textMeshPro.text = healthManager.GetHealth().ToString("F1");

            // カメラの方向を向くようにする
            textMeshPro.transform.LookAt(textMeshPro.transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }
}