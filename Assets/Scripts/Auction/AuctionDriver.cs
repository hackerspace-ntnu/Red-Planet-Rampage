using CollectionExtensions;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AuctionDriver : MonoBehaviour
{
    [SerializeField] 
    private int numberOfItems;
    [SerializeField] 
    private AuctionSequence sequence;
    private IEnumerator<BiddingRound> enumerator;

#if UNITY_EDITOR
    // USING THIS PATTERN TO SHOW PROPERTIES IN EDITOR, UPON BUILD COMPILATION THIS OVERHEAD IS REMOVED
    // DO NOT USE _EDITORONLY_ prepended fields outside of an UNITY_EDITOR block!
    [SerializeField, Uneditable] 
    private BiddingRound _EDITORONLY_ActiveBiddingRound;
    private BiddingRound ActiveBiddingRound
    {
        get
        {
            _EDITORONLY_ActiveBiddingRound = enumerator.Current;
            return _EDITORONLY_ActiveBiddingRound;
        }
    }
#else
    private BiddingRound ActiveBiddingRound => enumerator.Current;
#endif

    [SerializeField] 
    private PlayerInventory[] playersInAuction;
    private Dictionary<PlayerInventory, int> tokensInPlay = new Dictionary<PlayerInventory, int>();


    [SerializeField] 
    private CoroutineTimer auctionTimer;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        numberOfItems = Mathf.Max(1, numberOfItems);
    }
#endif

    // Stages
    // BiddingStage contains a list of items (scriptableobjects)

    private void Awake()
    {
        auctionTimer.OnWaitStart += GoToNextAuctionRound;
        auctionTimer.OnWaitCompleted += FinishAuctionRound;
    }

    private void Start()
    {
        playersInAuction = FindObjectsOfType<PlayerInventory>();
    }

#if UNITY_EDITOR
    [ContextMenu("Test Start Auction")]
#endif
    public void StartAuction()
    {
        foreach (var player in playersInAuction)
            tokensInPlay[player] = 0;

        enumerator = sequence.GetEnumerator();
        //auctionTimer.Start(this);
    }

#if UNITY_EDITOR
    [ContextMenu("Test Stop Auction")]
#endif
    public void StopAuction()
    {
        //auctionTimer.Stop(this);
    }


#if UNITY_EDITOR
    [ContextMenu("Place Random Bids")]
    private void PlaceRandomBids()
    {
        foreach (var player in playersInAuction)
        {
            int itemSlot = Random.Range(0, ActiveBiddingRound.NumberOfItems);
            PlayerInventory originalHolder = ActiveBiddingRound.players[itemSlot]; //(ActiveBiddingRound.playerIDs[itemSlot] != -1) ? playersInAuction[ActiveBiddingRound.playerIDs[itemSlot]] : null;
            if (TryPlaceBid(player, itemSlot))
            {
                if (originalHolder != null)
                    RandomBiddingWar(originalHolder, player, itemSlot);
            }
        }
    }

    private PlayerInventory RandomBiddingWar(PlayerInventory originalHolder, PlayerInventory newHolder, int slot, float continue_chance = 0.9f)
    {
        bool continueMatching = Random.value < continue_chance;
        // Return winner of bidding war
        if (continueMatching && TryPlaceBid(originalHolder, slot))
        {
            return RandomBiddingWar(newHolder, originalHolder, slot, continue_chance - 0.1f);
        }
        return newHolder;
    }
#endif

    private bool TryPlaceBid(PlayerInventory player, int slot)
    {
        int cost = ActiveBiddingRound.tokens[slot] + 1;
        if (tokensInPlay[player] + cost >= player.Tokens)
        {
            Debug.Log($"Player {System.Array.FindIndex(playersInAuction, p => p == player):D2} tried to place a bid " +
                $"on {ActiveBiddingRound.items[slot]} without having the tokens for it!");
            // The player has no more tokens to spend!
            return false;
        }

        // Has someone bid on this item before?
        if (ActiveBiddingRound.players[slot] != null)
        {
            PlayerInventory outbid = ActiveBiddingRound.players[slot];
            tokensInPlay[outbid] -= ActiveBiddingRound.tokens[slot];

            Debug.Log($"Player {System.Array.FindIndex(playersInAuction, p => p == player):D2} outbid Player {System.Array.FindIndex(playersInAuction, p => p == outbid):D2}" +
                $"for {ActiveBiddingRound.items[slot]} at a cost of {cost} tokens!");
        }

        // Actually Place the bid
        tokensInPlay[player] += cost;
        ActiveBiddingRound.tokens[slot] = cost;
        ActiveBiddingRound.players[slot] = player;
        return true;
    }

    [ContextMenu("GoToNextAuctionRound")]
    private void GoToNextAuctionRound()
    {
        if (!enumerator.MoveNext())
        {
            StopAuction();
            return;
        }
        foreach (var item in ActiveBiddingRound.items)
        {
            Debug.Log($"Item up for grabs: {item}");
        }
    }
    [ContextMenu("FinishAuctionRound")]
    private void FinishAuctionRound()
    {
        Debug.Log($"Round {ActiveBiddingRound} Finished!");
        for (int i = 0; i < ActiveBiddingRound.NumberOfItems; i++)
        {
            if (ActiveBiddingRound.players[i] != null)
            {
                ActiveBiddingRound.players[i].PerformTransaction(ActiveBiddingRound.items[i], ActiveBiddingRound.tokens[i]);
            }
        }
    }
}
