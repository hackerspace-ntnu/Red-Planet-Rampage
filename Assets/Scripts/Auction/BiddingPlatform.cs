using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Timer))]
public class BiddingPlatform : NetworkBehaviour
{
    [SerializeField]
    private Item item;
    public Item Item => item;

    [SerializeField]
    public int chips = 0;

    [SerializeField]
    private uint leadingBidder = InvalidID;
    public uint LeadingBidder => leadingBidder;
    private const uint InvalidID = 255;
    private PlayerIdentity leadingBidderIdentity;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private TMP_Text itemNameText;

    [SerializeField]
    private TMP_Text itemDescriptionText;

    [SerializeField]
    private TMP_Text itemCostText;

    [SerializeField]
    private GameObject modelHolder;

    [SerializeField]
    private float baseWaitTime;

    [SerializeField]
    private float bumpTime = 5.0f;

    public BiddingRound ActiveBiddingRound;

    [SerializeField]
    private Timer auctionTimer;

    [SerializeField]
    private GameObject description;

    [SerializeField]
    private float borderTweenDuration = 0.2f;

    private GameObject augmentModel;

    [SerializeField]
    private Color startingMaterialColor;
    private Material material;

    [SerializeField]
    private Image radialUI;

    [SerializeField]
    private Image augmentSymbol;
    [SerializeField]
    private Sprite bodySymbol;
    [SerializeField]
    private Sprite barrelSymbol;
    [SerializeField]
    private Sprite extensionSymbol;

    private int playerCount = 0;

    private bool isActive = false;
    public bool IsActive => isActive;

    public delegate void BiddingEvent(BiddingPlatform biddingPlatform);

