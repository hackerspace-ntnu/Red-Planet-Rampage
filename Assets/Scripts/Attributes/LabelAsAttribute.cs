using System;
using UnityEngine;

/// <summary>
/// Change how fields marked with this attribute are shown in the unity editor.
/// 
/// Example:
/// <code>
/// [LabelAs("Nice Name")]
/// public string notNiceNameInInspector;
/// </code>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class LabelAsAttribute : PropertyAttribute
{
    public readonly string Text;
    public LabelAsAttribute(string text) => Text = text;
}
