using UnityEngine;

// このスクリプトがアタッチされたオブジェクトにAudioSourceが必須であることを保証する
[RequireComponent(typeof(AudioSource))]
public class AudioTimeScaler : MonoBehaviour
{
    private AudioSource audioSource;
    private bool wasPaused = false; // 直前のフレームでポーズされていたか

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Time.timeScaleがほぼゼロ（停止）の場合
        if (Time.timeScale < 0.01f)
        {
            // まだポーズされていなければポーズする
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
                wasPaused = true;
            }
        }
        // Time.timeScaleが動いている場合
        else
        {
            // 直前までポーズされていたなら再生を再開
            if (wasPaused)
            {
                audioSource.UnPause();
                wasPaused = false;
            }

            // pitchをTime.timeScaleと同期させる
            audioSource.pitch = Time.timeScale;
        }
    }
}