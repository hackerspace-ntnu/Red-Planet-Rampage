using UnityEngine;

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
    public string displayDescription;
    [SerializeField]
    private AugmentType augmentType;

    public GameObject augment;
}
