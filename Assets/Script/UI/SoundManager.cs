using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio; // AudioMixerを使うために必要

public class SoundManager : MonoBehaviour
{
    // Mixer本体
    public AudioMixer masterMixer;

    // 各スライダー
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider seSlider;

    // 各ボリューム表示テキスト
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI bgmVolumeText;
    public TextMeshProUGUI seVolumeText;

    void Start()
    {
        // --- 初期値の設定 ---
        // PlayerPrefsなどから保存した値を取得（なければデフォルト値）
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float seVol = PlayerPrefs.GetFloat("SEVolume", 1f);

        // スライダーの値を更新
        masterSlider.value = masterVol;
        bgmSlider.value = bgmVol;
        seSlider.value = seVol;

        // Mixerとテキストの値を更新
        SetMasterVolume(masterVol);
        SetBGMVolume(bgmVol);
        SetSEVolume(seVol);

        // --- スライダーの値が変更された時のイベントリスナーを設定 ---
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        seSlider.onValueChanged.AddListener(SetSEVolume);
    }

    // マスター音量を設定
    public void SetMasterVolume(float sliderValue)
    {
        float volume = ConvertToDecibel(sliderValue);
        masterMixer.SetFloat("MasterVolume", volume); // 公開したパラメータ名と一致させる
        UpdateVolumeText(masterVolumeText, sliderValue);
        PlayerPrefs.SetFloat("MasterVolume", sliderValue); // 値を保存
    }

    // BGM音量を設定
    public void SetBGMVolume(float sliderValue)
    {
        float volume = ConvertToDecibel(sliderValue);
        masterMixer.SetFloat("BGMVolume", volume);
        UpdateVolumeText(bgmVolumeText, sliderValue);
        PlayerPrefs.SetFloat("BGMVolume", sliderValue);
    }

    // SE音量を設定
    public void SetSEVolume(float sliderValue)
    {
        float volume = ConvertToDecibel(sliderValue);
        masterMixer.SetFloat("SEVolume", volume);
        UpdateVolumeText(seVolumeText, sliderValue);
        PlayerPrefs.SetFloat("SEVolume", sliderValue);
    }

    // テキスト更新
    private void UpdateVolumeText(TextMeshProUGUI textElement, float value)
    {
        int percent = Mathf.RoundToInt(value * 100);
        textElement.text = percent.ToString(); // シンプルにパーセントだけ表示する例
    }

    // スライダーの値(0~1)をデシベル(-80~0)に変換する
    private float ConvertToDecibel(float sliderValue)
    {
        // 0の時に-∞になるのを防ぐ
        if (sliderValue < 0.0001f)
        {
            return -80f;
        }
        return Mathf.Log10(sliderValue) * 20;
    }
}