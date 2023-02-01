using UnityEngine;


[System.Serializable]
public struct FloatRange
{
    [SerializeField]
    private float min, max;
    public float Min => min;
    public float Max => max;
}

[System.Serializable]
public struct IntRange
{
    [SerializeField]
    private int min, max;
    public int Min => min;
    public int Max => max;
}
