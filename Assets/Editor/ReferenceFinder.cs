#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ReferenceFinder : EditorWindow
{
    [MenuItem("Tools/Find References in Scene")]
    public static void FindReferences()
    {
        GameObject target = Selection.activeGameObject;
        if (target == null)
        {
            Debug.LogWarning("探したいオブジェクトを選択してから実行してください。");
            return;
        }

        Debug.Log($"<color=cyan>--- '{target.name}' を参照しているオブジェクトを検索開始 ---</color>");

        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        int count = 0;

        foreach (var obj in allObjects)
        {
            // シーン上のオブジェクト以外（プレハブの中身など）は除外
            if (obj.hideFlags != HideFlags.None || EditorUtility.IsPersistent(obj)) continue;
            if (obj == target) continue; // 自分自身は除外

            Component[] components = obj.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component == null) continue;

                SerializedObject so = new SerializedObject(component);
                SerializedProperty sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == target)
                        {
                            Debug.Log($"発見: <color=yellow>{obj.name}</color> のコンポーネント <color=orange>{component.GetType().Name}</color> の変数 '{sp.displayName}' で参照されています。", obj);
                            count++;
                        }
                    }
                }
            }
        }

        if (count == 0) Debug.Log("参照しているオブジェクトは見つかりませんでした。");
        else Debug.Log($"<color=green>検索終了: {count} 件見つかりました。</color>");
    }
}
#endif