using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using CollectionExtensions;

#nullable enable

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

    public MatchEvent onOutcomeDecided;
    public MatchEvent onRoundEnd;
    public MatchEvent onRoundStart;
    public MatchEvent onBiddingStart;
    public MatchEvent onBiddingEnd;

    [Header("Timing")]
    [SerializeField]
    private float roundLength;

    [SerializeField]
    private float delayBeforeRoundResults = 3f;

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
    public List<Player> Players => players;
    public IEnumerable<Player> AIPlayers => players.Where(p => p.playerManager is AIManager);
    public IEnumerable<Player> HumanPlayers => players.Where(p => p.playerManager is not AIManager);

    [SerializeField]
    private List<CollectableChip> collectableChips;

    private static List<Round> rounds = new List<Round>();
    public Round GetLastRound { get { return rounds[rounds.Count - 1]; } }

    public int RoundCount { get => rounds.Count(); }

    private bool isAuction = false;
    public bool IsAuction => isAuction;

    [SerializeField]
    private GameObject loadingScreen;

    private int loadingDuration = 6;

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
        GameObject mainLight = GameObject.FindGameObjectsWithTag("MainLight")[0];
        RenderSettings.skybox.SetVector("_SunDirection", mainLight.transform.forward);
        RenderSettings.skybox.SetFloat("_MaxGradientTreshold", 0.25f);
        StartNextRound();
    }

    public void StartNextRound()
    {
        if (collectableChips.Count == 0)
            collectableChips = FindObjectsOfType<CollectableChip>().ToList();
        // Setup of playerInputs
        var aiPlayerCount = PlayerInputManagerController.Singleton.MatchHasAI ?
            Mathf.Max(4 - PlayerInputManagerController.Singleton.playerInputs.Count, 0) : 0;
        playerFactory.InstantiatePlayersFPS(aiPlayerCount)
            .ForEach(player => players.Add(new Player(player.identity, player, startAmount)));

        var aiPLayers = players.Where(player => player.playerManager is AIManager)
            .Select(player => player.playerManager)
            .Cast<AIManager>()
            .ToList();

        aiPLayers.ForEach(ai =>
                ai.TrackedPlayers = players.Select(player => player.playerManager)
                    .Where(player => player != ai).ToList());

        MusicTrackManager.Singleton.SwitchTo(MusicType.Battle);
        onRoundStart?.Invoke();
        isAuction = false;
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
        collectableChips = new List<CollectableChip>();

        StartCoroutine(ShowLoadingScreenBeforeBidding());
        // TODO: Add Destroy on match win   
    }
    private IEnumerator ShowLoadingScreenBeforeBidding()
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(loadingDuration);

        PlayerInputManagerController.Singleton.ChangeInputMaps("Bidding");
        MusicTrackManager.Singleton.SwitchTo(MusicType.Bidding);
        onBiddingStart?.Invoke();
        if (!SteamManager.Singleton.ChangeScene("Bidding"))
            SceneManager.LoadSceneAsync("Bidding");
        PlayerInputManagerController.Singleton.PlayerInputManager.splitScreen = false;
        isAuction = true;
    }
    public void EndActiveRound()
    {
        onOutcomeDecided?.Invoke();
        roundTimer.OnTimerUpdate -= AdjustMusic;
        roundTimer.OnTimerUpdate -= HUDTimerUpdate;
        roundTimer.OnTimerRunCompleted -= EndActiveRound;
        GlobalHUD.RoundTimer.enabled = false;
        StartCoroutine(WaitAndShowResults());
    }

    private IEnumerator WaitAndShowResults()
    {
        // Delay first so we can see who killed who
        yield return new WaitForSeconds(delayBeforeRoundResults);
        AssignRewards();
        // Scoreboard subscribes here
        onRoundEnd?.Invoke();
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
        var wins = PlayerWins(winner);
        Debug.Log($"Current winner ({winner}) has {wins} wins.");
        if (wins >= 3)
        {
            // We have a winner!
            StartCoroutine(DisplayWinScreenAndRestart(winner));
            // Remember stats from this match.
            PersistentInfo.SavePersistentData();
            return true;
        }
        else
        {
            return false;
        }
    }

    public int PlayerWins(PlayerIdentity player)
    {
        return rounds.Where(round => round.IsWinner(player)).Count();
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
        // Update playerInputs / identities in preperation for Menu scene
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
        // Remove AI identities
        FindObjectsOfType<PlayerIdentity>()
            .Where(identity => identity.IsAI)
            .ToList().ForEach(aiIdentity => Destroy(aiIdentity));

        MusicTrackManager.Singleton.SwitchTo(MusicType.Menu);
        rounds = new List<Round>();
        PlayerInputManagerController.Singleton.playerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().resetItems());
        if (!SteamManager.Singleton.ChangeScene("Menu"))
            SceneManager.LoadSceneAsync("Menu");
    }
}
