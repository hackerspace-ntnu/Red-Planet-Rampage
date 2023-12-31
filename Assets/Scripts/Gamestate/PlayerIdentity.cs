using System.Collections.Generic;
using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    [Header("Cosmetics")]
    public Color color;
    public string playerName;

    [Header("Augments")]
    [SerializeField]
    private Item body;
    public Item Body => body;

    [SerializeField]
    private Item barrel;
    public Item Barrel => barrel;

    [SerializeField]
    private Item extension;
    public Item Extension => extension;

    public List<Item> Bodies { get; private set; } = new List<Item>();
    public List<Item> Barrels { get; private set; } = new List<Item>();
    public List<Item> Extensions { get; private set; } = new List<Item>();

    public int chips { get; private set; } = 0;

    public int bounty = 5;

    public delegate void ChipEvent(int amount);
    public delegate void ItemEvent(Item item);

    public ChipEvent onChipGain;
    public ChipEvent onChipSpent;
    public ChipEvent onChipChange;
    public ItemEvent onInventoryChange;

    void Start()
    {
        if (Bodies.Count == 0)
        {
            Bodies.Add(body);
        }
        if (Barrels.Count == 0)
        {
            Barrels.Add(barrel);
        }
        if (Extensions.Count == 0)
        {
            Extensions.Add(extension);
        }

        bounty += 5;
    }

    public void UpdateChip(int amount)
    {
        if (amount == 0) return;
        chips += amount;

        onChipChange?.Invoke(chips);

        if (amount < 0)
        {
            onChipSpent?.Invoke(amount);
        }
        else
        {
            onChipGain?.Invoke(amount);
        }
    }

    public void PerformTransaction(Item item)
    {
        switch (item.augmentType)
        {
            case AugmentType.Body:
                Bodies.Add(item);
                break;
            case AugmentType.Barrel:
                Barrels.Add(item);
                break;
            case AugmentType.Extension:
                Extensions.Add(item);
                break;
            default:
                Debug.Log($"No appropritate augmentType ({item.augmentType}) found in item.");
                break;
        }
        onInventoryChange?.Invoke(item);
    }

    public void resetItems()
    {
        Bodies = new List<Item>();
        Barrels = new List<Item>();
        Extensions = new List<Item>();
        body = StaticInfo.Singleton.StartingBody;
        barrel = StaticInfo.Singleton.StartingBarrel;
        extension = StaticInfo.Singleton.StartingExtension;
        chips = 0;
    }

    public void SetLoadout(Item body, Item barrel, Item extension)
    {
        this.body = body;
        this.barrel = barrel;
        this.extension = extension;
    }

    public override string ToString()
    {
        return name;
    }
}
