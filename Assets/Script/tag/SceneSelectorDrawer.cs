using UnityEditor;
using UnityEngine;

public class SceneSelectorAttribute : PropertyAttribute { }


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneSelectorAttribute))]
public class SceneSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [SceneSelector] with string.");
            return;
        }

        var scenes = EditorBuildSettings.scenes;
        string[] sceneNames = new string[scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            string path = scenes[i].path;
            sceneNames[i] = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        int selectedIndex = Mathf.Max(0, System.Array.IndexOf(sceneNames, property.stringValue));
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, sceneNames);

        if (selectedIndex >= 0 && selectedIndex < sceneNames.Length)
        {
            property.stringValue = sceneNames[selectedIndex];
        }
    }
}
#endif
