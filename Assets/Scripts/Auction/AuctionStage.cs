using UnityEngine;
using CollectionExtensions;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Auction/Stage/Scripted")]
public class AuctionStage : ScriptableObject
{
    [SerializeField] 
    protected float stageDuration = 15;
    [SerializeField] 
    protected Item[] items;

    /// <summary>
    /// This method should ONLY be called by staticInfo.
    /// </summary>
    /// <param name="items">Which items exists in this AuctionStage</param>
    public void SetItems(Item[] items)
    {
        this.items = items;
    }

    public virtual bool Promote(out BiddingRound round)
    {
        if (items.Length == 0)
        {
            round = null;
            return false;
        }
        round = new BiddingRound(items);
        return true;
    }
}
