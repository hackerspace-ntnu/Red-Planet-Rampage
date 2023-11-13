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
    private AudioSource audioSourceFX;
    [SerializeField]
    private AudioClip[] bid;
    [SerializeField]
    private AudioClip[] haste;
    [SerializeField]
    private AudioClip[] missingChips;
    [SerializeField]
    private AudioClip[] sold;

    private void Start()
    {
        // TODO: Replace with queue after initial auctioneer greet animation once that is implemented
        StartCoroutine(TryQueueVoice(2.5f, bid));
    }

    public void BidOn(int platformIndex)
    {
        audioSource.Stop();
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
        audioSource.clip = bid.RandomElement();
        audioSource.Play();
        StartCoroutine(TryQueueVoice(audioSource.clip.length, haste));
    }

    private IEnumerator TryQueueVoice(float time, AudioClip[] audioClips)
    {
        yield return new WaitForSeconds(time+1);
        if (!audioSource.isPlaying)
        {
            audioSource.clip = audioClips.RandomElement();
            audioSource.Play();
        }
            
    }

    public void Sell()
    {
        animator.SetTrigger("Sell");
        audioSource.Stop();
        audioSource.clip = sold.RandomElement();
        audioSource.Play();
        audioSourceFX.PlayDelayed(0.5f);
    }

    public void Haste()
    {
        audioSource.Stop();
        audioSource.clip = haste.RandomElement();
        audioSource.Play();
        StartCoroutine(TryQueueVoice(audioSource.clip.length, haste));
    }

    public void Missing()
    {
        audioSource.Stop();
        audioSource.clip = missingChips.RandomElement();
        audioSource.Play();
        StartCoroutine(TryQueueVoice(audioSource.clip.length, bid));
    }
}
