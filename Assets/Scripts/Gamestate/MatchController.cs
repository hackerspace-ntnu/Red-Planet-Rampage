using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using CollectionExtensions;
using Mirror;

#nullable enable

[RequireComponent(typeof(PlayerFactory))]
public class MatchController : MonoBehaviour
{
    public static MatchController Singleton { get; private set; }

    private PlayerFactory playerFactory;

    public delegate void MatchEvent();

    public MatchEvent? onOutcomeDecided;
    public MatchEvent? onRoundEnd;
    public MatchEvent? onRoundStart;
    public MatchEvent? onBiddingStart;
    public MatchEvent? onBiddingEnd;

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

    public Dictionary<uint, PlayerManager> PlayerById = new();
    private List<PlayerManager> players = new();
    public List<PlayerManager> Players => players;
    public IEnumerable<PlayerManager> AIPlayers => players.Where(p => p is AIManager);
    public IEnumerable<PlayerManager> HumanPlayers => players.Where(p => p is not AIManager);

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
            PlayerInputManagerController.Singleton.LocalPlayerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().ResetItems());
        }
        playerFactory = FindObjectOfType<PlayerFactory>();

        if (currentMapName == null)
            currentMapName = SceneManager.GetActiveScene().name;

        // Makes shooting end quickly if testing with 1 player
#if UNITY_EDITOR
        if (PlayerInputManagerController.Singleton.LocalPlayerInputs.Count == 1)
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

        players = new();
        PlayerById = new();

        if (NetworkManager.singleton.isNetworkActive)
        {
            StartCoroutine(WaitForClientsAndInitialize());
            return;
        }

        InitializePlayers();

        InitializeRound();
    }

    private void InitializePlayers()
    {
        // Setup of playerInputs
        // TODO replace this so that we have ai players :)
        var aiPlayerCount = PlayerInputManagerController.Singleton.MatchHasAI ?
            Mathf.Max(4 - PlayerInputManagerController.Singleton.PlayerCount, 0) : 0;
        playerFactory.InstantiatePlayersFPS(aiPlayerCount)
            .ForEach(player => players.Add(player));

        UpdateAIPlayers();

        PlayerById = new();

        foreach (var player in players)
        {
            PlayerById.Add(player.id, player);
        }
    }

    private void UpdateAIPlayers()
    {
        var aiPLayers = players.Where(player => player is AIManager)
            .Cast<AIManager>()
            .ToList();

        aiPLayers.ForEach(ai =>
                ai.TrackedPlayers = players
                    .Where(player => player != ai).ToList());
    }

    public void RegisterPlayer(PlayerManager player)
    {
        // TODO check if we've got all players, initialize match if so!
        players.Add(player);
        PlayerById.Add(player.id, player);
    }

    // TODO only do this after all players are initialized!
    // TODO give players start amount worth of chips (on match start only)
    private void InitializeRound()
    {
        UpdateAIPlayers();
        MusicTrackManager.Singleton.SwitchTo(MusicType.Battle);
        onRoundStart?.Invoke();
        isAuction = false;
        rounds.Add(new Round(players.ToList()));
        roundTimer.StartTimer(roundLength);
        roundTimer.OnTimerUpdate += AdjustMusic;
        roundTimer.OnTimerUpdate += HUDTimerUpdate;
        roundTimer.OnTimerRunCompleted += EndActiveRound;
    }

    private IEnumerator WaitForClientsAndInitialize()
    {
        PlayerInputManagerController.Singleton.ChangeInputMaps("FPS");
        // TODO add a timeout thingy for when one player doesn't join in time
        // Spin while waiting for players to 
        while (NetworkManager.singleton.numPlayers == 0 || players.Count != Peer2PeerTransport.NumPlayersInMatch) yield return null;

        InitializeRound();
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
        if (!SteamManager.Singleton.ChangeScene(currentMapName))
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
        foreach (var player in players)
        {
            // Base reward and kill bonus
            var reward = rewardBase + lastRound.KillCount(player) * rewardKill;
            // Win bonus
            if (lastRound.IsWinner(player.identity))
                reward += rewardWin;

            player.identity.UpdateChip(reward);
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
        PlayerInputManagerController.Singleton.LocalPlayerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().ResetItems());
        if (!SteamManager.Singleton.ChangeScene("Menu"))
            SceneManager.LoadSceneAsync("Menu");
    }
}
