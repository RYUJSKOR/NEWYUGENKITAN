using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 新InputSystem用（ある場合）
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.UI;

public class InputGlobalLocker : MonoBehaviour
{
    // ーーー シングルトン参照 ーーー
    // （どこからでも入力ロックを呼べるようにする）
    public static InputGlobalLocker Instance;

    // ーーー 現在ロック中かどうかのフラグ ーーー
    public bool IsLocked { get; private set; }

    // ーーー 起動時：シングルトンセット ーーー
    private void Awake()
    {
        // ーーー 既に存在すれば自分を破棄 ーーー
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        // ーーー 自分をインスタンスに設定 ーーー
        Instance = this;
        // ーーー シーンを跨いでも残したい場合は有効化（任意） ーーー
        // DontDestroyOnLoad(gameObject);
    }

    // ーーー 公開API：全入力をロック（UI含む） ーーー
    public void LockAll()
    {
        // ーーー 既にロック中なら何もしない ーーー
        if (IsLocked) return;

        // ーーー フラグON ーーー
        IsLocked = true;

        // ーーー EventSystemがあれば無効化（UIナビ/クリック完全停止） ーーー
        if (EventSystem.current != null)
        {
            // ーーー 入力モジュールを停止（旧/新UI両対応） ーーー
            var sim = EventSystem.current.currentInputModule as StandaloneInputModule;
            if (sim) sim.enabled = false;

#if ENABLE_INPUT_SYSTEM
            var ns = EventSystem.current.currentInputModule as InputSystemUIInputModule;
            if (ns) ns.enabled = false;
#endif

            // ーーー EventSystem自体も停止（より確実に無効化） ーーー
            EventSystem.current.enabled = false;
        }

#if ENABLE_INPUT_SYSTEM
        // ーーー 新InputSystemのプレイヤー入力を全部停止 ーーー
        foreach (var pi in FindObjectsByType<PlayerInput>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            // ーーー コンポーネントごと停止 ーーー
            pi.enabled = false;
        }
#endif
    }

    // ーーー 公開API：全入力のロック解除 ーーー
    public void UnlockAll()
    {
        // ーーー ロック中でなければ何もしない ーーー
        if (!IsLocked) return;

        // ーーー EventSystemがあれば再有効化 ーーー
        if (EventSystem.current != null)
        {
            // ーーー EventSystem本体を有効化 ーーー
            EventSystem.current.enabled = true;

            // ーーー 旧/新UI入力モジュールを可能なら再度有効化 ーーー
            var sim = EventSystem.current.currentInputModule as StandaloneInputModule;
            if (sim) sim.enabled = true;

#if ENABLE_INPUT_SYSTEM
            var ns = EventSystem.current.currentInputModule as InputSystemUIInputModule;
            if (ns) ns.enabled = true;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        // ーーー 新InputSystemのプレイヤー入力を再有効化 ーーー
        foreach (var pi in FindObjectsByType<PlayerInput>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            // ーーー コンポーネントを再度有効化 ーーー
            pi.enabled = true;
        }
#endif

        // ーーー フラグOFF ーーー
        IsLocked = false;
    }

    // ーーー 公開API：指定秒数だけロック（アンスケールド時間推奨） ーーー
    public System.Collections.IEnumerator LockForSeconds(float seconds, bool useUnscaledTime = true)
    {
        // ーーー まずロック開始 ーーー
        LockAll();

        // ーーー 時間経過を待機（Unscaledを使えばポーズ中でも正確） ーーー
        float t = 0f;
        while (t < seconds)
        {
            // ーーー 経過時間を加算（Unscaled/Scaled切替） ーーー
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            // ーーー 次のフレームまで待機 ーーー
            yield return null;
        }

        // ーーー ロック解除 ーーー
        UnlockAll();
    }
}
