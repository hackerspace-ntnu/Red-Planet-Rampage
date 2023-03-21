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
    private float baseWaitTime;

    [SerializeField]
    private float bumpTime = 5.0f;

    public BiddingRound ActiveBiddingRound;

    [SerializeField]
    private Timer auctionTimer;

    [SerializeField]
    private float borderTweenDuration = 0.2f;

    private Material material;

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
        if (ActiveBiddingRound == null)
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

            if ((auctionTimer.WaitTime - auctionTimer.ElapsedTime) < bumpTime) { auctionTimer.AddTime(bumpTime); }

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
            LeanTween.value(gameObject, UpdateBorder, 1f, 0f, borderTweenDuration);
        }

        //TODO: Remove this, call from auction driver or matchmanager
        StartCoroutine(MatchController.Singleton.WaitAndStartNextRound());
        PlayerInputManagerController.Singleton.playerInputs.ForEach(playerInput => playerInput.RemoveListeners());
    }

    public void SetItem(Item item)
    {
        this.item = item;
        itemNameText.text = item.displayName;
        itemDescriptionText.text = item.displayDescription;
        itemCostText.text = chips.ToString();
        auctionTimer.StartTimer(baseWaitTime);
    }
}
