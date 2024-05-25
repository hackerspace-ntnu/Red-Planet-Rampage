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
    [Tooltip("Unique identifier for this augment. NEVER change this after it has been set the first time, as it will mess up statistics!")]
    public string id;
    [Tooltip("Shown in auctions.")]
    public string displayName;
    [Tooltip("Hidden word used to create weapon name.")]
    public string secretName;

    [TextArea]
    [Tooltip("Shown in auctions. Briefly explains what the augment does.")]
    public string displayDescription;
    [TextArea]
    [Tooltip("Shown in augment gallery. Explains what the augment does in greater detail, perhaps with strategy tips.")]
    public string extendedDescription;
    [TextArea]
    [Tooltip("Shown in augment gallery. The lore behind the item.")]
    public string loreDescription;

    public AugmentType augmentType;

    public GameObject augment;

    public override string ToString()
    {
        return displayName;
    }
}
