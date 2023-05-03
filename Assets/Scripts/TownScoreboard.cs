using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TownScoreboard : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField]
    private List<string> wantedSubtitles = new List<string>();

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
        players = matchController.Players;
        posters = new WantedPoster[players.Count];
        audioSource = GetComponent<AudioSource>();

        for (int i = 0; i < players.Count; i++)
        {
            posters[i] = Instantiate(wantedPosterPrefab, content).GetComponentInChildren<WantedPoster>();
            posters[i].SetupPoster(players[i], matchController, wantedSubtitles[UnityEngine.Random.Range(0, wantedSubtitles.Count - 1)]);
            posters[i].gameObject.SetActive(false);
        }

        matchController.onRoundEnd += UpdateScoreboard;
        ShowNextCrime += PlayCrimeSound;
    }

    public void UpdateScoreboard()
    {
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
        StartCoroutine(DelayDisplayCrimes(delay));
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
        matchController.StartNextBidding();
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
}
