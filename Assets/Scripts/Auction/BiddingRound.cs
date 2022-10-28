using UnityEngine;

[System.Serializable]
public class BiddingRound
{
    public int NumberOfItems { get => items.Length; }
    [SerializeField] public Item[] items;
    [SerializeField] public int[] tokens;
    [SerializeField] public PlayerInventory[] players;

    public BiddingRound(Item[] items)
    {
        this.items = items;
        tokens = new int[items.Length];
        players = new PlayerInventory[items.Length];
    }
}