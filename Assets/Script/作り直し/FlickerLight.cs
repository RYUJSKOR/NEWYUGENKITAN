using System.Collections;
// using System.Collections.Generic; // Listを使わないので不要
using UnityEngine;

public class FluctuateChildLights : MonoBehaviour
{
	[Header("光の基本の強さ")]
	public float baseIntensity = 0.5f;

	[Header("変動の量（この数値の分だけ強弱がつく）")]
	public float fluctuationAmount = 0.2f;

	[Header("変動の速さ")]
	public float fluctuationSpeed = 1.0f;

	private Light[] _lights;

	// ★ 変更点: _originalIntensities リストは不要なので削除
	// private List<float> _originalIntensities = new List<float>();

	void Start()
	{
		_lights = GetComponentsInChildren<Light>();
		if (_lights.Length == 0)
		{
			Debug.LogError("子オブジェクトに Light コンポーネントが見つかりません。", this);
			return;
		}

		// ★ 変更点: 元のIntensityを記憶する処理を削除
		/*
        foreach (Light light in _lights)
        {
            _originalIntensities.Add(light.intensity);
        }
        */
	}

	// ★ 変更点: Updateメソッドをシンプルな計算に変更
	void Update()
	{
		if (_lights.Length == 0) return;

		// 1. パーリンノイズで -1.0 ~ 1.0 の滑らかな値を取得
		float noise = (Mathf.PerlinNoise(Time.time * fluctuationSpeed, 0.0f) * 2.0f) - 1.0f;

		// 2. 変動を計算
		float fluctuation = noise * fluctuationAmount; // (例: -0.2 ~ 0.2)

		// 3. 新しい光の強さを計算
		// (元の強さ(15)を無視し、インスペクタの「基本の強さ」だけを使う)
		float newIntensity = baseIntensity + fluctuation; // (例: 0.5 + (-0.2 ~ 0.2) = 0.3 ~ 0.7)

		// 4. すべてのライトに適用
		foreach (Light light in _lights)
		{
			// 光の強さがマイナスにならないように 0 でクランプ(制限)
			light.intensity = Mathf.Max(0.0f, newIntensity);
		}
	}
}