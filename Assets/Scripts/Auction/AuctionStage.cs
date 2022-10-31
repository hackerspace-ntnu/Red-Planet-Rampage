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

[CreateAssetMenu(menuName = "Auction/Stage/Randomised")]
public class RandomisedAuctionStage : AuctionStage
{
    [SerializeField] 
    protected int numItems = 5;

    [SerializeField] 
    protected bool withReplacement = true;

#if UNITY_EDITOR
    private void OnValidate()
    {
        numItems = Mathf.Max(1, numItems); // at least one pls
    }
#endif

    private Item[] RandomSelectionWithReplacement()
    {
        Item[] selection = new Item[numItems];
        for (int i = 0; i < numItems; i++)
        {
            selection[i] = items.RandomElement();
        }
        return selection;
    }
    private Item[] RandomSelectionWithoutReplacement()
    {
        int[] idx = items.RandomIndicesOf();
        Item[] selection = new Item[numItems];
        for (int i = 0; i < numItems; i++)
        {
            selection[i] = items[idx[i]];
        }
        return selection;
    }

    public override bool Promote(out BiddingRound round)
    {
        if (numItems == 0 || items.Length == 0)
        {
            round = null;
            return false;
        }
        Item[] selection = withReplacement ? RandomSelectionWithReplacement() : RandomSelectionWithoutReplacement();
        round = new BiddingRound(selection);
        return true;
    }
}

// TODO: Implement weighted selection with and without replacement
[CreateAssetMenu(menuName = "Auction/Stage/Weighted Randomised")]
public class WeightedRandomisedAuctionStage : RandomisedAuctionStage
{
    // TODO: the count of rates must mach the count of items - can be solved in onvalidate/in editor script
    [SerializeField] 
    protected float[] rates;
    

#if UNITY_EDITOR
    private void OnValidate()
    {
        numItems = Mathf.Max(1, numItems); // at least one pls
        if (items.Length != rates.Length)
        {
            rates.Resize(items.Length);
        }
    }
#endif

    private Item[] RandomSelectionWithReplacement()
    {
        Item[] selection = new Item[numItems];
        for (int i = 0; i < numItems; i++)
        {
            selection[i] = items.RandomElement();
        }
        return selection;
    }
    private Item[] RandomSelectionWithoutReplacement()
    {
        int[] idx = items.RandomIndicesOf();
        Item[] selection = new Item[numItems];
        for (int i = 0; i < numItems; i++)
        {
            selection[i] = items[idx[i]];
        }
        return selection;
    }

    public override bool Promote(out BiddingRound round)
    {
        if (numItems == 0 || items.Length == 0)
        {
            round = null;
            return false;
        }

        float sum = 0f;
        for (int i = 0; i < rates.Length; i++)
            sum += rates[i];


        Item[] selection = withReplacement ? RandomSelectionWithReplacement() : RandomSelectionWithoutReplacement();
        round = new BiddingRound(selection);
        return true;
    }
}
