using System;
using UnityEngine;

/// <summary>
/// Constraint a value to be in between Min and Max in the unity editor. (like the Range attribute, but more covering!)
/// 
/// Example:
/// <code>
/// [MinMax(0, 5)]
/// public int someValueThatShouldBeBetweenZeroAndFive;
/// </code>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class MinMaxAttribute : PropertyAttribute
{
    public readonly float Min, Max;
    public MinMaxAttribute(float min, float max)
    {
        Min = min;
        Max = max;
    }
    public MinMaxAttribute(int min, int max)
    {
        Min = min;
        Max = max;
    }
}
