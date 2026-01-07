using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIWrapGrid : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("横に何個ボタンが並んでいるか（列数）")]
    [SerializeField] private int columns = 1;

    [Tooltip("有効化時に左上を自動選択するか")]
    [SerializeField] private bool selectFirstOnEnable = true;

    private readonly List<Selectable> items = new List<Selectable>();

    private void OnEnable()
    {
        Rebuild();

        // 有効化時に左上を選択
        if (selectFirstOnEnable && items.Count > 0)
        {
            // 少し遅らせないとEventSystemが反応しないことがあるため
            StartCoroutine(SelectFirstLater());
        }
    }

    private System.Collections.IEnumerator SelectFirstLater()
    {
        yield return null; // 1フレーム待つ
        if (EventSystem.current != null && items.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(items[0].gameObject);
        }
    }

    // 子の数や並びが変わったら再構築
    private void OnTransformChildrenChanged()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        items.Clear();
        // 自分以下のSelectableを全取得
        foreach (Transform child in transform)
        {
            // アクティブなものだけ対象
            if (!child.gameObject.activeSelf) continue;

            var sel = child.GetComponent<Selectable>();
            if (sel != null) items.Add(sel);
        }

        Debug.Log($"<color=yellow>検出されたボタン数: {items.Count}</color>");
        foreach (var item in items) Debug.Log($" - {item.name}");

        int count = items.Count;
        if (count == 0) return;

        // 全ボタンのナビゲーションを設定
        for (int i = 0; i < count; i++)
        {
            var s = items[i];
            var nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;

            // --- 左右の計算 (ループあり) ---
            // 行の左端なら、同じ行の右端へ
            // 行の右端なら、同じ行の左端へ

            // 左へ: もし行の先頭(i % columns == 0)なら、その行の末尾(i + columns - 1)へ。ただしリスト範囲外なら調整
            int rowStart = (i / columns) * columns;
            int rowEnd = Mathf.Min(rowStart + columns - 1, count - 1);

            int leftIndex = (i == rowStart) ? rowEnd : i - 1;
            int rightIndex = (i == rowEnd) ? rowStart : i + 1;

            nav.selectOnLeft = items[leftIndex];
            nav.selectOnRight = items[rightIndex];

            // --- 上下の計算 (ループあり) ---
            // 列の上端なら、同じ列の下端へ
            // 列の下端なら、同じ列の上端へ

            int upIndex = i - columns;
            int downIndex = i + columns;

            // 上がはみ出る場合 -> 一番下の同じ列を探す
            if (upIndex < 0)
            {
                // リストの底まで降りる
                int lastRowIndex = (count - 1) / columns * columns + (i % columns);
                // もし最後の行のその列が空なら（不揃いなグリッド）、1つ上の行にする
                if (lastRowIndex >= count) lastRowIndex -= columns;
                upIndex = lastRowIndex;
            }

            // 下がはみ出る場合 -> 一番上の同じ列(i % columns)に戻る
            if (downIndex >= count)
            {
                downIndex = i % columns;
            }

            nav.selectOnUp = items[upIndex];
            nav.selectOnDown = items[downIndex];

            s.navigation = nav;

            Debug.Log($"設定: {s.name} の右は -> {(nav.selectOnRight != null ? nav.selectOnRight.name : "null")}");
        }
    }
}