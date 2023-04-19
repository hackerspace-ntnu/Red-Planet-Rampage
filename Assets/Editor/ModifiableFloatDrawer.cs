using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

[CustomPropertyDrawer(typeof(ModifiableFloat))]
public class ModifiableFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty baseValueProperty = property.FindPropertyRelative("baseValue");
        EditorGUI.PropertyField(position, baseValueProperty, label);

    }
}  
