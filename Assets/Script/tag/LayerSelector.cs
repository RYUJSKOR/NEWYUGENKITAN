using UnityEngine;

using UnityEditor;

public class LayerSelectorAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LayerSelectorAttribute))]
public class LayerSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType == SerializedPropertyType.Integer)
        {
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Use LayerSelector with int only");
        }
    }
}
#endif
