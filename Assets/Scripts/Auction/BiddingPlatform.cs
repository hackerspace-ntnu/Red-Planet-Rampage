using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BiddingPlatform : MonoBehaviour
{
    [SerializeField]
    private Item item;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager playerManager)){
            playerManager.selectedBiddingPlatform = this;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager playerManager))
        {
            if (playerManager.selectedBiddingPlatform == this)
            playerManager.selectedBiddingPlatform = null;
        }
    }

    public bool TryPlaceBid(PlayerIdentity playerIdentity)
    {
        if (playerIdentity.chips > chips)
        {
            // TODO: Rewrite this to use auction driver

            // Refund
            if (leadingBidder)
            {
                leadingBidder.UpdateChip(chips);
            }

            chips++;
            playerIdentity.UpdateChip(-chips); 
            itemCostText.text = chips.ToString();
            leadingBidder = playerIdentity;
            return true;
        }
        return false;
    }

    public void setItem(Item item)
    {
        this.item = item;
        itemNameText.text = item.displayName;
        itemDescriptionText.text = item.displayDescription;
        itemCostText.text = chips.ToString();
    }
}
