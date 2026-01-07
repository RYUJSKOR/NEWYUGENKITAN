using UnityEngine;
using System.Collections;

public class SkyManager : MonoBehaviour
{
    // インスペクターで設定する項目

    [Header("必須コンポーネント")]
    [Tooltip("シーン内のディレクショナルライト")]
    public Light directionalLight;

    [Header("時間帯ごとの設定")]
    [Tooltip("昼、夕方、夜などの設定プロファイルの配列")]
    public TimeProfile[] timeProfiles;

    [Header("移行設定")]
    [Tooltip("時間帯が切り替わる際の移行にかかる時間（秒）")]
    public float transitionDuration = 5.0f;


    // 内部で使用する変数

    private Material skyboxInstance; // 元のマテリアルを保護するための、実行中専用のスカイボックスマテリアル
    private int currentProfileIndex = -1; // 現在のプロファイル番号
    private Coroutine transitionCoroutine; // 実行中の移行処理を保持


    // Unityのライフサイクルメソッド

    private void Awake()
    {
        // ライトが設定されていなければ、シーンから自動で探す
        if (directionalLight == null)
        {
            directionalLight = FindObjectOfType<Light>();
            if (directionalLight == null)
            {
                Debug.LogError("シーンにディレクショナルライトが見つかりません！");
                enabled = false;
                return;
            }
        }

        // 【重要】現在のスカイボックスマテリアルのコピー（インスタンス）を作成し、
        // 今後の変更が元のアセットに影響しないようにする。
        if (RenderSettings.skybox != null)
        {
            skyboxInstance = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skyboxInstance;
        }
        else
        {
            Debug.LogError("シーンにスカイボックスが設定されていません！ (Window > Rendering > Lighting)");
            enabled = false;
        }
    }

    private void Start()
    {
        // ゲーム開始時に、最初のプロファイル（0番）を瞬時に適用する
        if (timeProfiles.Length > 0)
        {
            ApplyProfileInstantly(timeProfiles[0]);
            currentProfileIndex = 0;
        }
        else
        {
            Debug.LogError("Time Profilesが1つも設定されていません！");
        }
    }

    // テスト用に、キーボードの矢印キーで時間帯を進めたり戻したりする
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int nextIndex = (currentProfileIndex + 1) % timeProfiles.Length;
            ChangeTime(nextIndex);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int prevIndex = (currentProfileIndex - 1 + timeProfiles.Length) % timeProfiles.Length;
            ChangeTime(prevIndex);
        }
    }


    // publicメソッド

    /// <summary>
    /// 指定した番号のプロファイルへ、時間をかけて滑らかに移行します。
    /// </summary>
    /// <param name="profileIndex">timeProfiles配列の要素番号</param>
    public void ChangeTime(int profileIndex)
    {
        if (profileIndex < 0 || profileIndex >= timeProfiles.Length)
        {
            Debug.LogError("無効なプロファイル番号です: " + profileIndex);
            return;
        }

        // 既に実行中の移行処理があれば停止する
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        // 新しい移行処理を開始する
        transitionCoroutine = StartCoroutine(TransitionToProfile(timeProfiles[profileIndex]));
        currentProfileIndex = profileIndex;
    }


    // 内部処理

    // プロファイルの設定を瞬時に適用する
    private void ApplyProfileInstantly(TimeProfile profile)
    {
        directionalLight.color = profile.lightColor;
        directionalLight.intensity = profile.lightIntensity;

        skyboxInstance.SetColor("_TintColor", profile.skyTintColor);
        skyboxInstance.SetFloat("_Exposure", profile.skyExposure);
        skyboxInstance.SetFloat("_AtmosphereThickness", profile.atmosphereThickness);
    }

    // プロファイルへ滑らかに移行するためのコルーチン
    private IEnumerator TransitionToProfile(TimeProfile targetProfile)
    {
        // 移行前の状態を記録
        Color startLightColor = directionalLight.color;
        float startLightIntensity = directionalLight.intensity;

        Color startSkyTintColor = skyboxInstance.GetColor("_TintColor");
        float startSkyExposure = skyboxInstance.GetFloat("_Exposure");
        float startAtmosphere = skyboxInstance.GetFloat("_AtmosphereThickness");

        float elapsedTime = 0f;

        // 移行時間中、毎フレーム値を更新する
        while (elapsedTime < transitionDuration)
        {
            // 0から1への進行度を計算
            float t = elapsedTime / transitionDuration;

            // 各プロパティを線形補間(Lerp)で滑らかに変化させる
            directionalLight.color = Color.Lerp(startLightColor, targetProfile.lightColor, t);
            directionalLight.intensity = Mathf.Lerp(startLightIntensity, targetProfile.lightIntensity, t);

            skyboxInstance.SetColor("_TintColor", Color.Lerp(startSkyTintColor, targetProfile.skyTintColor, t));
            skyboxInstance.SetFloat("_Exposure", Mathf.Lerp(startSkyExposure, targetProfile.skyExposure, t));
            skyboxInstance.SetFloat("_AtmosphereThickness", Mathf.Lerp(startAtmosphere, targetProfile.atmosphereThickness, t));

            // 環境光の更新（負荷が高めなので、移行中は毎フレーム呼ばなくても良い場合もある）
            DynamicGI.UpdateEnvironment();

            elapsedTime += Time.deltaTime;
            yield return null; // 次のフレームまで待つ
        }

        // 移行完了後、最終的な値を正確に設定
        ApplyProfileInstantly(targetProfile);
        transitionCoroutine = null;
    }
}


/// <summary>
/// 時間帯ごとの設定を保持するためのクラス
/// </summary>
[System.Serializable]
public class TimeProfile
{
    public string name;

    [Header("ライト設定")]
    public Color lightColor = Color.white;
    [Range(0f, 8f)]
    public float lightIntensity = 1f;

    [Header("スカイボックス設定")]
    [Tooltip("Skybox/Panoramicシェーダーなどで使用")]
    public Color skyTintColor = Color.white;
    [Tooltip("Procedural Skyboxなどで使用")]
    [Range(0f, 8f)]
    public float skyExposure = 1f;
    [Tooltip("Procedural Skyboxなどで使用")]
    [Range(0f, 5f)]
    public float atmosphereThickness = 1f;
}