using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;

[RequireComponent(typeof(AudioSource))]
public class Scoreboard : MonoBehaviour
{
    public static Scoreboard Singleton { get; private set; }

    [Header("Variables")]
    [SerializeField]
    private List<string> wantedSubtitles = new();

    [SerializeField]
    private float startBiddingDelay = 3;

    [SerializeField]
    private AudioClip nextCrimeSound;

    [Header("References")]
    [SerializeField]
    private Transform content;

    private MatchController matchController;

    private List<Player> players;

    private AudioSource audioSource;

    [SerializeField]
    private float displayCrimeDelay = 1;

    [Header("Prefabs")]
    public GameObject wantedPosterPrefab;
    private WantedPoster[] posters;

    public event Action ShowNextCrime;

    private int step = 0;
    private int maxSteps = 0;

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

    void Start()
    {
        matchController = MatchController.Singleton;
        audioSource = GetComponent<AudioSource>();        
    }

    public void CreateMatchResults()
    {
        // Setup posters
        InitiatePosters();
        // Populate posters with scores
        SetPosterValues();

        // Animate the after battle scene
        Camera.main.transform.parent = transform;
        Camera.main.GetComponent<Animator>().SetTrigger("ScoreboardZoom");

        for (int i = 0; i < posters.Length; i++)
        {
            // Disable player camera
            players[i].playerManager.inputManager.GetComponent<Camera>().enabled = false;

            // Enable posters
            posters[i].gameObject.SetActive(true);
        }
        
        // Do not start adding crimes before the camera has finished the animation
        int delay = Mathf.RoundToInt(Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length) * 1000;
        StartCoroutine(DelayDisplayCrimes(delay));

        maxSteps = MaxNumberOfCrimes();

        StartCoroutine(NextCrime());
        while (step <= maxSteps)
        {
            print("try this?");
        }

        print("Finished CreateMatchResults");
    }


    private IEnumerator DelayDisplayCrimes(int delay)
    {
        yield return new WaitForSeconds(delay);
    }

    private IEnumerator NextCrime()
    {
        if(step <= maxSteps)
        {
            yield return new WaitForSeconds(displayCrimeDelay);
            print("Invoke next crime");
            ShowNextCrime?.Invoke();
            step++;
            StartCoroutine(NextCrime());
        }
        else
        {
            yield break;
        }
        
    }

    private void PlayCrimeSound()
    {
        audioSource.PlayOneShot(nextCrimeSound);
    }

    public void SetPosterValues()
    {
        if (matchController == null)
            matchController = MatchController.Singleton;

        Round lastRound = matchController.GetLastRound;

        for (int i = 0; i < players.Count; i++)
        {
            Player player = players[i];
            WantedPoster poster = posters[i];

            poster.AddPosterCrime("Savings", player.playerIdentity.chips);

            // Participation award
            posters[i].AddPosterCrime("Base", matchController.RewardBase);

            // Kill Award
            if (lastRound.KillCount(players[i].playerManager) != 0)
            {
                poster.AddPosterCrime("Kills", matchController.RewardKill * lastRound.KillCount(player.playerManager));
            }

            // Round winner award
            if (lastRound.IsWinner(player.playerIdentity))
            {
                poster.AddPosterCrime("Victor", matchController.RewardWin);
            }

            // New total
            int roundSpoils = matchController.RewardBase
                + matchController.RewardKill * lastRound.KillCount(player.playerManager)
                + (lastRound.IsWinner(player.playerIdentity) ? matchController.RewardWin : 0);

            poster.AddPosterCrime("Total", roundSpoils);
        }
    }

    private void InitiatePosters (){
        if (posters != null)
        {
            foreach (var poster in posters)
                Destroy(poster.gameObject);
        }

        players = matchController.Players;
        posters = new WantedPoster[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            posters[i] = Instantiate(wantedPosterPrefab, content).GetComponentInChildren<WantedPoster>();
            posters[i].SetupPoster(players[i], wantedSubtitles[UnityEngine.Random.Range(0, wantedSubtitles.Count - 1)]);
            posters[i].gameObject.SetActive(false);
        }

        ShowNextCrime += PlayCrimeSound;
    }

    private int MaxNumberOfCrimes()
    {
        int maxCrimes = 0;
        foreach (var poster in posters)
        {
            if (poster.TotalCrimes > maxCrimes)
                maxCrimes = poster.TotalCrimes;
        }

        return maxCrimes;
    }
}
