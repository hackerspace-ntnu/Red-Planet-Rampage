using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    public Color color;
    public string playerName;
    public int chips;

    public delegate void ChipEvent(int amount);

    public ChipEvent onChipGain;
    public ChipEvent onChipSpent;
    public ChipEvent onChipChange;

    public void UpdateChip(int amount)
    {
        if (amount == 0) return;

        onChipChange(chips+amount);
        
        if (amount < 0)
        {
            onChipSpent(amount);    
        }
        else
        {
            onChipGain(amount);
        }
    }
}
