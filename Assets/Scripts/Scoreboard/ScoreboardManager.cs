using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using CollectionExtensions;
using System;

internal struct Rewards
{
    public int savings;
    public int collected;
    public int baseReward;
    public int kills;
    public int killReward;
    public int winReward;
    public int total;
}

[RequireComponent(typeof(AudioSource))]
public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Singleton { get; private set; }

    // Actions
    public event Action ShowNextCrime;
    public event Action ShowVictoryProgress;

    [Header("Variables")]
    private List<PlayerManager> players = new();

    [SerializeField]
    public List<string> wantedSubtitles = new();

    [SerializeField]
    private AudioGroup nextCrimeSound;

    private AudioSource audioSource;

    [SerializeField]
    [Range(0f, 5f)]
    private float newCrimeDelay = 1f;

    [SerializeField]
    public float matchProgressDelay = 5f;

    private int step = 0;
    private int maxSteps = 0;

    private List<Scoreboard> scoreboards = new();

    // Refrences
    private MatchController matchController;


    private void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
                return;
            }
        }

        Singleton = this;

        #endregion Singleton boilerplate    
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

    }

    public void SetupPosters()
    {
        matchController = MatchController.Singleton;
        players = matchController.Players.OrderBy(p => p.id).ToList();

        scoreboards = GetComponentsInChildren<Scoreboard>().ToList();

        // Give each scoreboard a random subtitle, while removing unused scoreboards. 
        for (int i = 0; i < scoreboards.Count; i++)
        {
            if (i < players.Count)
            {
                scoreboards[i].SetupPoster(players[i], wantedSubtitles.RandomElement());
            }
            else
            {
                Destroy(scoreboards[i].gameObject);
            }
        }
        scoreboards.RemoveRange(players.Count, Mathf.Max(0, scoreboards.Count - players.Count));
        matchController.onRoundEnd += SetPosterValues;
    }

    public IEnumerator ShowMatchResults()
    {
        // Animate the after battle scene
        Camera.main.transform.parent = transform;
        Camera.main.GetComponent<Animator>().SetTrigger("ScoreboardZoom");

        for (int i = 0; i < scoreboards.Count; i++)
        {
            // Disable player camera
            if (players[i].inputManager)
                players[i].inputManager.PlayerCamera.enabled = false;
        }

        // Do not start adding crimes before the camera has finished the animation
        int delay = Mathf.RoundToInt(Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length);
        yield return new WaitForSeconds(delay);

        maxSteps = MaxNumberOfCrimes();

        StartCoroutine(NextCrime());
    }

    private int MaxNumberOfCrimes()
    {
        int maxCrimes = 0;
        foreach (var scoreboard in scoreboards)
        {
            if (scoreboard.TotalCrimes > maxCrimes)
                maxCrimes = scoreboard.TotalCrimes;
        }

        return maxCrimes;
    }

    private IEnumerator NextCrime()
    {
        if (step < maxSteps)
        {
            nextCrimeSound.Play(audioSource);
            ShowNextCrime?.Invoke();
            step++;
            yield return new WaitForSeconds(newCrimeDelay);
            StartCoroutine(NextCrime());
        }
        else
        {
            ShowVictoryProgress?.Invoke();
            // Start next round
            yield return new WaitForSeconds(matchProgressDelay);
            matchController.StartNextBidding();
        }
    }

    private Rewards AssignAndDetermineRewards(PlayerManager player)
    {
        var lastRound = matchController.LastRound;
        var playerDetails = Peer2PeerTransport.PlayerDetails.Where(p => p.id == player.id).SingleOrDefault();
        var result = new Rewards
        {
            savings = playerDetails.chips,
            collected = player.identity.Chips - playerDetails.chips,
            kills = lastRound.KillCount(player)
        };
        foreach (Reward reward in MatchRules.Current.Rewards)
        {
            switch (reward.Condition)
            {
                case RewardCondition.Survive:
                    if (reward.Type is RewardType.Chips)
                        result.baseReward += reward.Amount;
                    player.identity.AssignReward(reward);
                    break;
                case RewardCondition.Kill:
                    var calculatedReward = reward;
                    calculatedReward.Amount = reward.Amount * result.kills;
                    if (reward.Type is RewardType.Chips)
                        result.killReward += calculatedReward.Amount;
                    player.identity.AssignReward(calculatedReward);
                    break;
                case RewardCondition.Win:
                    if (lastRound.IsWinner(player.identity))
                    {
                        if (reward.Type is RewardType.Chips)
                            result.winReward += reward.Amount;
                        player.identity.AssignReward(reward);
                    }
                    break;
                default:
                    break;
            }
        }
        result.total = result.savings + result.collected + result.baseReward + result.killReward + result.winReward;
        return result;
    }

    public void SetPosterValues()
    {
        if (matchController == null)
            matchController = MatchController.Singleton;

        // Loop through each player, assign points as commented
        for (int i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var scoreboard = scoreboards[i];

            var rewards = AssignAndDetermineRewards(player);

            scoreboard.AddReward("Savings", rewards.savings);

            // Participation award
            scoreboard.AddReward("Base", rewards.baseReward);

            scoreboard.AddReward("Pickups", rewards.collected);

            // Kill Award, 0 if none
            scoreboard.AddReward("Kills", rewards.killReward);

            // Round winner award, blank if none
            if (rewards.winReward > 0)
            {
                scoreboard.AddReward("Victor", rewards.winReward);
            }
            else
            {
                scoreboard.AddBlankReward();
            }

            scoreboard.AddReward("Total", rewards.total);
        }

        StartCoroutine(ShowMatchResults());
    }
}
