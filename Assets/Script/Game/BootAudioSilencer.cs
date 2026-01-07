// -------------- 目的 --------------
// シーン開始直後に勝手に鳴るSFX/BGMを一括で黙らせる応急パッチ
// （最初の0.2秒だけ全オーディオを停止 → その後自動で解除）
// ※ コンポーネントをシーンに付ける必要はありません（自動で生成）

using UnityEngine;                 // ← Unityの基本API
using System.Collections;          // ← コルーチン用
using System.Collections.Generic;

public class BootAudioSilencer : MonoBehaviour
{
    // ← 何秒ミュートするか（必要に応じて0.3〜1.0の間で調整）
    private const float GateSeconds = 0.8f;

    // 元の音量を保持
    private float _prevListenerVolume = 1f;

    // 自分が触ったAudioSource（元のmute状態）を記録して後で戻す
    private readonly List<(AudioSource src, bool wasMuted)> _touched = new List<(AudioSource, bool)>();

    // ゲーム起動/シーン読み込み前に一度だけ呼ばれる（自動インストール）
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        var go = new GameObject("BootAudioSilencer(Runner)");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<BootAudioSilencer>();
    }

    private void Awake()
    {
        // 現在の全体音量を保存してから 0 に
        _prevListenerVolume = AudioListener.volume;
        AudioListener.volume = 0f;

        // 現在存在する全AudioSourceを一旦mute（後で戻す）
        var sources = FindObjectsOfType<AudioSource>(true);
        foreach (var s in sources)
        {
            _touched.Add((s, s.mute));
            s.mute = true;
        }

        // 実時間で待機開始
        StartCoroutine(ReleaseGate());
    }

    private IEnumerator ReleaseGate()
    {
        // Time.timeScaleの影響を受けない待機
        yield return new WaitForSecondsRealtime(GateSeconds);

        // 触ったAudioSourceのmuteを元に戻す（存在していれば）
        foreach (var (src, wasMuted) in _touched)
        {
            if (src) src.mute = wasMuted;
        }
        _touched.Clear();

        // 全体音量を元に戻す
        AudioListener.volume = _prevListenerVolume;

        // ランナーを破棄
        Destroy(gameObject);
    }
}
