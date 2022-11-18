using UnityEngine;

[System.Serializable]
public class BiddingRound
{
    public int NumberOfItems { get => items.Length; }
    [SerializeField] 
    public Item[] items;
    [SerializeField] 
    public int[] chips;
    [SerializeField] 
    public PlayerManager[] players;

    public BiddingRound(Item[] items)
    {
        this.items = items;
        chips = new int[items.Length];
        players = new PlayerManager[items.Length];
    }
}
