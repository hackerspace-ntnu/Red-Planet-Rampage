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
    private List<PlayerManager> players = new();

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
        matchController = MatchController.Singleton;

        audioSource = GetComponent<AudioSource>();

        matchController.onRoundStart += SetupPosters;
    }

    private void SetupPosters()
    {
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
            var player = players[i];
            var scoreboard = scoreboards[i];

            var baseReward = matchController.RewardBase;
            var killsReward = matchController.RewardKill * lastRound.KillCount(player);
            var winReward = lastRound.IsWinner(player.identity) ? matchController.RewardWin : 0;

            var total = player.identity.chips;
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
