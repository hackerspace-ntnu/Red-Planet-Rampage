using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;

// TODO
// Topic for discussion:
// Using RequireComponent + GetComponent in Awake,
// vs manually assigning component references
//
[RequireComponent(typeof(Timer))]
[RequireComponent(typeof(PlayerFactory))]
public class AuctionDriver : NetworkBehaviour
{
    [SerializeField]
    private float biddingBeginDelay = 2f;
    private bool isAuctionStart = false;

    [SerializeField]
    private BiddingPlatform[] biddingPlatforms;
    [HideInInspector]
    public BiddingPlatform[] BiddingPlatforms => biddingPlatforms;
    private RandomisedAuctionStage[] availableAuctionStages;
    [SerializeField]
    private YieldZone[] yieldZones;
    [SerializeField]
    private Transform yieldPosition;
    public Vector3 YieldPosition => yieldPosition.position;
    private List<PlayerManager> yieldingPlayers = new();
    public int YieldingPlayerCount => yieldingPlayers.Count;
    
    public delegate void AuctionEvent();
    public AuctionEvent OnYieldChange;

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

    [SerializeField]
    private Animator cameraAnimator;
    [SerializeField]
    private Vector3 cameraStandbyPosition;
    private int screenShakeTween;
    public GameObject Camera => cameraAnimator.gameObject;
    [SerializeField]
    private Camera extraCamera;

    public static AuctionDriver Singleton;

    private void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate
    }

    // Stages
    // BiddingStage contains a list of items (scriptableobjects)
    private void Start()
    {
        StartCoroutine(WaitAndStartAuction());
    }

    private IEnumerator WaitAndStartAuction()
    {
        // TODO add a timeout to this kinda thing
        while (FindObjectsOfType<PlayerManager>().Count() < Peer2PeerTransport.NumPlayers)
            yield return null;

        availableAuctionStages = MatchRules.Current.AuctionForRound(MatchController.Singleton.RoundCount) switch
        {
            AuctionType.Body => new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.BodyAuction },
            AuctionType.Barrel => new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.BarrelAuction },
            AuctionType.Extension => new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.ExtensionAuction },
            AuctionType.Random => new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.EverythingAuction },
            _ => new WeightedRandomisedAuctionStage[] { StaticInfo.Singleton.BodyAuction, StaticInfo.Singleton.BarrelAuction, StaticInfo.Singleton.ExtensionAuction }
        };

        playerFactory = GetComponent<PlayerFactory>();
        playersInAuction = new HashSet<PlayerManager>(FindObjectsOfType<PlayerManager>());
        yieldZones.ToList().ForEach(zone => zone.Subscribe());

        StartCoroutine(WaitAndStartCameraAnimation());
        StartCoroutine(PopulatePlatforms());
        LoadingScreen.Singleton.Hide();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < biddingPlatforms.Length; i++)
        {
            biddingPlatforms[i].onBiddingExtended -= SetPrioritizedPlatform;
            biddingPlatforms[i].onBidPlaced -= ActivateAuctioneerBid;
            biddingPlatforms[i].onBiddingEnd -= ActivateAuctioneerSell;
            biddingPlatforms[i].onBiddingExtended -= ActivateAuctioneerHaste;
            biddingPlatforms[i].onBidDenied -= ActivateAuctioneerMissing;
        }
    }

    private IEnumerator WaitAndStartCameraAnimation()
    {
        yield return new WaitForSeconds(biddingBeginDelay);
        cameraAnimator.SetTrigger("start");
        isAuctionStart = true;
    }

    [Server]
    public void StopAuctionEarly()
    {
        biddingPlatforms.ToList().ForEach(platform => platform.ForceEndAuction());
    }

    public void AddYieldingPlayer(PlayerManager player)
    {
        if (yieldingPlayers.Contains(player))
            return;

        yieldingPlayers.Add(player);
        OnYieldChange?.Invoke();


        if (isAuctionStart && IsAuctionYielded())
            StartCoroutine(nameof(WaitAndTryAuctionEnd));
        else
            StopCoroutine(nameof(WaitAndTryAuctionEnd));
            
    }

    private bool IsAuctionYielded()
    {
        if (yieldingPlayers.Count == Peer2PeerTransport.NumPlayers)
            return true;

        var relevantPlatforms = biddingPlatforms.Where(platform => platform.IsActive);
        bool isAnyContesting = playersInAuction.Except(yieldingPlayers)
            .Where(player =>
                relevantPlatforms.Where(platform =>
                    platform.CanBid(player.identity)
                    && platform.LeadingBidder != player.id)
                .Any())
            .Any();

        return !isAnyContesting;
    }

    public bool IsPlayerYielding(PlayerManager player)
    {
        return yieldingPlayers.Contains(player);
    }

    private IEnumerator WaitAndTryAuctionEnd()
    {
        var yieldZones = FindObjectsOfType<YieldZone>().ToList();
        yieldZones.ForEach(sign => sign.SetRemainingTimeText(3));
        yield return new WaitForSeconds(1f);
        yieldZones.ForEach(sign => sign.SetRemainingTimeText(2));
        yield return new WaitForSeconds(1f);
        yieldZones.ForEach(sign => sign.SetRemainingTimeText(1));
        yield return new WaitForSeconds(1f);
        if (IsAuctionYielded() && isServer)
            StopAuctionEarly();
    }

    public void RemoveYieldingPlayer(PlayerManager player)
    {
        if (!yieldingPlayers.Remove(player))
            return;

        StopCoroutine(nameof(WaitAndTryAuctionEnd));
        OnYieldChange?.Invoke();
    }

    public void ScreenShake()
    {
        if (cameraAnimator.transform.localPosition != cameraStandbyPosition)
            return;
        cameraAnimator.enabled = false;
        if (LeanTween.isTweening(screenShakeTween))
        {
            LeanTween.cancel(screenShakeTween);
            cameraAnimator.transform.localPosition = cameraStandbyPosition;
        }
        screenShakeTween = cameraAnimator.gameObject.LeanMoveLocal(cameraStandbyPosition * 1.01f, 0.2f).setEaseShake().id;
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
            biddingPlatforms[i].onBiddingExtended += ActivateAuctioneerHaste;
            biddingPlatforms[i].onBidDenied += ActivateAuctioneerMissing;
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
        if (isServer)
            StartCoroutine(WaitAndSwitchToItemSelect());
    }

    private IEnumerator WaitAndSwitchToItemSelect()
    {
        // Wait a couple o' frames so gun parts are sent to their respective players
        yield return null;
        yield return null;
        Peer2PeerTransport.UpdatePlayerDetailsAfterAuction();
        RpcSwitchToItemSelect();
    }

    [ClientRpc]
    private void RpcSwitchToItemSelect()
    {
        lastExtendedAuction.onBiddingEnd = null;
        Camera.GetComponent<Camera>().enabled = false;
        extraCamera.enabled = true;
        PlayerInputManagerController.Singleton.PlayerInputManager.splitScreen = true;
        playerFactory.InstantiatePlayerSelectItems();
        GetComponent<ItemSelectManager>().StartTrackingMenus();
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

    private void ActivateAuctioneerHaste(BiddingPlatform platform)
    {
        auctioneer.Haste();
    }

    private void ActivateAuctioneerMissing(BiddingPlatform platform)
    {
        auctioneer.Missing();
    }
}
