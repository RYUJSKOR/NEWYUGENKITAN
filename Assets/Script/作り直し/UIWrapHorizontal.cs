// このスクリプトは「横方向の左右ラップ移動のみ」を設定する最小実装です
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIWrapHorizontal : MonoBehaviour
{
    [SerializeField] private bool includeInactive = false;

    private readonly List<Selectable> items = new List<Selectable>();

    // 変数名を「最初に選択する」に変更
    [SerializeField] private bool selectFirstOnEnable = true;

    private void OnEnable()
    {
        Rebuild();
    }

    private void OnTransformChildrenChanged()
    {
        Rebuild();
    }

    public void ForceRebuild()
    {
        Rebuild();
    }

    private void Rebuild()
    {
        items.Clear();
        CollectSelectablesInHierarchyOrder(transform);

        if (items.Count == 0) return;

        // ===== ★ 修正箇所 1/2 （1個の場合） =====
        if (items.Count == 1)
        {
            var s = items[0];
            var nav = s.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // 左右を自分自身に、上下を無効に
            nav.selectOnLeft = s;
            nav.selectOnRight = s;
            nav.selectOnUp = null;
            nav.selectOnDown = null;

            s.navigation = nav;
            return;
        }

        // ===== ★ 修正箇所 2/2 （2個以上の場合） =====
        for (int i = 0; i < items.Count; i++)
        {
            var s = items[i];
            var nav = s.navigation;
            nav.mode = Navigation.Mode.Explicit;

            // 前（左）と次（右）のインデックスをラップで算出
            int prev = (i - 1 + items.Count) % items.Count;
            int next = (i + 1) % items.Count;

            // 左右のみ接続（上下は完全に未設定）
            nav.selectOnLeft = items[prev];
            nav.selectOnRight = items[next];
            nav.selectOnUp = null;
            nav.selectOnDown = null;

            s.navigation = nav;

            // 最初の要素を選択するロジック (変数名を変更)
            if (i == 0 && selectFirstOnEnable && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                var sel = items[0] as Selectable;
                if (sel != null) sel.Select();
                EventSystem.current.SetSelectedGameObject(items[0].gameObject);
            }
        }
    }

    // ===== 変更なし (元スクリプトと同じ) =====
    // Transform配下を「ヒエラルキー順プレオーダー」で走査し、Selectableを拾う
    private void CollectSelectablesInHierarchyOrder(Transform root)
    {
        int childCount = root.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = root.GetChild(i);
            var sel = child.GetComponent<Selectable>();
            bool activeOK = includeInactive || child.gameObject.activeInHierarchy;

            if (activeOK && sel != null)
            {
                items.Add(sel);
            }
            CollectSelectablesInHierarchyOrder(child);
        }
    }
}