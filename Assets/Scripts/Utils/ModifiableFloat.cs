using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModifiableFloat
{
    [SerializeField]
    private float baseValue = 1f;

    private float addition = 0f;

    private float multiplier = 1f;

    private float exponential = 1f;

    public ModifiableFloat(float value)
    {
        baseValue = value;
    }

    public void AddMultiplier(float value)
    {
        multiplier += value;
    }

    public void AddExponential(float value)
    {
        exponential *= value;
    }

    public void AddBaseValue(float value)
    {
        addition += value;
    }

    public float Value()
    {
        return (baseValue + addition) * multiplier * exponential;
    }

    public static implicit operator float(ModifiableFloat a)
    {
        return a.Value();
    }

    public override string ToString()
    {
        return $"({baseValue} + {addition}) * {multiplier} * {exponential}";
    }
}