    public BiddingEvent onItemSet;
    public BiddingEvent onBiddingExtended;
    public BiddingEvent onBiddingEnd;
    public BiddingEvent onBidPlaced;
    public BiddingEvent onBidDenied;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager playerManager))
        {
            playerManager.SelectedBiddingPlatform = this;
            description.LeanScale(Vector3.one, 0.2f);
            playerCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager playerManager))
        {
            if (playerManager.SelectedBiddingPlatform == this)
                playerManager.SelectedBiddingPlatform = null;

            playerCount--;
            if (playerCount == 0)
                description.LeanScale(Vector3.zero, 0.2f);
        }
    }

    private void Awake()
    {
        auctionTimer = GetComponent<Timer>();
        auctionTimer.OnTimerRunCompleted += EndAuction;
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_Scale", 0f);
        material.SetColor("_BidderColor", startingMaterialColor);
        description.transform.localScale = Vector3.zero;
        radialUI.material = Instantiate(radialUI.material);
        radialUI.material.SetFloat("_Arc2", 0);
    }

    private void OnDestroy()
    {
        auctionTimer.OnTimerRunCompleted -= EndAuction;
    }

    public void PlaceBid(PlayerIdentity playerIdentity)
    {
        if (ActiveBiddingRound == null || item == null || Mathf.Round(auctionTimer.ElapsedTime) <= 0)
        {
            Debug.LogWarning("No active biddingRound on biddingPlatform!");
            return;
        }

        CmdPlaceBid(playerIdentity.id);
    }

    // All players should be able to place bids, thus we ignore authority (for the time being).
    // TODO verify that the player is on the platform and is from the connection that calls this cmd.
    [Command(requiresAuthority = false)]
    private void CmdPlaceBid(uint playerID)
    {
        if (ActiveBiddingRound == null || item == null || Mathf.Round(auctionTimer.ElapsedTime) <= 0)
        {
            Debug.LogWarning("No active biddingRound on biddingPlatform!");
            return;
        }

        // TODO verify that this player belongs to the source connection
        if (!MatchController.Singleton.PlayerById.TryGetValue(playerID, out var player))
        {
            Debug.LogError($"Bidding platform received invalid player {playerID} from client!");
            return;
        }
        var playerIdentity = player.identity;

        Debug.Log($"Got bidding request from {playerIdentity.ToColorString()} (chips={playerIdentity.chips}) on {item.displayName} (chips={chips}, leader={leadingBidderIdentity?.ToColorString()})");

        bool leadingPlayerCanIncrement = playerIdentity.id == leadingBidder && playerIdentity.chips > 0;
        if (playerIdentity.chips > chips || leadingPlayerCanIncrement)
        {
            Debug.Log($"Accepted request from {playerIdentity.ToColorString()} on {item.displayName}");
            RpcAcceptBid(playerIdentity.id);
        }
        else
        {
            Debug.Log($"Denied request from {playerIdentity.ToColorString()} on {item.displayName}");
            RpcDenyBid();
        }
    }

    [ClientRpc]
    private void RpcAcceptBid(uint playerID)
    {
        if (!MatchController.Singleton.PlayerById.TryGetValue(playerID, out var player))
        {
            Debug.LogError($"Bidding platform received invalid player {playerID} from server!");
            return;
        }
        var playerIdentity = player.identity;

        // TODO consider rewriting some of the following

        // Refund
        if (leadingBidderIdentity)
        {
            leadingBidderIdentity.UpdateChip(chips);
            if (playerIdentity.id != leadingBidder)
            {
                audioSource.Play();
                AuctionDriver.Singleton.ScreenShake();
            }
        }

        chips++;
        playerIdentity.UpdateChip(-chips);
        itemCostText.text = chips.ToString();
        LeanTween.value(
            gameObject,
            (color) => material.SetColor("_BidderColor", color),
            leadingBidderIdentity ? leadingBidderIdentity.color : Color.black,
            playerIdentity.color, 0.2f)
            .setEaseInOutBounce();
        leadingBidder = playerIdentity.id;
        leadingBidderIdentity = playerIdentity;
        LeanTween.value(gameObject, UpdateBorder, 0f, 1f, borderTweenDuration);
        onBidPlaced?.Invoke(this);

        if ((auctionTimer.WaitTime - auctionTimer.ElapsedTime) < bumpTime)
        {
            auctionTimer.AddTime(bumpTime);
            onBiddingExtended?.Invoke(this);
        }

        Debug.Log($"Accepted bid on {item.displayName} (chips={chips}, leader={leadingBidderIdentity?.ToColorString()})");
    }

    [ClientRpc]
    private void RpcDenyBid()
    {
        onBidDenied?.Invoke(this);
        Debug.Log($"Denied bid on {item.displayName}");
    }

    private void UpdateBorder(float scale)
    {
        material.SetFloat("_Scale", scale);
    }

    private void EndAuction()
    {
        Debug.Log($"Ending auction for {item.displayName} with {leadingBidder} = {(leadingBidderIdentity ? leadingBidderIdentity.ToColorString() : "nobody")} as the winner");
        if (leadingBidder != InvalidID && isServer)
        {
            RpcPerformTransaction(leadingBidder, item.id);
        }

        Destroy(augmentModel, 0.5f);
        isActive = false;
        onBiddingEnd?.Invoke(this);
        gameObject.LeanScale(Vector3.zero, 0.5f).setEaseInOutExpo();
    }

    [ClientRpc]
    private void RpcPerformTransaction(uint playerID, string itemID)
    {
        if (!MatchController.Singleton.PlayerById.TryGetValue(playerID, out var player))
        {
            Debug.LogError($"Bidding platform received invalid player {playerID} from server!");
            return;
        }
        if (!StaticInfo.Singleton.ItemsById.TryGetValue(itemID, out var item))
        {
            Debug.LogError($"Bidding platform received invalid item {itemID} from server!");
            return;
        }
        Debug.Log($"Rewarding {item.displayName} to {player.identity.ToColorString()}");
        player.identity.PerformTransaction(item);
    }

    public void SetItem(Item item)
    {
        if (isServer)
            RpcSetItem(item.id);
    }

    [ClientRpc]
    private void RpcSetItem(string itemID)
    {
        if (!StaticInfo.Singleton.ItemsById.TryGetValue(itemID, out var item))
        {
            Debug.LogError($"Bidding platform received invalid item {itemID} from server!");
            return;
        }

        this.item = item;
        itemNameText.text = item.displayName;
        itemDescriptionText.text = item.displayDescription;
        itemCostText.text = chips.ToString();
        switch (item.augmentType)
        {
            case AugmentType.Body:
                augmentSymbol.sprite = bodySymbol;
                break;
            case AugmentType.Barrel:
                augmentSymbol.sprite = barrelSymbol;
                break;
            case AugmentType.Extension:
                augmentSymbol.sprite = extensionSymbol;
                break;
        }


        augmentModel = Instantiate(item.augment, modelHolder.transform);
        augmentModel.transform.localScale = Vector3.one / 20f;
        augmentModel.transform.localPosition = -Augment.Midpoint(augmentModel, item.augmentType).localPosition / 20f;
        Augment.DisableInstance(augmentModel, item.augmentType);

        modelHolder.transform.parent.Rotate(new Vector3(90f, 0f));
        LeanTween.sequence()
            .append(LeanTween.rotateAroundLocal(modelHolder, Vector3.up, 360, 2.5f).setLoopCount(-1))
            .append(LeanTween.moveLocalY(modelHolder, 0.01f, 3.0f).setLoopPingPong().setEaseInOutSine());
        isActive = true;
        onItemSet?.Invoke(this);
#if UNITY_EDITOR
        auctionTimer.StartTimer(10);
#else
        auctionTimer.StartTimer(baseWaitTime);
#endif
    }
}
