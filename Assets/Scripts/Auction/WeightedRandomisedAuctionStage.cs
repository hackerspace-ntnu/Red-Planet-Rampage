using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Auction/Stage/Weighted Randomised")]
public class WeightedRandomisedAuctionStage : RandomisedAuctionStage
{
    // TODO: the count of rates must mach the count of items - can be solved in onvalidate/in editor script
    protected List<(Item item, int weight)> weightedItems;
    protected int totalWeight;

    private Dictionary<Item, int> ownedByXPlayers;
    private Dictionary<string, int> notBoughtForXRounds;


#if UNITY_EDITOR
    private void OnValidate()
    {
        numItems = Mathf.Max(1, numItems); // at least one pls
    }
#endif

    public void PrepareWeights()
    {
        // Technically this shouldn't need to happen for each auction stage
        // but relatively inexpensive all things considered...
        ownedByXPlayers = RPRNetworkManager.PlayerInstanceByID.Values
            .Select(p => p.identity.AllItems.Select(item => (item, owners: 1)))
            .Aggregate((acc, val) =>
            {
                var accSet = acc.Select(pair => pair.item).ToHashSet();
                var valSet = val.Select(pair => pair.item).ToHashSet();
                return acc
                    // Increment count if present in more inventories
                    .Select(pair => (pair.item, owners: pair.owners + (valSet.Contains(pair.item) ? 1 : 0)))
                    // and include the new ones
                    .Concat(val.Where(pair => !accSet.Contains(pair.item)));
            })
            .ToDictionary(pair => pair.item, pair => pair.owners);

        notBoughtForXRounds = AuctionDriver.Rounds
            .Select(r => r.notBoughtItems.Select(item => (item, rounds: 1)))
            .Aggregate((acc, val) =>
            {
                // Basically same as above :/
                // TODO refactor if you really wanna bother
                var accSet = acc.Select(pair => pair.item).ToHashSet();
                var valSet = val.Select(pair => pair.item).ToHashSet();
                return acc
                    // Increment count if present in more inventories
                    .Select(pair => (pair.item, owners: pair.rounds + (valSet.Contains(pair.item) ? 1 : 0)))
                    // and include the new ones
                    .Concat(val.Where(pair => !accSet.Contains(pair.item)));
            })
            .ToDictionary(pair => pair.item, pair => pair.rounds);

        weightedItems = items
            .Select(i => (item: i, weight: DetermineWeight(i)))
            .ToList();

        totalWeight = weightedItems.Sum(pair => pair.weight);
    }

    private int DetermineWeight(Item item)
    {
        var score = 1000;

        // Being owned by a player is the biggest sign we should ignore it
        if (ownedByXPlayers.TryGetValue(item, out var owners))
        {
            score -= owners switch
            {
                0 => 0,
                1 => 700,
                2 => 800,
                3 => 950,
                _ => 999,
            };
        }

        if (notBoughtForXRounds.TryGetValue(item.id, out var roundsNotBought))
        {
            score -= roundsNotBought switch
            {
                0 => 0,
                1 => 200,
                2 => 900,
                3 => 950,
                _ => 999,
            };
        }

        // TODO more criteria? winning weapon etc?

        return Mathf.Max(score, 1);
    }

    // prolly really stupick but this is what my exhausted brain came up with :))))))
    private Item RandomElement(bool withReplacement = false)
    {
        // This should be compared to running weight sum
        // Larger weight = higher probability of being picked
        int random = Random.Range(0, totalWeight);

        Item item = null;
        int weight = 0, index = -1, runningWeight = 0;
        for (int i = 0; i < weightedItems.Count; i++)
        {
            (item, weight) = weightedItems[i];
            // Random within span?
            if (random < runningWeight + weight && random >= runningWeight)
            {
                // This is it.
                index = i;
                break;
            }
            runningWeight += weight;
        }

        if (index < 0)
        {
            Debug.LogWarning($"Found no item at {random} running weight of {totalWeight}");
            // Default to *something* if we don't find an item
            index = Random.Range(0, weightedItems.Count);
            (item, weight) = weightedItems[index];
        }

        if (!withReplacement)
        {
            weightedItems.RemoveAt(index);
            totalWeight -= weight;
        }

        return item;
    }

    private Item[] RandomSelection(bool withReplacement = false)
    {
        Item[] selection = new Item[numItems];
        for (int i = 0; i < numItems; i++)
        {
            selection[i] = RandomElement(withReplacement);
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

        Item[] selection = RandomSelection(withReplacement);
        round = new BiddingRound(selection);
        return true;
    }
}
