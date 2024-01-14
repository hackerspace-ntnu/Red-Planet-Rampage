using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using CollectionExtensions;

/// <summary>
/// Wrapper struct for tying refference to Player class with the in-game player.
/// </summary>
[Serializable]
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

    [SerializeField]
    private float biddingEndDelay = 10;

    [SerializeField]
    private float matchEndDelay = 5;


    [Header("Chip rewards")]
    [SerializeField]
    private int startAmount = 5;
    public int StartAmount => startAmount;
    [SerializeField]
    private int rewardWin = 1;
    public int RewardWin => rewardWin;
    [SerializeField]
    private int rewardKill = 1;
    public int RewardKill => rewardKill;
    [SerializeField]
    private int rewardBase = 2;
    public int RewardBase => rewardBase;

    public Timer roundTimer;

    [SerializeField]
    private GlobalHUDController globalHUDController;
    public GlobalHUDController GlobalHUD => globalHUDController;

    private string currentMapName;

    private List<Player> players = new List<Player>();
    public List<Player> Players { get { return players; } }
    private List<CollectableChip> collectableChips;

    private static List<Round> rounds = new List<Round>();
    public Round GetLastRound { get { return rounds[rounds.Count - 1]; } }

    public int RoundCount { get => rounds.Count(); }

    private void Awake()
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
    }

    void Start()
    {
        if (rounds.Count == 0)
        {
            PlayerInputManagerController.Singleton.playerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().resetItems());
        }
        playerFactory = FindObjectOfType<PlayerFactory>();

        if (currentMapName == null)
            currentMapName = SceneManager.GetActiveScene().name;

        // Makes shooting end quickly if testing with 1 player
#if UNITY_EDITOR
        if (PlayerInputManagerController.Singleton.playerInputs.Count == 1)
            roundLength = 100f;
#endif

        StartNextRound();
    }

    public void StartNextRound()
    {
        collectableChips = FindObjectsOfType<CollectableChip>().ToList();
        // Setup of playerInputs
        playerFactory.InstantiatePlayersFPS();

        PlayerInputManagerController.Singleton.playerInputs.ForEach(playerInput =>
        {
            var playerIdentity = playerInput.GetComponent<PlayerIdentity>();
            var playerStateController = playerInput.transform.parent.GetComponent<PlayerManager>();
            players.Add(new Player(playerIdentity, playerStateController, startAmount));
        });
        if (PlayerInputManagerController.Singleton.playerInputs.Count < 4)
        {
            playerFactory.InstantiateAIOpponents(4 - PlayerInputManagerController.Singleton.playerInputs.Count)
                .ForEach(ai => players.Add(new Player(ai.identity, ai, startAmount)));
        }

        players.Where(player => player.playerManager is AIManager)
            .Select(player => player.playerManager)
            .Cast<AIManager>()
            .ToList().ForEach(ai => 
                ai.TrackedPlayers = players.Select(player => player.playerManager)
                .Where(player => player != ai).ToList());

        MusicTrackManager.Singleton.SwitchTo(MusicType.BATTLE);
        onRoundStart?.Invoke();
        rounds.Add(new Round(players.Select(player => player.playerManager).ToList()));
        roundTimer.StartTimer(roundLength);
        roundTimer.OnTimerUpdate += AdjustMusic;
        roundTimer.OnTimerUpdate += HUDTimerUpdate;
        roundTimer.OnTimerRunCompleted += EndActiveRound;
    }

    public void StartNextBidding()
    {
        if (IsWin())
            return;

        PlayerInputManagerController.Singleton.ChangeInputMaps("Bidding");
        MusicTrackManager.Singleton.SwitchTo(MusicType.BIDDING);
        onBiddingStart?.Invoke();
        // TODO: Add Destroy on match win
        SceneManager.LoadSceneAsync("Bidding");
        PlayerInputManagerController.Singleton.playerInputManager.splitScreen = false;
    }

    public void EndActiveRound()
    {
        onRoundEnd?.Invoke();
        roundTimer.OnTimerUpdate -= AdjustMusic;
        roundTimer.OnTimerUpdate -= HUDTimerUpdate;
        roundTimer.OnTimerRunCompleted -= EndActiveRound;
        AssignRewards();
    }

    public IEnumerator WaitAndStartNextBidding()
    {
        yield return new WaitForSeconds(roundEndDelay);
        StartNextBidding();
    }

    public IEnumerator WaitAndStartNextRound()
    {
        yield return new WaitForSeconds(biddingEndDelay);
        // This needs to be called after inputs are set at start the first time this is needed.
        PlayerInputManagerController.Singleton.ChangeInputMaps("FPS");
        SceneManager.LoadScene(currentMapName);
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

    private void AdjustMusic()
    {
        if (roundTimer.ElapsedTime > roundLength * .7f)
        {
            MusicTrackManager.Singleton.IntensifyBattleTheme();
        }
    }

    private void HUDTimerUpdate()
    {
        globalHUDController.OnTimerUpdate(roundLength - roundTimer.ElapsedTime);
    }

    private bool IsWin()
    {
        var winner = rounds.Last().Winner;
        if (winner == null) { return false; }
        var wins = rounds.Where(round => round.IsWinner(winner)).Count();
        Debug.Log($"Current winner ({winner}) has {wins} wins.");
        if (wins >= 3)
        {
            // We have a winner!
            StartCoroutine(DisplayWinScreenAndRestart(winner));
            return true;
        }
        else
        {
            return false;
        }
    }
    public void RemoveChip(CollectableChip chip)
    {
        collectableChips.Remove(chip);
    }
    public Transform? GetRandomActiveChip()
    {
        if (collectableChips.Count == 0)
            return null;
        return collectableChips.RandomElement().transform;
    }

    private IEnumerator DisplayWinScreenAndRestart(PlayerIdentity winner)
    {
        globalHUDController.DisplayWinScreen(winner);

        yield return new WaitForSecondsRealtime(matchEndDelay);

        ReturnToMainMenu();
    }

    public void ReturnToMainMenu()
    {
        // Update playerInputs in preperation for Menu scene
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");

        MusicTrackManager.Singleton.SwitchTo(MusicType.MENU);
        rounds = new List<Round>();
        PlayerInputManagerController.Singleton.playerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().resetItems());
        SceneManager.LoadSceneAsync("Menu");
    }
}
