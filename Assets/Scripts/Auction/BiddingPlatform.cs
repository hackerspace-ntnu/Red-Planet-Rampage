using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Timer))]
public class BiddingPlatform : MonoBehaviour
{
    [SerializeField]
    private Item item;
    public Item Item => item;

    [SerializeField]
    public int chips = 0;

    [SerializeField]
    private PlayerIdentity leadingBidder;

    [SerializeField]
    private TMP_Text itemNameText;

    [SerializeField]
    private TMP_Text itemDescriptionText;

    [SerializeField]
    private TMP_Text itemCostText;

    [SerializeField]
    private TMP_Text timerText;

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
    private float borderTweenDuration = 0.2f;

    private GameObject augmentModel; 

    private Material material;

    public delegate void BiddingEvent(BiddingPlatform biddingPlatform);

    public BiddingEvent onBiddingExtended;
    public BiddingEvent onBiddingEnd;
    public BiddingEvent onBidPlaced;
    public BiddingEvent onBidDenied;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager playerManager))
        {
            playerManager.SelectedBiddingPlatform = this;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager playerManager))
        {
            if (playerManager.SelectedBiddingPlatform == this)
                playerManager.SelectedBiddingPlatform = null;
        }
    }

    private void Awake()
    {
        auctionTimer = GetComponent<Timer>();
        auctionTimer.OnTimerUpdate += UpdateTimer;
        auctionTimer.OnTimerRunCompleted += EndAuction;
        material = GetComponent<MeshRenderer>().material;
        material.SetFloat("_Scale", 0f);
    }

    private void OnDestroy()
    {
        auctionTimer.OnTimerUpdate -= UpdateTimer;
        auctionTimer.OnTimerRunCompleted -= EndAuction;
    }

    public bool TryPlaceBid(PlayerIdentity playerIdentity)
    {
        if (ActiveBiddingRound == null || item == null || Mathf.Round(auctionTimer.ElapsedTime) <= 0)
        {
            Debug.Log("No active biddingRound on biddingPlatform!");
            return false;
        }
        bool leadingPlayerCanIncrement = playerIdentity == leadingBidder && playerIdentity.chips > 0;
        if (playerIdentity.chips > chips || leadingPlayerCanIncrement)
        {
            // Refund
            if (leadingBidder)
            {
                leadingBidder.UpdateChip(chips);
            }
            chips++;
            playerIdentity.UpdateChip(-chips);
            itemCostText.text = chips.ToString();
            leadingBidder = playerIdentity;
            material.SetColor("_BidderColor", playerIdentity.color);
            LeanTween.value(gameObject, UpdateBorder, 0f, 1f, borderTweenDuration);
            onBidPlaced(this);

            if ((auctionTimer.WaitTime - auctionTimer.ElapsedTime) < bumpTime)
            {
                auctionTimer.AddTime(bumpTime);
                onBiddingExtended(this);
            }
            return true;
        }
        onBidDenied(this);
        return false;
    }

    private void UpdateBorder(float scale)
    {
        material.SetFloat("_Scale", scale);
    }

    private void UpdateTimer()
    {
        timerText.text = Mathf.Round(auctionTimer.WaitTime - auctionTimer.ElapsedTime).ToString();
    }

    private void EndAuction()
    {
        if (leadingBidder)
            leadingBidder.PerformTransaction(item);

        Destroy(augmentModel, 0.5f);
        onBiddingEnd?.Invoke(this);
        gameObject.LeanScale(Vector3.zero, 0.5f).setEaseInOutExpo();
    }

    public void SetItem(Item item)
    {
        this.item = item;
        itemNameText.text = item.displayName;
        itemDescriptionText.text = item.displayDescription;
        itemCostText.text = chips.ToString();
        augmentModel = Instantiate(item.augment, modelHolder.transform);
        augmentModel.transform.localScale = Vector3.one / 20.0f;
        augmentModel.transform.localPosition = Vector3.zero;

        modelHolder.transform.Rotate(new Vector3(90f, 0f));
        LeanTween.sequence()
            .append(LeanTween.rotateAroundLocal(augmentModel, Vector3.up, 360, 2.5f).setLoopCount(-1))
            .append(LeanTween.moveLocalY(augmentModel, 0.01f, 3.0f).setLoopPingPong().setEaseInOutSine());


#if UNITY_EDITOR
        auctionTimer.StartTimer(10);
#else
        auctionTimer.StartTimer(baseWaitTime);
#endif
    }
}
