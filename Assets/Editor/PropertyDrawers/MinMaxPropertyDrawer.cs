using UnityEngine;
using UnityEditor;


[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class MinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxAttribute range = attribute as MinMaxAttribute;

        label = EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType == SerializedPropertyType.Float)
            EditorGUI.Slider(position, property, range.Min, range.Max, label);
        else if (property.propertyType == SerializedPropertyType.Integer)
            EditorGUI.IntSlider(position, property, (int)range.Min, (int)range.Max, label);
        else
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            float floatFieldWidth = EditorGUIUtility.fieldWidth;
            float sliderWidth = position.width - labelWidth - 2.0f * floatFieldWidth;
            float sliderPadding = 5.0f;

            Rect labelRect = new Rect(position) { width = labelWidth };

            Rect sliderRect = new Rect(position) {
                x = position.x + labelWidth + floatFieldWidth + sliderPadding + EditorGUI.indentLevel,
                width = sliderWidth - 2.0f * sliderPadding + EditorGUI.indentLevel
            };

            Rect minFloatFieldRect = new Rect(position) {
                x = position.x + labelWidth - EditorGUI.indentLevel,
                width = floatFieldWidth + EditorGUI.indentLevel
            };

            Rect maxFloatFieldRect = new Rect(position) {
                x = position.x + labelWidth + floatFieldWidth + sliderWidth - EditorGUI.indentLevel,
                width = floatFieldWidth + EditorGUI.indentLevel
            };

            SerializedProperty minValue = property.FindPropertyRelative("min");
            SerializedProperty maxValue = property.FindPropertyRelative("max");

            if (fieldInfo.FieldType == typeof(FloatRange))
            {
                float min = minValue.floatValue, max = maxValue.floatValue;
                EditorGUI.BeginChangeCheck();
                EditorGUI.LabelField(labelRect, label);
                EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, range.Min, range.Max);
                min = Mathf.Clamp(EditorGUI.FloatField(minFloatFieldRect, min), range.Min, Mathf.Min(range.Max, max));
                max = Mathf.Clamp(EditorGUI.FloatField(maxFloatFieldRect, max), Mathf.Max(range.Min, min), range.Max);
                if (EditorGUI.EndChangeCheck())
                {
                    minValue.floatValue = min;
                    maxValue.floatValue = max;
                }
            }
            else if (fieldInfo.FieldType == typeof(IntRange))
            {
                float min = minValue.intValue, max = maxValue.intValue;
                EditorGUI.BeginChangeCheck();
                EditorGUI.LabelField(labelRect, label);
                EditorGUI.MinMaxSlider(sliderRect, ref min, ref max, range.Min, range.Max);
                min = Mathf.Clamp(Mathf.Round(EditorGUI.FloatField(minFloatFieldRect, min)), range.Min, Mathf.Min(range.Max, max));
                max = Mathf.Clamp(Mathf.Round(EditorGUI.FloatField(maxFloatFieldRect, max)), Mathf.Max(range.Min, min), range.Max);
                if (EditorGUI.EndChangeCheck())
                {
                    minValue.intValue = (int)min;
                    maxValue.intValue = (int)max;
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Range with float or int, FloatRange or IntRange.");
            }
        }

        EditorGUI.EndProperty();
    }
}
