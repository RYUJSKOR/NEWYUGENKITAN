using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// リザルト画面などで「必ず既定のボタンを選択状態」にするヘルパー。
/// - EventSystem の First Selected が効かない環境でも確実に動作
/// - OnEnable/Start の“次のフレーム”で Select() を呼んで選択演出も発火
/// - 指定が無ければ子孫の Button から「一番上（Y最大）」を自動検出
/// </summary>
public class ResultDefaultSelect : MonoBehaviour
{
    [Header("最初に選択したいボタン（未設定なら自動検出）")]
    [SerializeField] private Selectable defaultSelectable;

    [Header("有効化のたびに再選択する（ポップアップ再表示時など）")]
    [SerializeField] private bool reselectOnEnable = true;

    [Header("選択前に待つフレーム数（UI生成/レイアウト後の安定化用）")]
    [SerializeField] private int delayFrames = 1;

    private Coroutine _selectCo;

    private void OnEnable()
    {
        if (reselectOnEnable)
        {
            if (_selectCo != null) StopCoroutine(_selectCo);
            _selectCo = StartCoroutine(SelectDeferred());
        }
    }

    private void Start()
    {
        // 初回も確実に選択（EventSystem 初期化後）
        if (_selectCo != null) StopCoroutine(_selectCo);
        _selectCo = StartCoroutine(SelectDeferred());
    }

    /// <summary>
    /// 外部から明示的に再選択したい場合に呼ぶ
    /// </summary>
    public void ReselectNow()
    {
        if (_selectCo != null) StopCoroutine(_selectCo);
        _selectCo = StartCoroutine(SelectDeferred());
    }

    private IEnumerator SelectDeferred()
    {
        // --- EventSystem / レイアウト安定化のために数フレーム待機 ---
        int frames = Mathf.Max(0, delayFrames);
        while (frames-- > 0) yield return null;
        // タイムスケール0でも確実に最後に回す
        yield return new WaitForEndOfFrame();

        if (EventSystem.current == null) yield break;

        // 対象を決定（指定が無ければ自動検出）
        var target = defaultSelectable ?? AutoFindTopMost();
        if (target == null) yield break;

        // 無効ボタンはスキップ
        if (!target.IsActive() || !target.interactable) yield break;

        // 既存選択をクリア → Select() → 現在選択を登録
        EventSystem.current.SetSelectedGameObject(null);

        // ★ 重要：Select() を呼ぶと Selected/OnSelect/アニメーションが確実に発火
        target.Select();

        // 併せて EventSystem に現在選択を登録（保険）
        EventSystem.current.SetSelectedGameObject(target.gameObject);

        _selectCo = null;
    }

    /// <summary>
    /// 子孫の Button から「一番上（ワールド座標の Y が最大）」を探す。
    /// 非表示や非インタラクトは除外。
    /// </summary>
    private Selectable AutoFindTopMost()
    {
        var buttons = GetComponentsInChildren<Button>(true);
        if (buttons == null || buttons.Length == 0) return null;

        return buttons
            .Where(b => b != null && b.IsActive() && b.interactable)
            .OrderByDescending(b => b.transform.position.y)
            .FirstOrDefault();
    }
}