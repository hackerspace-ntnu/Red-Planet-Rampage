﻿using UnityEngine;

public enum AugmentType
{
    Body,
    Barrel,
    Extension
}

[CreateAssetMenu(menuName = "Auction/Item")]
public class Item : ScriptableObject
{
    public string displayName;
    public string secretName;
    public string displayDescription;
    public AugmentType augmentType;

    public GameObject augment;

    public override string ToString()
    {
        return displayName;
    }
}
