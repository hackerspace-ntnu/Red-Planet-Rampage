using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class Scoreboard : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform content;

    private ScoreboardManager scoreboardManager;

    private Player player;

    private AudioSource audioSource;

    public event Action ShowNextCrime;

    public int TotalCrimes { get => crimeData.Count; }

    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text subtitle;

    public Image photo;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Transform crimeContent;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject crimeTextPrefab;


    private int currentStep;
    public int CurrentStep { get => currentStep; }

    private List<(string, string)> crimeData = new();
    private List<(TMP_Text, TMP_Text)> crimeTextComponents = new();

    private Scoreboard scoreboard;

    public void SetupPoster(Player player, string subtitle)
    {
        background.color = player.playerIdentity.color;
        this.subtitle.text = subtitle;
        scoreboard = GetComponentInParent<Scoreboard>();

        scoreboard.ShowNextCrime += NextStep;
    }

    public void AddPosterCrime(string crimeLabel, int Value)
    {
        GameObject crime = Instantiate(crimeTextPrefab, crimeContent);
        TMP_Text[] texts = crime.GetComponentsInChildren<TMP_Text>();

        crimeTextComponents.Add((texts[0], texts[1]));
        crimeData.Add((crimeLabel, Value.ToString()));
    }

    private void NextStep()
    {
        DisplayCrime(currentStep);
        currentStep++;
    }

    private void DisplayCrime(int index)
    {
        if (index <= crimeData.Count - 1)
        {
            crimeTextComponents[index].Item1.text = crimeData[index].Item1;
            crimeTextComponents[index].Item2.text = crimeData[index].Item2;
        }
    }
    void Start()
    {
        scoreboardManager = ScoreboardManager.Singleton;
        audioSource = GetComponent<AudioSource>();    
        
        
    }

    /*
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
    }*/
}
