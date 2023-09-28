using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Linq.Expressions;
using System.Linq;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class Scoreboard : MonoBehaviour
{
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

    void Start()
    {
        matchController = MatchController.Singleton;
        audioSource = GetComponent<AudioSource>();        

        matchController.onRoundStart += CreateMostWanted;
        matchController.onRoundEnd += CreateMatchResults;
    }

    private void OnDestroy()
    {

    }

    private void CreateMostWanted()
    {
        // Initiate posters
        InitiatePosters();
        
        // Check for current score amongst players
        Dictionary<PlayerIdentity, int> bounties = matchController.GetSortedBounties();

        // Display bounties!
        for (int i = 0; i < bounties.Count; i++)
        {
            posters[i].SetupWantedPoster(bounties.ElementAt(i).Key);
        }
    }

    private async void CreateMatchResults()
    {
        InitiatePosters();

        // Order the relevant poster to update
        for (int i = 0; i < posters.Length; i++)
        {
            posters[i].UpdatePosterValues();
        }
        // Assign main camera
        Camera.main.transform.parent = transform;

        // Animate the after battle scene
        Camera.main.GetComponent<Animator>().SetTrigger("ScoreboardZoom");

        for (int i = 0; i < posters.Length; i++)
        {
            // Disable player camera
            players[i].playerManager.inputManager.GetComponent<Camera>().enabled = false;

            // Enable posters
            posters[i].gameObject.SetActive(true);

            posters[i].UpdatePosterValues();
        }

        float delay = Camera.main.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
        await DelayDisplayCrimes(delay);
    }

    public void TryToStartBidding()
    {
        // Check that all posters have finished displaying
        foreach (WantedPoster poster in posters)
        {
            if (poster.CurrentStep < poster.TotalSteps)
                return;
        }

        StartCoroutine(StartBiddingCountDown());
    }

    private IEnumerator StartBiddingCountDown()
    {
        yield return new WaitForSeconds(startBiddingDelay);
        
    }

    private IEnumerator DelayDisplayCrimes(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(NextCrime());
    }
    private IEnumerator NextCrime()
    {
        yield return new WaitForSeconds(displayCrimeDelay);
        ShowNextCrime?.Invoke();
        StartCoroutine(NextCrime());
    }

    private void PlayCrimeSound()
    {
        audioSource.PlayOneShot(nextCrimeSound);
    }

    private void InitiatePosters (){
        if (posters != null)
        {
            foreach (var poster in posters)
                Destroy(poster.gameObject);
        }

        players = PlayerInputManagerController.Singleton.playerInputs;
        posters = new WantedPoster[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            posters[i] = Instantiate(wantedPosterPrefab, content).GetComponentInChildren<WantedPoster>();
            posters[i].SetupPoster(players[i], wantedSubtitles[UnityEngine.Random.Range(0, wantedSubtitles.Count - 1)]);
            posters[i].gameObject.SetActive(false);
        }

        ShowNextCrime += PlayCrimeSound;
    }
}
