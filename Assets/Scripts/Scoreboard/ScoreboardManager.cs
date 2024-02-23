using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;
using CollectionExtensions;
using System;

[RequireComponent(typeof(AudioSource))]
public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Singleton { get; private set; }

    // Actions
    public event Action ShowNextCrime;
    public event Action ShowVictoryProgress;

    [Header("Variables")]
    private List<Player> players = new List<Player>();

    [SerializeField]
    public List<string> wantedSubtitles = new();

    [SerializeField]
    private AudioClip nextCrimeSound;

    private AudioSource audioSource;

    [SerializeField]
    [Range(0f, 5f)]
    private float newCrimeDelay = 1f;

    [SerializeField]
    public float matchProgressDelay = 5f;

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

        audioSource = GetComponent<AudioSource>();

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

    public IEnumerator ShowMatchResults()
    {
        // Animate the after battle scene
        Camera.main.transform.parent = transform;
        Camera.main.GetComponent<Animator>().SetTrigger("ScoreboardZoom");

        for (int i = 0; i < scoreboards.Count; i++)
        {
            // Disable player camera
            if (players[i].playerManager.inputManager)
                players[i].playerManager.inputManager.PlayerCamera.enabled = false;
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
            audioSource.PlayOneShot(nextCrimeSound);
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

            var baseReward = matchController.RewardBase;
            var killsReward = matchController.RewardKill * lastRound.KillCount(player.playerManager);
            var winReward = lastRound.IsWinner(player.playerIdentity) ? matchController.RewardWin : 0;

            var total = player.playerIdentity.chips;
            var gain = baseReward + killsReward + winReward;
            var savings = total - gain;

            scoreboard.AddPosterCrime("Savings", savings);

            // Participation award
            scoreboard.AddPosterCrime("Base", baseReward);

            // Kill Award (shows 0 if none)
            scoreboard.AddPosterCrime("Kills", killsReward);

            // Round winner award
            if (winReward > 0)
            {
                scoreboard.AddPosterCrime("Victor", winReward);
            }
            else
            {
                scoreboard.AddBlankPoster();
            }

            scoreboard.AddPosterCrime("Total", total);
        }

        StartCoroutine(ShowMatchResults());
    }
}
