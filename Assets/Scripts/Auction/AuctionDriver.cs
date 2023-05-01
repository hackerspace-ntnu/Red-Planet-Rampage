using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    private float biddingBeginDelay = 5f;
    [SerializeField] 
    private AuctionSequence sequence;
    private IEnumerator<BiddingRound> enumerator;

    [SerializeField]
    private BiddingPlatform[] biddingPlatforms;
    private RandomisedAuctionStage[] availableAuctionStages;

    private BiddingPlatform lastExtendedAuction;

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
    
    [SerializeField]
    private RectTransform[] gunConstructionPanels;
    private float gunConstructionScale = 8f;

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
//#if UNITY_EDITOR
//        biddingBeginDelay = 0f;
//#endif
        availableAuctionStages = new RandomisedAuctionStage[] { StaticInfo.Singleton.BodyAuction, StaticInfo.Singleton.BarrelAuction, StaticInfo.Singleton.ExtensionAuction };
        playerFactory = GetComponent<PlayerFactory>();
        playerFactory.InstantiatePlayersBidding();
        playersInAuction = new HashSet<PlayerManager>(FindObjectsOfType<PlayerManager>());
        playersInAuctionRound = new HashSet<PlayerManager>(playersInAuction);

        AnimateAuctionStart();
        // TODO: Make AuctionDriver instantiate bidding platforms instead of finding them?
        StartCoroutine(PopulatePlatforms());
    }

    private void AnimateAuctionStart()
    {
        GlobalHUDController globalHUD = GetComponentInChildren<GlobalHUDController>();
        StartCoroutine(globalHUD.DisplayStartScreen(biddingBeginDelay));
    }

    private IEnumerator PopulatePlatforms()
    {
        yield return new WaitForSeconds(biddingBeginDelay);
        if (!(availableAuctionStages.Length == biddingPlatforms.Length))
        {
            Debug.Log("Not enough available auctionStages or biddingPlatforms!");
        }

        lastExtendedAuction = biddingPlatforms[0];
        lastExtendedAuction.onBiddingEnd += EndAuction;

        for (int i = 0; i < biddingPlatforms.Length; i++)
        {
            availableAuctionStages[i].Promote(out BiddingRound biddingRound);
            biddingPlatforms[i].ActiveBiddingRound = biddingRound;
            biddingPlatforms[i].SetItem(biddingRound.items[0]);
            biddingPlatforms[i].onBiddingExtended += SetPrioritizedPlatform;
        }
    }

    private void SetPrioritizedPlatform(BiddingPlatform biddingPlatform)
    {
        lastExtendedAuction.onBiddingEnd -= EndAuction;
        lastExtendedAuction = biddingPlatform;
        lastExtendedAuction.onBiddingEnd += EndAuction;
    }

    private void EndAuction(BiddingPlatform biddingPlatform)
    {
        lastExtendedAuction.onBiddingEnd = null;

        LeanTween.alpha(gunConstructionPanels[0].parent.GetComponent<RectTransform>(), 1f, 1f).setEase(LeanTweenType.linear);
        MusicTrackManager.Singleton.SwitchTo(MusicType.FANFARE);
        
        for (int i = 0; i < playersInAuction.Count; i++)
        {
            StartCoroutine(AnimateGunConstruction(playersInAuction.ElementAt(playersInAuction.Count-i-1), gunConstructionPanels[i]));
        }

        StartCoroutine(MatchController.Singleton.WaitAndStartNextRound());
        PlayerInputManagerController.Singleton.playerInputs.ForEach(playerInput => playerInput.RemoveListeners());
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

    private IEnumerator AnimateGunConstruction(PlayerManager playerManager, RectTransform parent)
    {
        yield return new WaitForSeconds(1);
        GameObject body = Instantiate(playerManager.identity.Body.augment, parent);
        body.LeanScale(new Vector3 (gunConstructionScale, gunConstructionScale, gunConstructionScale), 0.5f);
        body.LeanRotateY(90f, 2f);
        GunBody gunBody = body.GetComponent<GunBody>();

        yield return new WaitForSeconds(1);
        GameObject barrel = Instantiate(playerManager.identity.Barrel.augment, gunBody.attachmentSite.position, gunBody.attachmentSite.rotation, parent);
        barrel.LeanScale(new Vector3(gunConstructionScale, gunConstructionScale, gunConstructionScale), 0.5f);
        barrel.LeanRotateY(90f, 2f);
        GunBarrel gunBarrel = barrel.GetComponent<GunBarrel>();

        if (playerManager.identity.Extension)
        {
            yield return new WaitForSeconds(1);
            GameObject extension = Instantiate(playerManager.identity.Extension.augment, gunBarrel.attachmentPoints[0].position, gunBarrel.attachmentPoints[0].rotation, parent);
            GunExtension gunExtension = extension.GetComponent<GunExtension>();
            var outputs = new List<Transform>();
            outputs.AddRange(gunExtension.outputs);
            outputs.AddRange(gunExtension.AttachToTransforms(gunBarrel.attachmentPoints));
            extension.LeanScale(new Vector3(gunConstructionScale, gunConstructionScale, gunConstructionScale), 0.5f);
            extension.LeanRotateY(90f, 2f);
        }
        TMP_Text name = parent.GetComponentInChildren<TMP_Text>();
        name.text = GunFactory.GetGunName(playerManager.identity.Body, playerManager.identity.Barrel, playerManager.identity.Extension);
        yield return null;
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
