using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq; // リスト操作用

public class SceneFontChanger : EditorWindow
{
    private Font _targetLegacyFont;
    private TMP_FontAsset _targetTMPFont;

    // 範囲指定用のフラグ
    private bool _onlySelection = false;

    [MenuItem("Tools/Font Changer Pro")]
    public static void ShowWindow()
    {
        GetWindow<SceneFontChanger>("Font Changer");
    }

    private void OnGUI()
    {
        GUILayout.Label("フォント一括置換ツール", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // --- 設定エリア ---
        // トグル（チェックボックス）で対象範囲を切り替え
        _onlySelection = EditorGUILayout.Toggle("選択した物とその子のみ", _onlySelection);

        if (_onlySelection)
        {
            EditorGUILayout.HelpBox("ヒエラルキーで選択中のオブジェクトとその配下全てが対象です。", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("シーン内の「非表示」を含む全てのオブジェクトが対象です。", MessageType.Info);
        }

        GUILayout.Space(10);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1)); // 区切り線
        GUILayout.Space(10);

        // --- Legacy Text ---
        GUILayout.Label("Legacy Text (uGUI)", EditorStyles.boldLabel);
        _targetLegacyFont = (Font)EditorGUILayout.ObjectField("New Font", _targetLegacyFont, typeof(Font), false);

        if (GUILayout.Button("Legacy Text 変更"))
        {
            ChangeFonts<Text>((textComp) =>
            {
                Undo.RecordObject(textComp, "Change Legacy Font");
                textComp.font = _targetLegacyFont;
                EditorUtility.SetDirty(textComp);
            });
        }

        GUILayout.Space(20);

        // --- TextMeshPro ---
        GUILayout.Label("TextMeshPro (TMP)", EditorStyles.boldLabel);
        _targetTMPFont = (TMP_FontAsset)EditorGUILayout.ObjectField("New Font Asset", _targetTMPFont, typeof(TMP_FontAsset), false);

        if (GUILayout.Button("TextMeshPro 変更"))
        {
            ChangeFonts<TMP_Text>((tmpComp) =>
            {
                Undo.RecordObject(tmpComp, "Change TMP Font");
                tmpComp.font = _targetTMPFont;
                EditorUtility.SetDirty(tmpComp);
            });
        }
    }

    // 共通の処理ロジック
    private void ChangeFonts<T>(System.Action<T> applyFontAction) where T : Component
    {
        // フォントがセットされているか確認
        if (typeof(T) == typeof(Text) && _targetLegacyFont == null) { Debug.LogWarning("フォントが未設定です"); return; }
        if (typeof(T) == typeof(TMP_Text) && _targetTMPFont == null) { Debug.LogWarning("TMPフォントが未設定です"); return; }

        List<T> targetComponents = new List<T>();

        if (_onlySelection)
        {
            // 選択しているオブジェクト（複数可）から子階層を含めて取得（非表示も含む）
            foreach (GameObject obj in Selection.gameObjects)
            {
                targetComponents.AddRange(obj.GetComponentsInChildren<T>(true));
            }
        }
        else
        {
            // シーン全体から取得（非表示も含む：FindObjectsInactive.Include が重要）
            targetComponents = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
        }

        // 重複を除外（親子で選択した場合などにダブるのを防ぐ）
        targetComponents = targetComponents.Distinct().ToList();

        if (targetComponents.Count == 0)
        {
            Debug.Log("対象のテキストオブジェクトが見つかりませんでした。");
            return;
        }

        // 実際の変更処理
        foreach (T comp in targetComponents)
        {
            applyFontAction(comp);
        }

        Debug.Log($"{targetComponents.Count} 個の {typeof(T).Name} を変更しました。");
    }
}