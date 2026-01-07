using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // UIのフォーカス制御に必要

public class TitleOptionManager : MonoBehaviour
{
    public GameObject OptionPanel; // オプションパネル

    // メインメニューのルート（全ボタンの親）に CanvasGroup をアタッチしてここに割り当て
    [SerializeField] private CanvasGroup mainMenuGroup;

    // オプションパネルを開いたときに最初に選択されるUI（例：オプションの最初のボタン）
    [SerializeField] private GameObject optionFirstSelected;

    // オプションパネルを閉じたときに戻るメインメニューのデフォルト選択ボタン
    [SerializeField] private GameObject mainFirstSelected;

    public bool isPaused = false; // オプションパネルが開いているかどうか

    [SerializeField] private Transform mainMenuRoot;

    // 元のNavigationを戻すために保存
    private List<Selectable> cachedSelectables = new();
    private List<Navigation> cachedNavigations = new();

    // オプションパネルを開閉するトグル処理
    public void ToggleOption()
    {
        if (isPaused) HideOption();
        else ShowOption();
    }

    // オプションパネルを表示
    public void ShowOption()
    {
        // ※ タイトル画面では Time.timeScale を変更しない方が良い
        // （ゲームプレイ中なら 0 にしてポーズ可能）
        // Time.timeScale = 0f;

        // メインメニューの操作とクリックを完全に無効化
        if (mainMenuGroup != null)
        {
            mainMenuGroup.interactable = false;   // ボタンの操作を無効化
            mainMenuGroup.blocksRaycasts = false; // マウス/タッチ入力を遮断
        }

        DisableMainMenuNavigation();

        // オプションパネルを有効化
        OptionPanel.SetActive(true);
        isPaused = true;

        // UIフォーカスをオプションパネル側の最初のボタンに移動
        if (optionFirstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(optionFirstSelected);
        }
    }

    // オプションパネルを非表示
    public void HideOption()
    {
        // オプションパネルを無効化
        OptionPanel.SetActive(false);

        RestoreMainMenuNavigation();

        // メインメニューの操作とクリックを再度有効化
        if (mainMenuGroup != null)
        {
            mainMenuGroup.interactable = true;  // ボタンの操作を復活
            mainMenuGroup.blocksRaycasts = true; // マウス/タッチ入力も復活
        }

        // ゲームプレイ中なら Time.timeScale を 1 に戻す
        // Time.timeScale = 1f;

        isPaused = false;

        // UIフォーカスをメインメニュー側のボタンに戻す
        if (mainFirstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainFirstSelected);
        }
    }

    // ─────────────────────────────────────────────
    // メインメニューのナビゲーションを一時的に無効化（矢印キー/スティック対策）
    // ─────────────────────────────────────────────
    private void DisableMainMenuNavigation()
    {
        cachedSelectables.Clear();
        cachedNavigations.Clear();

        if (mainMenuRoot == null) return;

        // 親以下の全Selectableを取得
        var selects = mainMenuRoot.GetComponentsInChildren<Selectable>(true);

        foreach (var s in selects)
        {
            cachedSelectables.Add(s);
            cachedNavigations.Add(s.navigation); // 元の設定を保存

            // NavigationをNoneにして、矢印/スティックで移動しないようにする
            var nav = s.navigation;
            nav.mode = Navigation.Mode.None;
            s.navigation = nav;
        }

        // もし現在選択がメインメニュー側ならクリア
        var current = EventSystem.current.currentSelectedGameObject;
        if (current != null && current.transform.IsChildOf(mainMenuRoot))
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // ─────────────────────────────────────────────
    // ナビゲーション設定を元に戻す
    // ─────────────────────────────────────────────
    private void RestoreMainMenuNavigation()
    {
        for (int i = 0; i < cachedSelectables.Count; i++)
        {
            if (cachedSelectables[i] != null)
                cachedSelectables[i].navigation = cachedNavigations[i];
        }
        cachedSelectables.Clear();
        cachedNavigations.Clear();
    }
}
