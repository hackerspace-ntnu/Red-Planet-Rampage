using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class LabelAsAttribute : PropertyAttribute
{
    public readonly string Text;
    public LabelAsAttribute(string text) => Text = text;
}