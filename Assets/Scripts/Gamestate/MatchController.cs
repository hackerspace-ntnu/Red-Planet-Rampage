using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

/// <summary>
/// Wrapper struct for tying refrence to Player class with the in-game player.
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

    public static bool operator==(Player lhs, Player rhs)
    {
        return lhs.playerIdentity == rhs.playerIdentity;
    }

    public static bool operator!=(Player lhs, Player rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals(obj);
    }

    public override int GetHashCode()
    {
        return playerIdentity.GetHashCode();
    }
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
    public float RoundEndDelay { get => roundEndDelay;}

    [SerializeField]
    private float biddingEndDelay = 10;

    [SerializeField]
    private float matchEndDelay = 5;


    [Header("Chip rewards")]
    [SerializeField]
    private int chipStartAmount;
    public int ChipStartAmount => chipStartAmount;

    [SerializeField]
    private int chipBaseReward;
    public int ChipBaseReward => chipBaseReward;

    [SerializeField]
    private int chipKillReward;
    public int ChipKillReward => chipKillReward;

    [SerializeField]
    private int chipWinReward;
    public int ChipWinReward => chipWinReward;

    public Timer roundTimer;

    [SerializeField]
    private GlobalHUDController globalHUDController;

    private List<Player> players = new List<Player>();
    public List<Player> Players 
    { 
        get { return players;} 
    }

    private static List<Round> rounds = new List<Round>();
    public Round GetLastRound() { return rounds.Last(); }

    public Dictionary<PlayerIdentity,int> GetSortedBounties()
    {
        Dictionary<PlayerIdentity, int> bounties = new();

        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].playerIdentity.bounty != 0)
                bounties.Add(players[i].playerIdentity, players[i].playerIdentity.bounty);
        }

        // Sort the dictionary from highest to lowest bounty:
        bounties = bounties.OrderByDescending(pair => pair.Value).ToDictionary(pair => pair.Key, pair => pair.Value);

        return bounties;
    }

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
            players.Add(new Player(playerIdentity, playerStateController, chipBaseReward));
        });

        MusicTrackManager.Singleton.SwitchTo(MusicType.BATTLE);
        rounds.Add(new Round(players.Select(player => player.playerManager).ToList()));
        roundTimer.StartTimer(roundLength);
        roundTimer.OnTimerUpdate += AdjustMusic;
        roundTimer.OnTimerUpdate += HUDTimerUpdate;
        roundTimer.OnTimerRunCompleted += EndActiveRound;
        
        StartCoroutine(WaitForNextFrame());
    }

    IEnumerator WaitForNextFrame()
    {
        yield return new WaitForEndOfFrame();
        onRoundStart?.Invoke();
    }

    public void StartNextBidding()
    {
        Debug.Log("Startnextbidding");
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

    public IEnumerator WaitAndStartNextRound()
    {
        yield return new WaitForSeconds(biddingEndDelay);
        // This needs to be called after inputs are set at start the first time this is needed.
        PlayerInputManagerController.Singleton.ChangeInputMaps("FPS");
        SceneManager.LoadScene("CraterTown");
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
            var reward = chipBaseReward + lastRound.KillCount(player.playerManager) * chipKillReward;
            
            // Win bonus
            if (lastRound.IsWinner(player.playerManager.identity)) {
                reward += chipWinReward;
            }

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

    private IEnumerator DisplayWinScreenAndRestart(PlayerIdentity winner)
    {
        globalHUDController.DisplayWinScreen(winner);

        yield return new WaitForSecondsRealtime(matchEndDelay);

        // Update playerInputs in preperation for Menu scene
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");

        MusicTrackManager.Singleton.SwitchTo(MusicType.MENU);
        rounds = new List<Round>();
        PlayerInputManagerController.Singleton.playerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().resetItems());
        SceneManager.LoadSceneAsync("Menu");
    }
}
