using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using CollectionExtensions;
using System;

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Singleton { get; private set; }

    // Actions
    public event Action ShowNextCrime;

    [Header("Variables")]
    private List<Player> players = new List<Player>();

    [SerializeField]
    public List<string> wantedSubtitles = new();

    [SerializeField]
    private AudioClip nextCrimeSound;

    [SerializeField]
    [Range(0f, 5f)]
    private float newCrimeDelay = 3f;

    private int step = 0;
    private int maxSteps = 0;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject scoreboard;

    private List<Scoreboard> scoreboards = new List<Scoreboard>();

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
        matchController = MatchController.Singleton;

        // Values in matchcontroller are not set in Start(), thus using a coroutine to wait until values are set.
        StartCoroutine(SetupPosters());
    }

    private IEnumerator SetupPosters()
    {
        yield return new WaitForEndOfFrame();

        players = matchController.Players;

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
        scoreboards.RemoveRange(players.Count, scoreboards.Count - players.Count);
        matchController.onRoundEnd += SetPosterValues;
    }

    public void ShowMatchResults()
    {
        // Animate the after battle scene
        Camera.main.transform.parent = transform;
        Camera.main.GetComponent<Animator>().SetTrigger("ScoreboardZoom");

        for (int i = 0; i < scoreboards.Count; i++)
        {
            // Disable player camera
            players[i].playerManager.inputManager.GetComponent<Camera>().enabled = false;
        }

        // Do not start adding crimes before the camera has finished the animation
        int delay = Mathf.RoundToInt(Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length) * 1000;
        StartCoroutine(DelayDisplayCrimes(delay));

        maxSteps = MaxNumberOfCrimes();

        StartCoroutine(NextCrime());
    }

    private IEnumerator DelayDisplayCrimes(int delay)
    {
        yield return new WaitForSeconds(delay);
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
        if (step <= maxSteps)
        {
            yield return new WaitForSeconds(newCrimeDelay);
            ShowNextCrime?.Invoke();
            step++;
            StartCoroutine(NextCrime());
        }
        else
        {
            // Start next round
            matchController.StartNextBidding();
            yield break;
        }
    }

    public void SetPosterValues()
    {
        if (matchController == null)
            matchController = MatchController.Singleton;

        Round lastRound = matchController.GetLastRound;

        // Loop through each player, assign points as commented
        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            Scoreboard scoreboard = scoreboards[i];

            scoreboard.AddPosterCrime("Savings", player.playerIdentity.chips);

            // Participation award
            scoreboard.AddPosterCrime("Base", matchController.RewardBase);

            // Kill Award
            if (lastRound.KillCount(players[i].playerManager) != 0)
            {
                scoreboard.AddPosterCrime("Kills", matchController.RewardKill * lastRound.KillCount(player.playerManager));
            }

            // Round winner award
            if (lastRound.IsWinner(player.playerIdentity))
            {
                scoreboard.AddPosterCrime("Victor", matchController.RewardWin);
            }

            // New total
            int roundSpoils = matchController.RewardBase
                + matchController.RewardKill * lastRound.KillCount(player.playerManager)
                + (lastRound.IsWinner(player.playerIdentity) ? matchController.RewardWin : 0);

            scoreboard.AddPosterCrime("Total", roundSpoils);
        }

        ShowMatchResults();
    }

    private void InitiatePosters()
    {

    }

}
