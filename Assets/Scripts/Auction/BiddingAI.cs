using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BiddingAI : BiddingPlayer
{
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Animator animator;
    private Dictionary<BiddingPlatform, int> priorities = new Dictionary<BiddingPlatform, int>();
    [SerializeField]
    private BiddingPlatform currentDestination;
    [SerializeField]
    private Vector3 platformDestinationOffset = Vector3.back * 2;

    private bool shouldEvaluate = true;

    void Start()
    {
        GetComponent<PlayerIK>().RightHandIKTarget = signTarget;
        agent.updateRotation = false;
        foreach (var platform in AuctionDriver.Singleton.BiddingPlatforms)
        {
            platform.onItemSet += EvaluateItem;
        }
        playerManager.onSelectedBiddingPlatformChange += OnBiddingPlatformChange;
        playerManager.onSelectedBiddingPlatformChange += AnimateChipStatus;
    }

    public void SetIdentity(PlayerIdentity identity)
    {
        chipText.text = identity.chips.ToString();
        identity.onChipChange += AnimateChipStatus;
    }

    private IEnumerator WaitAndEvaluate()
    {
        foreach (BiddingPlatform platform in AuctionDriver.Singleton.BiddingPlatforms)
            if (platform.IsActive)
                EvaluatePlatformStates(platform);
        yield return new WaitForSeconds(2);
        StartCoroutine(WaitAndEvaluate());
    }

    private void EvaluatePlatformStates(BiddingPlatform platform)
    {
        if (platform.LeadingBidder == playerManager.identity)
            return;

        if (platform.chips >= playerManager.identity.chips)
            priorities[platform] = -1;

        currentDestination = priorities.ToList()
            .OrderByDescending(x => x.Value)
            .Select(x => x.Key).First();

        if (currentDestination)
            agent.SetDestination(currentDestination.transform.position + platformDestinationOffset);

        if (currentDestination == playerManager.SelectedBiddingPlatform)
            OnBiddingPlatformChange(currentDestination);
    }

    private void EvaluateItem(BiddingPlatform platform)
    {
        int priority = 0;
        switch (platform.Item.augmentType)
        {
            case AugmentType.Body:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Body.Equals(platform.Item.displayName))
                    .Sum((augment) => augment.KillCount);
                break;
            case AugmentType.Barrel:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Barrel.Equals(platform.Item.displayName))
                    .Sum((augment) => augment.KillCount);
                break;
            case AugmentType.Extension:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Extension.Equals(platform.Item.displayName))
                    .Sum((augment) => augment.KillCount);
                break;
        }
        priorities.Add(platform, priority);
        // Move towards a position that does not obscure the cost text (!)
        agent.SetDestination(platform.transform.position + platformDestinationOffset);

        if (!shouldEvaluate)
            return;
        StartCoroutine(WaitAndEvaluate());
        shouldEvaluate = false;
    }

    private void AnimateBid()
    {
        if (LeanTween.isTweening(signMesh.gameObject) || !currentPlatform)
            return;

        LeanTween.sequence()
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, 90, 0.15f))
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, -90, 0.4f));
    }

    private void OnBiddingPlatformChange(BiddingPlatform platform)
    {
        if (!platform || !currentDestination || platform != currentDestination || platform.LeadingBidder == playerManager.identity)
            return;

        AnimateBid();
        currentDestination.TryPlaceBid(playerManager.identity);
        currentDestination = null;
    }

    private void Update()
    {
        if (!currentDestination)
        {
            animator.SetFloat("Forward", 0f);
            animator.SetFloat("Right", 0f);
            return;
        }
        animator.SetFloat("Forward", Vector3.Dot(agent.velocity, transform.forward) / agent.speed);
        animator.SetFloat("Right", Vector3.Dot(agent.velocity, transform.right) / agent.speed);
    }
}
