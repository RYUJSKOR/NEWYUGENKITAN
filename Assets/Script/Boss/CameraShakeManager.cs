using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static CameraShakeManager instance;

    // シェイクの基本設定
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.7f;
    private float dampingSpeed = 1.0f;

    // カメラの初期位置
    private Vector3 initialPosition;

    void Awake()
    {
        // シングルトンパターンの実装
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // 起動時にカメラの初期位置を記録
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // シェイク時間が残っていればカメラを揺らす
        if (shakeDuration > 0)
        {
            transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            // 時間が尽きたら初期位置に戻す
            shakeDuration = 0f;
            transform.localPosition = initialPosition;
        }
    }

    /// <summary>
    /// 外部からカメラシェイクを呼び出すための関数
    /// </summary>
    /// <param name="duration">揺れの持続時間（秒）</param>
    /// <param name="magnitude">揺れの強さ</param>
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}