// このスクリプトは「縦方向の上下ラップ移動のみ」を設定する最小実装です
using UnityEngine;
// UIのSelectable(Button等)を扱うために必要
using UnityEngine.UI;
// 汎用コレクションを使用するため
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIWrapVertical : MonoBehaviour
{
    // ← 子孫のSelectableを取得する際に「非アクティブも含める」か（通常はfalseを推奨）
    [SerializeField] private bool includeInactive = false;

    // ← 収集したSelectableの作業リスト（Hierarchy順で並ぶ）
    private readonly List<Selectable> items = new List<Selectable>();

    [SerializeField] private bool selectTopOnEnable = true;

    // ← 有効化時に一度配線を構築する
    private void OnEnable()
    {
        // ← 配線を再構築
        Rebuild();
    }

    // ← 子の増減や並び替えがあった時に自動で配線を再構築する
    private void OnTransformChildrenChanged()
    {
        // ← 配線を再構築
        Rebuild();
    }

    // ← 外部から明示的に再構築したい場合に呼び出す
    public void ForceRebuild()
    {
        // ← 配線を再構築
        Rebuild();
    }

    // ← 配線の主処理：子孫のSelectableを階層順に集め、上下のみラップ接続する
    private void Rebuild()
    {
        // ← まずリストをクリア
        items.Clear();

        // ← 自身を起点として階層順（上から順に）でSelectableを収集
        CollectSelectablesInHierarchyOrder(transform);

        // ← 使用対象が0 or 1個なら特別対応（1個は自分自身へ接続しても可）
        if (items.Count == 0) return;

        // ← 要素数が1個の場合も上下は自分自身に向けておく（操作感の破綻を防ぐため）
        if (items.Count == 1)
        {
            // ← ただ1つのSelectableを取得
            var s = items[0];
            // ← 現在のNavigationを取得
            var nav = s.navigation;
            // ← 明示的ナビゲーションに切替
            nav.mode = Navigation.Mode.Explicit;
            // ← 上下とも自分自身（左右は無効）
            nav.selectOnUp = s;
            nav.selectOnDown = s;
            nav.selectOnLeft = null;
            nav.selectOnRight = null;
            // ← 設定を反映
            s.navigation = nav;
            // ← これで終了
            return;
        }

        // ← 2個以上ある場合は上下をラップで接続
        for (int i = 0; i < items.Count; i++)
        {
            // ← 対象Selectable
            var s = items[i];
            // ← 既存Navigationを取得
            var nav = s.navigation;
            // ← 明示的ナビゲーションに切替
            nav.mode = Navigation.Mode.Explicit;

            // ← 前（上）と次（下）のインデックスをラップで算出
            int prev = (i - 1 + items.Count) % items.Count;
            int next = (i + 1) % items.Count;

            // ← 上下のみ接続（左右は完全に未設定）
            nav.selectOnUp = items[prev];
            nav.selectOnDown = items[next];
            nav.selectOnLeft = null;
            nav.selectOnRight = null;

            // ← Navigation設定を反映
            s.navigation = nav;

            if (i == 0 && selectTopOnEnable && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                var sel = items[0] as Selectable;
                if (sel != null) sel.Select();
                EventSystem.current.SetSelectedGameObject(items[0].gameObject);
            }
        }

    }

    // ← Transform配下を「ヒエラルキー順プレオーダー」で走査し、Selectableを拾う
    private void CollectSelectablesInHierarchyOrder(Transform root)
    {
        // ← 直下の子を、インスペクタ上の順番（SiblingIndex順）で走査
        int childCount = root.childCount;
        for (int i = 0; i < childCount; i++)
        {
            // ← i番目の子を取得
            var child = root.GetChild(i);

            // ← 子自身にSelectableが付いているか確認
            var sel = child.GetComponent<Selectable>();

            // ← includeInactive=falseのときは「アクティブな階層のみ」対象にする
            bool activeOK = includeInactive || child.gameObject.activeInHierarchy;

            // ← 条件を満たし、かつSelectableがあれば追加
            if (activeOK && sel != null)
            {
                // ← リストへ追加（この順がナビゲーション順となる）
                items.Add(sel);
            }

            // ← 再帰的に孫以降も同じ順序で探索
            CollectSelectablesInHierarchyOrder(child);
        }
    }
}