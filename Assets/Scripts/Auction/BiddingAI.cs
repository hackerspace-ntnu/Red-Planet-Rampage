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
    private List<(BiddingPlatform, int)> priorities = new List<(BiddingPlatform, int)>();
    private BiddingPlatform currentDestination;
    void Start()
    {
        GetComponent<PlayerIK>().RightHandIKTarget = signTarget;

        foreach (var platform in AuctionDriver.Singleton.BiddingPlatforms)
        {
            platform.onItemSet += EvaluateItem;
            platform.onBidPlaced += EvaluatePlatformStates;
            platform.onBiddingEnd += EvaluatePlatformStates;
        }

        playerManager.onSelectedBiddingPlatformChange += AnimateChipStatus;
    }

    public void SetIdentity(PlayerIdentity identity)
    {
        chipText.text = playerManager.identity.chips.ToString();
        playerManager.identity.onChipChange += AnimateChipStatus;
    }

    private void EvaluatePlatformStates(BiddingPlatform platform)
    {
        if (platform.LeadingBidder == playerManager.identity)
            return;
        if (platform.chips >= playerManager.identity.chips)
        {
            var platformToRemove = priorities.Where(entry => entry.Item1 == platform).FirstOrDefault();
            priorities.Remove(platformToRemove);
            platform.onBidPlaced -= EvaluateItem;
        }

        var targets = priorities.OrderByDescending(priority => priority.Item2);

        if (targets.Count() <= 0)
        {
            currentDestination = null;
            return;
        }

        currentDestination = targets.First().Item1;

    }

    private void EvaluateItem(BiddingPlatform platform)
    {
        int priority = 0;
        switch (platform.Item.augmentType)
        {
            case AugmentType.Body:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Body == platform.Item.name)
                    .Sum((augment) => augment.KillCount);
                break;
            case AugmentType.Barrel:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Barrel == platform.Item.name)
                    .Sum((augment) => augment.KillCount);
                break;
            case AugmentType.Extension:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Extension == platform.Item.name)
                    .Sum((augment) => augment.KillCount);
                break;
        }
        priorities.Add((platform, priority));
    }

    private void AnimateBid()
    {
        if (LeanTween.isTweening(signMesh.gameObject) || !currentPlatform)
            return;

        LeanTween.sequence()
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, 90, 0.15f))
            .append(LeanTween.rotateAroundLocal(signMesh.gameObject, Vector3.right, -90, 0.4f));
    }

    private void Update()
    {
        if (!currentDestination)
            return;
        if (agent.remainingDistance > 0.1f)
            return;
        AnimateBid();
        currentDestination.TryPlaceBid(playerManager.identity);
        currentDestination = null;
    }
}
