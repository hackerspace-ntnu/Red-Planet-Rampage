using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModifyableFloat
{
    [SerializeField]
    private float baseValue = 1f;

    private float addition = 0f;

    private float multiplier = 1f;

    private float exponential = 1f;

    public  ModifyableFloat(float value)
    {
        baseValue = value;
    }

    public void addMultiplier(float value)
    {
        multiplier += value;
    }

    public void addExponential(float value)
    {
        exponential *= value;
    }

    public void addBaseValue(float value)
    {
        addition += value;
    }

    public float value()
    {
        return (baseValue + addition) * multiplier * exponential;
    }

    public static implicit operator float(ModifyableFloat a)
    {
        return a.value();
    }
}
    
