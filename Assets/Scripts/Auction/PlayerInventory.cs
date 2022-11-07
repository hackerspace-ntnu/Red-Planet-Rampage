using CollectionExtensions;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] 
    private int tokens;
    [SerializeField] 
    private List<Item> items;

    public int Tokens => tokens;

    public void PerformTransaction(Item item, int cost)
    {
        items.Add(item);
        tokens -= cost;
    }
}
