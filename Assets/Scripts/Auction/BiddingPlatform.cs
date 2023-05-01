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

        if (playerIdentity.chips > chips && playerIdentity != leadingBidder)
        {
            // Refund
            if (leadingBidder)
            {
                leadingBidder.UpdateChip(chips);
            }
            //activeBiddingRound.chips[0] = chips++;
            chips++;
            playerIdentity.UpdateChip(-chips);
            itemCostText.text = chips.ToString();
            leadingBidder = playerIdentity;
            material.SetColor("_BidderColor", playerIdentity.color);
            LeanTween.value(gameObject, UpdateBorder, 0f, 1f, borderTweenDuration);

            if ((auctionTimer.WaitTime - auctionTimer.ElapsedTime) < bumpTime)
            {
                auctionTimer.AddTime(bumpTime);
                onBiddingExtended(this);
            }

            return true;
        }
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
        {
            leadingBidder.PerformTransaction(item);

            // Animate weapon flying towards winner
            LeanTween.value(gameObject, UpdateBorder, 1f, 0f, borderTweenDuration);
            augmentModel.LeanScale(new Vector3(40f, 40f, 40f), 0.2f);
            LeanTween.followLinear(augmentModel.transform, leadingBidder.transform, LeanProp.position, 20f);
            Destroy(augmentModel, 0.6f);
        }
        else
        {
            augmentModel.LeanScale(new Vector3(0f, 0f), 0.3f);
            Destroy(augmentModel, 0.5f);
        }
        onBiddingEnd?.Invoke(this);
    }

    public void SetItem(Item item)
    {
        this.item = item;
        itemNameText.text = item.displayName;
        itemDescriptionText.text = item.displayDescription;
        itemCostText.text = chips.ToString();
        augmentModel = Instantiate(item.augment, modelHolder.transform);

        // All barrels have their origins skewed by design, this is the best solution to center barrels as long as that is the case.
        if (item.augmentType == AugmentType.Barrel)
        {
            augmentModel.transform.Translate(new Vector3(-1f, 0f, 0f));
        }

        augmentModel.transform.Rotate(new Vector3(0f, 90f));
        augmentModel.LeanScale(new Vector3(100f,100f,100f), 0.5f);


#if UNITY_EDITOR
        auctionTimer.StartTimer(10);
#else
        auctionTimer.StartTimer(baseWaitTime);
#endif
    }
}
