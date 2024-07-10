using UnityEngine;


[System.Serializable]
public struct OverrideName
{
    public Item Body;
    public Item Barrel;
    public Item Extension;
    public string Name;
    public SecretCombinationType Achievement;
}

/// <summary>
/// Holds a list that can be set in editor
/// This information is accessiable in the StaticInfo prefab.
/// New instances of override names should be set in the same scriptableObject that is used in StaticInfo.
/// </summary>
public class SecretNamesStaticStorage : ScriptableObject
{
    public OverrideName[] Overrides;
}
