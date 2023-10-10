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
    private float biddingBeginDelay = 5f;

    [SerializeField]
    private BiddingPlatform[] biddingPlatforms;
    private RandomisedAuctionStage[] availableAuctionStages;

    [SerializeField]
    private Auctioneer auctioneer;

    private BiddingPlatform lastExtendedAuction;

    private HashSet<PlayerManager> playersInAuction;
    private Dictionary<PlayerManager, int> chipsInPlay = new Dictionary<PlayerManager, int>();

    [SerializeField]
    private PlayerFactory playerFactory;
    
    [SerializeField]
    private RectTransform[] gunConstructionPanels;
    private float gunConstructionScale = 8f;

    // Stages
    // BiddingStage contains a list of items (scriptableobjects)
    private void Start()
    {
        Cursor.visible = true;
#if UNITY_EDITOR
        biddingBeginDelay = 2f;
#endif
        if (MatchController.Singleton.RoundCount == 1)
            availableAuctionStages = new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.BodyAuction };
        if (MatchController.Singleton.RoundCount == 2)
            availableAuctionStages = new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.BarrelAuction };
        if (MatchController.Singleton.RoundCount == 3)
            availableAuctionStages = new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.ExtensionAuction };
        if (MatchController.Singleton.RoundCount > 3)
            availableAuctionStages = new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.BodyAuction, StaticInfo.Singleton.BarrelAuction, StaticInfo.Singleton.ExtensionAuction };

        playerFactory = GetComponent<PlayerFactory>();
        playerFactory.InstantiatePlayersBidding();
        playersInAuction = new HashSet<PlayerManager>(FindObjectsOfType<PlayerManager>());

        AnimateAuctionStart();
        StartCoroutine(PopulatePlatforms());
    }

    private void OnDestroy()
    {
        for (int i = 0; i < biddingPlatforms.Length; i++)
        {
            biddingPlatforms[i].onBiddingExtended -= SetPrioritizedPlatform;
            biddingPlatforms[i].onBidPlaced -= ActivateAuctioneerBid;
            biddingPlatforms[i].onBiddingEnd -= ActivateAuctioneerSell;
        }
    }

    private void AnimateAuctionStart()
    {
        GlobalHUDController globalHUD = GetComponentInChildren<GlobalHUDController>();
        StartCoroutine(globalHUD.DisplayStartScreen(biddingBeginDelay));
    }

    private IEnumerator PopulatePlatforms()
    {
        yield return new WaitForSeconds(biddingBeginDelay);

        lastExtendedAuction = biddingPlatforms[0];
        lastExtendedAuction.onBiddingEnd += EndAuction;

        List<BiddingRound> biddingRounds = new List<BiddingRound>();
        for (int i = 0; i < availableAuctionStages.Length; i++)
        {
            availableAuctionStages[i].Promote(out BiddingRound biddingRound);
            biddingRounds.Add(biddingRound);
        }
        bool isMultiple = availableAuctionStages.Length >= biddingPlatforms.Length;

        for (int i = 0; i < biddingPlatforms.Length; i++)
        {
            biddingPlatforms[i].ActiveBiddingRound = biddingRounds[isMultiple ? i : 0];
            biddingPlatforms[i].SetItem(biddingRounds[isMultiple ? i : 0].items[isMultiple ? 0 : i]);
            biddingPlatforms[i].onBiddingExtended += SetPrioritizedPlatform;
            biddingPlatforms[i].onBidPlaced += ActivateAuctioneerBid;
            biddingPlatforms[i].onBiddingEnd += ActivateAuctioneerSell;
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
        MusicTrackManager.Singleton.SwitchTo(MusicType.CONSTRUCTION_FANFARE);
        
        for (int i = 0; i < playersInAuction.Count; i++)
        {
            StartCoroutine(AnimateGunConstruction(playersInAuction.ElementAt(playersInAuction.Count-i-1), gunConstructionPanels[i]));
        }

        StartCoroutine(MatchController.Singleton.WaitAndStartNextRound());
        PlayerInputManagerController.Singleton.playerInputs.ForEach(playerInput => playerInput.RemoveListeners());
    }

    private IEnumerator AnimateGunConstruction(PlayerManager playerManager, RectTransform parent)
    {
        yield return new WaitForSeconds(1);
        GameObject body = Instantiate(playerManager.identity.Body.augment, parent);
        AnimatePopUp(body);
        GunBody gunBody = body.GetComponent<GunBody>();
        
        yield return new WaitForSeconds(1);
        GameObject barrel = Instantiate(playerManager.identity.Barrel.augment, gunBody.attachmentSite.position, gunBody.attachmentSite.rotation, parent);
        AnimatePopUp(barrel);
        GunBarrel gunBarrel = barrel.GetComponent<GunBarrel>();

        if (playerManager.identity.Extension)
        {
            yield return new WaitForSeconds(1);
            GameObject extension = Instantiate(playerManager.identity.Extension.augment, gunBarrel.attachmentPoints[0].position, gunBarrel.attachmentPoints[0].rotation, parent);
            GunExtension gunExtension = extension.GetComponent<GunExtension>();
            var outputs = new List<Transform>();
            outputs.AddRange(gunExtension.outputs);
            outputs.AddRange(gunExtension.AttachToTransforms(gunBarrel.attachmentPoints));
            AnimatePopUp(extension);
        }
        TMP_Text name = parent.GetComponentInChildren<TMP_Text>();
        name.text = GunFactory.GetGunName(playerManager.identity.Body, playerManager.identity.Barrel, playerManager.identity.Extension);
        yield return null;
    }

    private void AnimatePopUp(GameObject gameObject)
    {
        gameObject.LeanScale(new Vector3(gunConstructionScale, gunConstructionScale, gunConstructionScale), 0.5f);
        gameObject.LeanRotateY(90f, 2f);
    }

    private void ActivateAuctioneerBid(BiddingPlatform platform)
    {
        // TODO: Refactor this
        auctioneer.BidOn(biddingPlatforms.ToList().IndexOf(platform));
    }

    private void ActivateAuctioneerSell(BiddingPlatform biddingPlatform)
    {
        auctioneer.Sell();
    }
}
