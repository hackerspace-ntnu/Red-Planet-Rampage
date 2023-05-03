using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class TownScoreboard : MonoBehaviour
{
    private MatchController matchController;
    private List<Player> players;

    public GameObject wantedPosterPrefab;
    private WantedPoster[] posters;

    [SerializeField]
    private Transform content;

    [SerializeField]
    private TMP_Text timerText;
    private float timeRemaining;
    private bool timerTrigger = false;

    [SerializeField]
    private List<string> wantedSubtitles = new List<string>();

    void Start()
    {
        matchController = MatchController.Singleton;
        players = matchController.Players;
        posters = new WantedPoster[players.Count];

        for (int i = 0; i < players.Count; i++)
        {
            posters[i] = Instantiate(wantedPosterPrefab, content).GetComponentInChildren<WantedPoster>();
            posters[i].SetupPoster(players[i], matchController, wantedSubtitles[Random.Range(0, wantedSubtitles.Count - 1)]);
            posters[i].gameObject.SetActive(false);
        }

        matchController.onRoundEnd += UpdateScoreboard;
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

        timerTrigger = true;
        timeRemaining = matchController.RoundEndDelay;
    }

    public void Update()
    {
        if(timerTrigger)
        {
            timerText.text = timeRemaining.ToString("F1");
            timeRemaining -= Time.deltaTime;
        }
    }
}
