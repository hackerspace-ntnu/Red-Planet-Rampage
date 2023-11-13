using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Singleton { get; private set; }
    
    // Events
    public delegate void scoreboardEvent();
    public scoreboardEvent updateMostWanted;
    public scoreboardEvent updateMatchResults;

    [Header("Variables")]
    private List<Player> players;

    [SerializeField]
    public List<string> wantedSubtitles = new();

    [SerializeField]
    private AudioClip nextCrimeSound;

    [SerializeField]
    [Range(0f, 5f)]
    private float newCrimeDelay = 3f;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject scoreboard;

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
        players = matchController.Players;

        for (int i = 0; i < players.Count; i++)
        {
            Scoreboard board = Instantiate(scoreboard, new Vector3(-i, 0, 0), Quaternion.identity).GetComponent<Scoreboard>();
            board.SetupPoster(players[i], wantedSubtitles[Random.Range(0, wantedSubtitles.Count-1)]);
        }
    }

    public void CreateMatchResults()
    {
        // Setup posters
        InitiatePosters();
        /*// Populate posters with scores
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

        print("Finished CreateMatchResults");*/
    }

    private void InitiatePosters()
    {

    }

}
