using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    public Color color;
    public string playerName;

    [SerializeField]
    public List<Item> items { get; private set; } = new List<Item>();

    public int chips { get; private set; } = 0;

    public delegate void ChipEvent(int amount);

    public ChipEvent onChipGain;
    public ChipEvent onChipSpent;
    public ChipEvent onChipChange;

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
        items.Add(item);
    }
}
