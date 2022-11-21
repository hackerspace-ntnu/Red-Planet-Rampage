using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IntRange)), CustomPropertyDrawer(typeof(FloatRange))]
public class NumericRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.LabelField(position, label);
        SerializedProperty minValue = property.FindPropertyRelative("min");
        SerializedProperty maxValue = property.FindPropertyRelative("max");

        
        Rect p = new Rect(position) { width = position.width - EditorGUIUtility.labelWidth, x = position.x + EditorGUIUtility.labelWidth };
        float s = p.width / 8;

        Rect pLabel = new Rect(p) { width = s };
        Rect pNumField = new Rect(pLabel) { x = pLabel.x + pLabel.width, width = s * 3 };

        EditorGUI.LabelField(pLabel, "Min");
        EditorGUI.PropertyField(pNumField, minValue, GUIContent.none, false);


        pLabel = new Rect(pLabel) { x = pLabel.x + pLabel.width * 4, width = s };
        pNumField = new Rect(pLabel) { x = pLabel.x + pLabel.width, width = s * 3 };

        EditorGUI.LabelField(pLabel, "Max");
        EditorGUI.PropertyField(pNumField, maxValue, GUIContent.none, false);
    }
}
