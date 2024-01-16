using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class BiddingAI : BiddingPlayer
{
    private NavMeshAgent agent;
    private List<(BiddingPlatform, int)> priorities; 
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        chipText.text = playerManager.identity.chips.ToString();
        GetComponent<PlayerIK>().RightHandIKTarget = signTarget;

        foreach (var platform in AuctionDriver.Singleton.BiddingPlatforms)
        {
            platform.onItemSet += EvaluateItem;
            platform.onBidPlaced += Evaluate;
        }
    }

    private void Evaluate(BiddingPlatform platform)
    {
        if (platform.LeadingBidder == playerManager.identity)
            return;
        if (platform.chips >= playerManager.identity.chips)
        {
            platform.onBidPlaced -= EvaluateItem;
        }

        priorities.Where(priority => priority.Item1 != platform)
            .Select(priority => priority.Item2);
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
        priorities.Add(platform, priority);
    }
}
