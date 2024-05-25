using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;

public class PlayerIdentity : MonoBehaviour
{
    [Header("Cosmetics")]
    public Color color;
    public string playerName;
    [SerializeField]
    protected bool isAI = false;
    public bool IsAI => isAI;

    [Header("Augments")]
    [SerializeField]
    protected Item body;
    public Item Body => body;

    [SerializeField]
    protected Item barrel;
    public Item Barrel => barrel;

    [SerializeField]
    protected Item extension;
    public Item Extension => extension;

    public List<Item> Bodies { get; private set; } = new List<Item>();
    public List<Item> Barrels { get; private set; } = new List<Item>();
    public List<Item> Extensions { get; private set; } = new List<Item>();

    public int chips { get; private set; } = 0;

    public uint id;

    public delegate void ChipEvent(int amount);
    public delegate void ItemEvent(Item item);

    public ChipEvent onChipGain;
    public ChipEvent onChipSpent;
    public ChipEvent onChipChange;
    public ItemEvent onInventoryChange;

    private void Start()
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

    public void ResetItems()
    {
        Bodies = new List<Item>();
        Barrels = new List<Item>();
        Extensions = new List<Item>();
        body = StaticInfo.Singleton.StartingBody;
        barrel = StaticInfo.Singleton.StartingBarrel;
        extension = StaticInfo.Singleton.StartingExtension;
        chips = 0;
    }


    public void SetItems(IEnumerable<string> bodies, IEnumerable<string> barrels, IEnumerable<string> extensions)
    {
        // TODO handle errors better
        Bodies = bodies.Select(id => StaticInfo.Singleton.ItemsById[id]).ToList();
        Barrels = barrels.Select(id => StaticInfo.Singleton.ItemsById[id]).ToList();
        Extensions = extensions.Select(id => StaticInfo.Singleton.ItemsById[id]).ToList();
    }


    public void SetLoadout(string body, string barrel, string extension)
    {
        this.body = StaticInfo.Singleton.ItemsById[body];
        this.barrel = StaticInfo.Singleton.ItemsById[barrel];
        this.extension = extension == null ? null : StaticInfo.Singleton.ItemsById[extension];
    }

    public void SetLoadout(Item body, Item barrel, Item extension)
    {
        this.body = body;
        this.barrel = barrel;
        this.extension = extension;
    }

    public void UpdateFromDetails(PlayerDetails playerDetails, string name)
    {
        id = playerDetails.id;

        playerName = name;
        color = playerDetails.color;

        chips = playerDetails.chips;

        SetItems(playerDetails.bodies, playerDetails.barrels, playerDetails.extensions);
        SetLoadout(playerDetails.body, playerDetails.barrel, playerDetails.extension);
    }

    public override string ToString() => playerName;

    public string ToColorString() => $"<color=#{color.ToHexString()}>{playerName}</color>";
}
