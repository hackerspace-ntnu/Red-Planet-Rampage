using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

/// <summary>
/// Wrapper struct for tying refference to Player class with the in-game player.
/// </summary>
public struct Player
{
    public Player(PlayerIdentity playerIdentity, PlayerManager playerManager, int startAmount)
    {
        this.playerIdentity = playerIdentity;
        this.playerManager = playerManager;
    }
    // Reference to player identity class
    public PlayerIdentity playerIdentity;
    // Reference to in-match player
    public PlayerManager playerManager;
}

[RequireComponent(typeof(PlayerFactory))]
public class MatchController : MonoBehaviour
{
    public static MatchController Singleton { get; private set; }

    private PlayerFactory playerFactory;

    public delegate void MatchEvent();

    public MatchEvent onRoundEnd;
    public MatchEvent onRoundStart;
    public MatchEvent onBiddingStart;
    public MatchEvent onBiddingEnd;

    [Header("Timing")]
    [SerializeField]
    private float roundLength;

    [SerializeField]
    private float roundEndDelay;

    [Header("Chip rewards")]
    [SerializeField]
    private int startAmount = 5;
    [SerializeField]
    private int rewardWin = 1;
    [SerializeField]
    private int rewardKill = 1;
    [SerializeField]
    private int rewardBase = 2;

    public Timer roundTimer;

    [SerializeField]
    private GlobalHUDController globalHUDController;

    private List<Player> players = new List<Player>();
    private static List<Round> rounds = new List<Round>();

    void Start()
    {

        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate

        playerFactory = FindObjectOfType<PlayerFactory>();

        // Makes shooting end quickly if testing with 1 player
#if UNITY_EDITOR
        if (PlayerInputManagerController.Singleton.playerInputs.Count == 1)
            roundLength = 5f;
#endif

        StartNextRound();


    }

    public void StartNextRound()
    {
        // Setup of playerInputs
        playerFactory.InstantiatePlayersFPS();

        PlayerInputManagerController.Singleton.playerInputs.ForEach(playerInput =>
        {
            var playerIdentity = playerInput.GetComponent<PlayerIdentity>();
            var playerStateController = playerInput.transform.parent.GetComponent<PlayerManager>();
            players.Add(new Player(playerIdentity, playerStateController, startAmount));
        });

        // TODO do something else funky wunky

        MusicTrackManager.Singleton.SwitchTo(MusicType.BATTLE);
        onRoundStart?.Invoke();
        rounds.Add(new Round(players.Select(player => player.playerManager).ToList()));
        roundTimer.StartTimer(roundLength);
        roundTimer.OnTimerUpdate += HUDTimerUpdate;
        roundTimer.OnTimerRunCompleted += EndActiveRound;
    }

    public void StartNextBidding()
    {
        ChangeInputMappings("Bidding");
        MusicTrackManager.Singleton.SwitchTo(MusicType.BIDDING);
        onBiddingStart?.Invoke();
        // TODO: Add Destroy on match win
        SceneManager.LoadSceneAsync("Bidding");
        PlayerInputManagerController.Singleton.playerInputManager.splitScreen = false;
    }

    public void EndActiveRound()
    {
        onRoundEnd?.Invoke();
        roundTimer.OnTimerUpdate -= HUDTimerUpdate;
        roundTimer.OnTimerRunCompleted -= EndActiveRound;
        AssignRewards();

        if (!IsWin())
        {
            StartCoroutine(WaitAndStartNextBidding());
        }
    }

    public IEnumerator WaitAndStartNextBidding()
    {
        yield return new WaitForSeconds(roundEndDelay);
        StartNextBidding();
    }

    public IEnumerator WaitAndStartNextRound()
    {
        yield return new WaitForSeconds(roundEndDelay);
        // This needs to be called after inputs are set at start the first time this is needed.
        ChangeInputMappings("FPS");
        SceneManager.LoadScene("DemoArena");
        StartNextRound();
    }

    public void EndActiveBidding()
    {
        onBiddingEnd?.Invoke();

        StartNextRound();
    }

    private void AssignRewards()
    {
        var lastRound = rounds.Last();
        foreach (Player player in players)
        {
            // Base reward and kill bonus
            var reward = rewardBase + lastRound.KillCount(player.playerManager) * rewardKill;
            // Win bonus
            if (lastRound.IsWinner(player.playerManager.identity))
                reward += rewardWin;

            player.playerManager.identity.UpdateChip(reward);
        }
    }

    private void HUDTimerUpdate()
    {
        globalHUDController.OnTimerUpdate(roundLength - roundTimer.ElapsedTime);
    }

    private bool IsWin()
    {
        var lastWinner = rounds.Last().Winner;
        if (lastWinner == null) { return false; }
        var wins = rounds.Where(round => round.IsWinner(lastWinner)).Count();
        Debug.Log($"Current winner ({lastWinner}) has {wins} wins.");
        if (wins >= 3)
        {
            // We have a winner!
            // TODO Go to victory scene
            Debug.Log($"Aaaaand the winner iiiiiiiis {lastWinner}");

            // Update playerInputs in preperation for Menu scene
            ChangeInputMappings("Menu");

            MusicTrackManager.Singleton.SwitchTo(MusicType.MENU);
            SceneManager.LoadSceneAsync("Menu");
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ChangeInputMappings(string inputMapName)
    {
        PlayerInputManagerController.Singleton.ChangeInputMaps(inputMapName);
        foreach (InputManager inputs in PlayerInputManagerController.Singleton.playerInputs)
        {
            // Update listeners to new map
            inputs.RemoveListeners();
            inputs.AddListeners();
            // Free the playerInputs from their mortail coils (Player prefab)
            inputs.transform.parent = null;
            DontDestroyOnLoad(inputs);
        }
    }
}
