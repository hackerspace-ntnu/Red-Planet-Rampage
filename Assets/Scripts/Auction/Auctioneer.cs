using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auctioneer : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    public void BidOn(int platformIndex)
    {
        Debug.Log("Bid at "+platformIndex);
        switch (platformIndex)
        {
            case 0:
                animator.SetTrigger("Auction1Bid");
                break;
            case 1:
                animator.SetTrigger("Auction2Bid");
                break;
            case 2:
                animator.SetTrigger("Auction3Bid");
                break;
            default:
                throw new ArgumentException("No auctioneer animation for given platform index");
        }
    }

    public void Sell()
    {
        animator.SetTrigger("Sell");
    }
}
