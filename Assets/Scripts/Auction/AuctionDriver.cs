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
        availableAuctionStages = new RandomisedAuctionStage[] { StaticInfo.Singleton.BodyAuction, StaticInfo.Singleton.BarrelAuction, StaticInfo.Singleton.ExtensionAuction };
        playerFactory = GetComponent<PlayerFactory>();
        playerFactory.InstantiatePlayersBidding();
        playersInAuction = new HashSet<PlayerManager>(FindObjectsOfType<PlayerManager>());

        AnimateAuctionStart();
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

}
