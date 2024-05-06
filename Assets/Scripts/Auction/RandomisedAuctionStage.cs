using CollectionExtensions;
using UnityEngine;

[System.Serializable]
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
            selection[i] = items.RandomElement(random);
        }
        return selection;
    }
    private Item[] RandomSelectionWithoutReplacement()
    {
        int[] idx = items.RandomIndicesOf(random);
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
