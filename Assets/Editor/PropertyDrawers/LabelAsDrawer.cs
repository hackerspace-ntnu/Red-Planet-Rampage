using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LabelAsAttribute), true)]
public class LabelAsDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label )
    {
        LabelAsAttribute labelAsAttribute = attribute as LabelAsAttribute;
            
        if (!property.isArray)
        {
            EditorGUI.PropertyField(position, property, EditorGUIUtility.TrTextContent(labelAsAttribute.Text, label.tooltip, label.image), true);
        }
        else
        {
            Debug.LogWarningFormat(
                "{0}(\"{1}\") doesn't support arrays ",
                typeof(LabelAsAttribute).Name,
                labelAsAttribute.Text
            );
            EditorGUI.PropertyField(position, property, label);
        }
    }
}
