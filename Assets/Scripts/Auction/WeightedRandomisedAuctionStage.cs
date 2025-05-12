using System;
using System.Collections.Generic;
using System.Linq;
using CollectionExtensions;
using UnityEngine;

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
        Item[] pool = AvailableSelection();

        for (int i = 0; i < numItems; i++)
        {
            selection[i] = pool.RandomElement();
        }
        return selection;
    }
    private Item[] RandomSelectionWithoutReplacement()
    {
        Item[] pool = AvailableSelection();
        int[] idx = pool.RandomIndicesOf();
        Item[] selection = new Item[numItems];

        if (pool.Length < numItems)
            return RandomSelectionWithReplacement();

        for (int i = 0; i < numItems; i++)
        {
            selection[i] = pool[idx[i]];
        }
        return selection;
    }
    private Item[] AvailableSelection()
    {
        List<Item> newSelection = items.ToList();
        foreach (Item item in ownedItems)
        {
            newSelection.Remove(item);
        }
        return newSelection.ToArray();
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
