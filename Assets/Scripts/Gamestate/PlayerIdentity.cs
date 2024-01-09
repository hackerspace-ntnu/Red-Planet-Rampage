using System.Collections.Generic;
using CollectionExtensions;
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
    public int score { get; private set; } = 0;

    public delegate void ChipEvent(int amount);
    public delegate void ItemEvent(Item item);

    public ChipEvent onChipGain;
    public ChipEvent onChipSpent;
    public ChipEvent onChipChange;
    public ChipEvent onScoreChange;
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
    }

    public void AssignReward(Reward reward)
    {
        switch (reward.Type)
        {
            case RewardType.Chips:
                UpdateChip(reward.Amount);
                break;
            case RewardType.Score:
                score += reward.Amount;
                break;
        }
    }

    public void UpdateScore(int amount)
    {
        if (amount == 0) return;
        score += amount;

        onScoreChange?.Invoke(score);
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
                if (!Bodies.Contains(item))
                    Bodies.Add(item);
                body = item;
                break;
            case AugmentType.Barrel:
                if (!Barrels.Contains(item))
                    Barrels.Add(item);
                barrel = item;
                break;
            case AugmentType.Extension:
                if (!Extensions.Contains(item))
                    Extensions.Add(item);
                extension = item;
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

        switch (MatchRules.Singleton.Rules.StartingWeapon.type)
        {
            case StartingWeaponType.Standard:
                body = StaticInfo.Singleton.StartingBody;
                barrel = StaticInfo.Singleton.StartingBarrel;
                extension = StaticInfo.Singleton.StartingExtension;
                break;

            // TODO implement shared random somehow?
            case StartingWeaponType.IndividualRandom:
                body = StaticInfo.Singleton.Bodies.RandomElement();
                barrel = StaticInfo.Singleton.Barrels.RandomElement();
                extension = StaticInfo.Singleton.Extensions.RandomElement();
                break;

            case StartingWeaponType.SharedRandom:
            case StartingWeaponType.Specific:
                body = MatchRules.Singleton.Rules.StartingWeapon.body;
                barrel = MatchRules.Singleton.Rules.StartingWeapon.barrel;
                extension = MatchRules.Singleton.Rules.StartingWeapon.extension;
                break;
        }

        Bodies.Add(body);
        Barrels.Add(barrel);
        Extensions.Add(extension);
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
