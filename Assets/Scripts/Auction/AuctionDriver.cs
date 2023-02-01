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
[RequireComponent(typeof(PlayerFactory))]
public class AuctionDriver : MonoBehaviour
{
    [SerializeField] 
    private int numberOfItems;
    [SerializeField]
    private float baseWaitTime;
    [SerializeField] 
    private AuctionSequence sequence;
    private IEnumerator<BiddingRound> enumerator;

    [SerializeField]
    private BiddingPlatform[] biddingPlatforms;
    [SerializeField]
    private RandomisedAuctionStage[] availableAuctionStages;


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

    private HashSet<PlayerManager> playersInAuction;
    private HashSet<PlayerManager> playersInAuctionRound;
    private Dictionary<PlayerManager, int> chipsInPlay = new Dictionary<PlayerManager, int>();


    [SerializeField] 
    private Timer auctionTimer;
    [SerializeField]
    private PlayerFactory playerFactory;

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
        playerFactory = GetComponent<PlayerFactory>();
        playerFactory.InstantiatePlayersBidding();
        playersInAuction = new HashSet<PlayerManager>(FindObjectsOfType<PlayerManager>());
        playersInAuctionRound = new HashSet<PlayerManager>(playersInAuction);

        // TODO: Make AuctionDriver instantiate bidding platforms instead of finding them?
        PopulatePlatforms();

        // TODO:
        // - Rewrite AuctionDriver to be a manager of bidding rounds on BiddingPlatforms 
        // - We /can/ use biddingRound to store relevant values (If we want to show a summary for bidding at the end or something), but it seems a bit out of scope atm
        // - AuctionDriver should be the class responsible for handling all AuctionStage-related tasks
        // - AuctionDriver should be responsible for detecting when all auctions are done and then trigger needed responses (preferably by calling MatchManager).
        
    }

    private void PopulatePlatforms()
    {
        if (!(availableAuctionStages.Length == biddingPlatforms.Length))
        {
            Debug.Log("Not enough avialable auctionStages or biddingPlatforms!");
        }

        for (int i = 0; i < biddingPlatforms.Length; i++)
        {
            availableAuctionStages[i].Promote(out BiddingRound biddingRound);
            biddingPlatforms[i].ActiveBiddingRound = biddingRound;
            biddingPlatforms[i].SetItem(biddingRound.items[0]);
        }
    }

    private bool TryPlaceBid(PlayerManager player, int slot)
    {
        if (!playersInAuctionRound.Contains(player))
        {
            Debug.Log($"Player '{player.name}' is not in current round!");
            return false;
        }

        int cost = ActiveBiddingRound.chips[slot] + 1;
        if (chipsInPlay[player] + cost >= player.identity.chips)
        {
            Debug.Log($"Player '{player.name}' tried to place a bid " +
                $"on {ActiveBiddingRound.items[slot]} without having the tokens for it!");
            // The player has no more tokens to spend!
            return false;
        }

        // Has someone bid on this item before?
        if (ActiveBiddingRound.players[slot] != null)
        {
            PlayerManager outbid = ActiveBiddingRound.players[slot];
            chipsInPlay[outbid] -= ActiveBiddingRound.chips[slot];

            Debug.Log($"Player '{player.name}' outbid Player '{outbid.name}'" +
                $"for {ActiveBiddingRound.items[slot]} at a cost of {cost} chips!");
        }

        // Actually Place the bid
        chipsInPlay[player] += cost;
        ActiveBiddingRound.chips[slot] = cost;
        ActiveBiddingRound.players[slot] = player;
        return true;
    }

    private void YieldFromAuction(PlayerManager player)
    {
        playersInAuction.Remove(player);
        if (playersInAuction.Count == 0)
        {
            auctionTimer.StopTimer();
        }
    }
    private void YieldFromRound(PlayerManager player)
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
            chipsInPlay[player] = 0;

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
                ActiveBiddingRound.players[i].identity.PerformTransaction(ActiveBiddingRound.items[i]);
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Yield All From Auction")]
    private void YieldAllPlayersFromAuction()
    {
        PlayerManager[] players = playersInAuction.ToArray();
        for (int i = 0; i < players.Length; i++)
        {
            YieldFromAuction(players[i]);
        }
    }
    [ContextMenu("Yield All From Round")]
    private void YieldAllPlayersFromRound()
    {
        PlayerManager[] players = playersInAuctionRound.ToArray();
        for (int i = 0; i < players.Length; i++)
        {
            YieldFromRound(players[i]);
        }
    }

    [ContextMenu("Yield Random Player From Auction")]
    private void YieldRandomPlayerFromAuction()
    {
        PlayerManager yielding = playersInAuction.RandomElement();
        Debug.Log($"Player: {yielding} yielded from auction round");
        YieldFromAuction(yielding);
    }
    [ContextMenu("Yield Random Player From Round")]
    private void YieldRandomPlayerFromRound()
    {
        PlayerManager yielding = playersInAuctionRound.RandomElement();
        YieldFromRound(yielding);
        Debug.Log($"Player: {yielding} yielded from auction round");
    }

    [ContextMenu("Place Random Bids")]
    private void PlaceRandomBids()
    {
        foreach (var player in playersInAuction)
        {
            int itemSlot = Random.Range(0, ActiveBiddingRound.NumberOfItems);
            PlayerManager originalHolder = ActiveBiddingRound.players[itemSlot]; //(ActiveBiddingRound.playerIDs[itemSlot] != -1) ? playersInAuction[ActiveBiddingRound.playerIDs[itemSlot]] : null;
            if (TryPlaceBid(player, itemSlot))
            {
                if (originalHolder != null)
                    RandomBiddingWar(originalHolder, player, itemSlot);
            }
        }
    }

    private PlayerManager RandomBiddingWar(PlayerManager originalHolder, PlayerManager newHolder, int slot, float continue_chance = 0.9f)
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
