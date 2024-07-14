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

    private int platformsEvaluated = 0;
    private bool shouldEvaluate = true;

    private int budget;
    private int availableChips;

    private PlayerIdentity identity;

    private void Start()
    {
        GetComponent<PlayerIK>().RightHandIKTarget = signTarget;
        agent.updateRotation = false;
        foreach (var platform in AuctionDriver.Singleton.BiddingPlatforms)
        {
            platform.onItemSet += EvaluateItem;
        }
        playerManager.onSelectedBiddingPlatformChange += OnBiddingPlatformChange;
        playerManager.onSelectedBiddingPlatformChange += AnimateChipStatus;
        playerManager.onSelectedBiddingPlatformChange += ToggleDither;
        InstantiateMaterials();
    }

    public void SetIdentity(PlayerIdentity identity)
    {
        this.identity = identity;
        chipText.text = "<sprite name=\"chip\">" + identity.Chips.ToString();
        identity.onChipChange += UpdateChipStatus;
        chipText.color = playerManager.identity.HasMaxChips ? Color.red : Color.black;
        availableChips = identity.Chips;
        budget = DetermineBudget(availableChips);
    }

    private IEnumerator WaitAndEvaluate()
    {
        foreach (BiddingPlatform platform in AuctionDriver.Singleton.BiddingPlatforms)
            EvaluatePlatformStates(platform);
        ChooseDestination();
        yield return new WaitForSeconds(2);
        StartCoroutine(WaitAndEvaluate());
    }

    private void EvaluatePlatformStates(BiddingPlatform platform)
    {
        var isAlreadyInTheLead = platform.IsActive && platform.LeadingBidder == playerManager.id;
        if (isAlreadyInTheLead)
            return;

        var isNotActive = !platform.IsActive;
        var isTooExpensive = platform.chips >= playerManager.identity.Chips;
        if (isNotActive || isTooExpensive || !IsWithinBudget(platform.chips))
            priorities[platform] = -1;
    }

    private void ChooseDestination()
    {
        currentDestination = priorities.ToList()
            .OrderByDescending(x => x.Value)
            .Select(x => x.Key).First();

        if (!currentDestination)
            return;

        // AI should yield
        var spentChips = availableChips - identity.Chips;
        var isOverBudget = spentChips >= budget;
        if (priorities[currentDestination] == -1 || isOverBudget)
        {
            agent.SetDestination(AuctionDriver.Singleton.YieldPosition);
            return;
        }

        var isAlreadyInTheLead = currentDestination.LeadingBidder == playerManager.id;
        if (isAlreadyInTheLead)
            return;

        agent.SetDestination(currentDestination.transform.position + platformDestinationOffset);

        var isAlreadyAtThisPlatform = currentDestination == playerManager.SelectedBiddingPlatform;
        if (isAlreadyAtThisPlatform)
            OnBiddingPlatformChange(currentDestination);
    }

    private void EvaluateItem(BiddingPlatform platform)
    {
        int priority = 0;
        switch (platform.Item.augmentType)
        {
            case AugmentType.Body:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Body == platform.Item.id)
                    .Sum((augment) => augment.KillCount);
                break;
            case AugmentType.Barrel:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Barrel == platform.Item.id)
                    .Sum((augment) => augment.KillCount);
                break;
            case AugmentType.Extension:
                priority = PersistentInfo.CombinationStats.Where(stat => stat.Extension == platform.Item.id)
                    .Sum((augment) => augment.KillCount);
                break;
        }
        // (re)set the priority (reassignment in case we have multiple bidding rounds)
        priorities[platform] = priority;

        platformsEvaluated++;
        if (platformsEvaluated < 3 || !shouldEvaluate)
            return;
        StartCoroutine(WaitAndEvaluate());
        shouldEvaluate = false;
    }

    private int DetermineBudget(int availableChips)
    {
        // Irrelevant for non-chip gamemodes
        var isFirstToXChips = MatchRules.Current.MatchWinCondition is { WinCondition: MatchWinConditionType.Chips, StopCondition: MatchStopConditionType.FirstToX };
        if (!isFirstToXChips)
            return availableChips;

        var numBodiesBought = identity.Bodies.Count - 1;
        var numBarrelsBought = identity.Barrels.Count - 1;
        var numExtensionsBought = identity.Extensions.Count;
        var numPartsOfCompleteSet = Mathf.Min(1, numBodiesBought) + Mathf.Min(1, numBarrelsBought) + Mathf.Min(1, numExtensionsBought);

        var budgetFactor = 1f;
        if (numPartsOfCompleteSet > 3)
            budgetFactor = .15f;
        else if (numPartsOfCompleteSet > 2)
            budgetFactor = .2f;
        else if (numPartsOfCompleteSet > 1)
            budgetFactor = .75f;

        var budget = Mathf.FloorToInt(budgetFactor * availableChips);
        Debug.Log($"{identity.ToColorString()} has {numPartsOfCompleteSet} parts and will spend {budget}/{availableChips}");
        return budget;

    }

    private bool IsWithinBudget(int currentBid)
    {
        var spentChips = availableChips - identity.Chips;
        var spentChipsIfBought = currentBid + 1 + spentChips;
        return spentChipsIfBought <= budget;
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
        if (!platform || !currentDestination || platform != currentDestination || platform.LeadingBidder == playerManager.id || platform != playerManager.SelectedBiddingPlatform)
            return;

        if (!IsWithinBudget(platform.chips))
            return;

        AnimateBid();
        currentDestination.PlaceBid(playerManager.identity);
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
