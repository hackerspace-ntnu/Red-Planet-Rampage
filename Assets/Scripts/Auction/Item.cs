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
    [SerializeField]
    private string displayName;
    [SerializeField]
    private AugmentType augmentType;

    public GameObject augment;
}
