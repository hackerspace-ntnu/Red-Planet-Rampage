using CollectionExtensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO
// Topic for discussion:
// Using RequireComponent + GetComponent in Awake,
// vs manually assigning component references
//
[RequireComponent(typeof(Timer))]
public class AuctionDriver : MonoBehaviour
{
    [SerializeField] 
    private int numberOfItems;
    [SerializeField]
    private float baseWaitTime;
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

    private HashSet<PlayerInventory> playersInAuction;
    private HashSet<PlayerInventory> playersInAuctionRound;
    private Dictionary<PlayerInventory, int> tokensInPlay = new Dictionary<PlayerInventory, int>();


    [SerializeField] 
    private Timer auctionTimer;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        numberOfItems = Mathf.Max(1, numberOfItems);
    }
#endif

    // Stages
    // BiddingStage contains a list of items (scriptableobjects)

    private void OnEnable()
    {
        auctionTimer.OnTimerRunStarted += GoToNextAuctionRound;
        auctionTimer.OnTimerRunCompleted += FinishAuctionRound;
    }

    private void OnDisable()
    {
        auctionTimer.OnTimerRunStarted -= GoToNextAuctionRound;
        auctionTimer.OnTimerRunCompleted -= FinishAuctionRound;
    }

    private void Start()
    {
        playersInAuction = new HashSet<PlayerInventory>(FindObjectsOfType<PlayerInventory>());
        playersInAuctionRound = new HashSet<PlayerInventory>(playersInAuction);
    }

    private bool TryPlaceBid(PlayerInventory player, int slot)
    {
        if (!playersInAuctionRound.Contains(player))
        {
            Debug.Log($"Player '{player.name}' is not in current round!");
            return false;
        }

        int cost = ActiveBiddingRound.tokens[slot] + 1;
        if (tokensInPlay[player] + cost >= player.Tokens)
        {
            Debug.Log($"Player '{player.name}' tried to place a bid " +
                $"on {ActiveBiddingRound.items[slot]} without having the tokens for it!");
            // The player has no more tokens to spend!
            return false;
        }

        // Has someone bid on this item before?
        if (ActiveBiddingRound.players[slot] != null)
        {
            PlayerInventory outbid = ActiveBiddingRound.players[slot];
            tokensInPlay[outbid] -= ActiveBiddingRound.tokens[slot];

            Debug.Log($"Player '{player.name}' outbid Player '{outbid.name}'" +
                $"for {ActiveBiddingRound.items[slot]} at a cost of {cost} tokens!");
        }

        // Actually Place the bid
        tokensInPlay[player] += cost;
        ActiveBiddingRound.tokens[slot] = cost;
        ActiveBiddingRound.players[slot] = player;
        return true;
    }

    private void YieldFromAuction(PlayerInventory player)
    {
        playersInAuction.Remove(player);
        if (playersInAuction.Count == 0)
        {
            auctionTimer.StopTimer();
        }
    }
    private void YieldFromRound(PlayerInventory player)
    {
        playersInAuctionRound.Remove(player);
        if (playersInAuctionRound.Count == 0)
        {
            auctionTimer.EndTimerRun();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Test Start Auction")]
#endif
    public void StartAuction()
    {
        foreach (var player in playersInAuction)
            tokensInPlay[player] = 0;

        enumerator = sequence.GetEnumerator();
        auctionTimer.StartTimer(10f, repeating: true);
    }

#if UNITY_EDITOR
    [ContextMenu("Test Stop Auction")]
#endif
    public void StopAuction()
    {
        auctionTimer.StopTimer();
    }

#if UNITY_EDITOR
    [ContextMenu("GoToNextAuctionRound")]
#endif
    private void GoToNextAuctionRound()
    {
        if (!enumerator.MoveNext())
        {
            Debug.Log($"Stopping auction!");
            StopAuction();
            return;
        }
        // Place all players who are still in the auction into the new round
        playersInAuctionRound.UnionWith(playersInAuction);
    }

#if UNITY_EDITOR
    [ContextMenu("FinishAuctionRound")]
#endif
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

#if UNITY_EDITOR
    [ContextMenu("Yield All From Auction")]
    private void YieldAllPlayersFromAuction()
    {
        PlayerInventory[] players = playersInAuction.ToArray();
        for (int i = 0; i < players.Length; i++)
        {
            YieldFromAuction(players[i]);
        }
    }
    [ContextMenu("Yield All From Round")]
    private void YieldAllPlayersFromRound()
    {
        PlayerInventory[] players = playersInAuctionRound.ToArray();
        for (int i = 0; i < players.Length; i++)
        {
            YieldFromRound(players[i]);
        }
    }

    [ContextMenu("Yield Random Player From Auction")]
    private void YieldRandomPlayerFromAuction()
    {
        PlayerInventory yielding = playersInAuction.RandomElement();
        Debug.Log($"Player: {yielding} yielded from auction round");
        YieldFromAuction(yielding);
    }
    [ContextMenu("Yield Random Player From Round")]
    private void YieldRandomPlayerFromRound()
    {
        PlayerInventory yielding = playersInAuctionRound.RandomElement();
        YieldFromRound(yielding);
        Debug.Log($"Player: {yielding} yielded from auction round");
    }

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
}
