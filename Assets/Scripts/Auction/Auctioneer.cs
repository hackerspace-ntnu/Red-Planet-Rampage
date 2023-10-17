using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollectionExtensions;

public class Auctioneer : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] bid;
    [SerializeField]
    private AudioClip[] haste;
    [SerializeField]
    private AudioClip[] missingChips;
    [SerializeField]
    private AudioClip[] sold;


    public void BidOn(int platformIndex)
    {
        audioSource.Stop();
        switch (platformIndex)
        {
            case 0:
                animator.SetTrigger("Auction1Bid");
                audioSource.clip = bid.RandomElement();
                break;
            case 1:
                animator.SetTrigger("Auction2Bid");
                audioSource.clip = bid.RandomElement();
                break;
            case 2:
                animator.SetTrigger("Auction3Bid");
                audioSource.clip = bid.RandomElement();
                break;
            default:
                throw new ArgumentException("No auctioneer animation for given platform index");
        }
        audioSource.Play();
    }

    public void Sell()
    {
        animator.SetTrigger("Sell");
        audioSource.Stop();
        audioSource.clip = sold.RandomElement();
        audioSource.Play();
    }

    public void Haste()
    {
        audioSource.Stop();
        audioSource.clip = haste.RandomElement();
        audioSource.Play();
    }

    public void Missing()
    {
        audioSource.Stop();
        audioSource.clip = missingChips.RandomElement();
        audioSource.Play();
    }
}
