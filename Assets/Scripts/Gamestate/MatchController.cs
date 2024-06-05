using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;
using CollectionExtensions;
using Mirror;

#nullable enable

[RequireComponent(typeof(PlayerFactory))]
public class MatchController : NetworkBehaviour
{
    public static MatchController Singleton { get; private set; }

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

    private string? currentMapName;

    private Dictionary<uint, PlayerManager> playerById = new();

    private List<PlayerManager> players = new();
    public ReadOnlyCollection<PlayerManager> Players;
    public IEnumerable<PlayerManager> AIPlayers => players.Where(p => p is AIManager);
    public IEnumerable<PlayerManager> HumanPlayers => players.Where(p => p is not AIManager);

    [SerializeField]
    private List<CollectableChip> collectableChips;

    private static List<Round> rounds = new();
    public Round LastRound => rounds.LastOrDefault();

    public int RoundCount { get => rounds.Count(); }

    private bool isAuction = false;
    public bool IsAuction => isAuction;

    private bool isRoundInProgress = false;
    public bool IsRoundInProgress => isRoundInProgress;

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

        players = new List<PlayerManager>();
        Players = new ReadOnlyCollection<PlayerManager>(players);
    }

    private void Start()
    {
        if (rounds.Count == 0)
        {
            PlayerInputManagerController.Singleton.LocalPlayerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().ResetItems());
        }

        currentMapName ??= SceneManager.GetActiveScene().name;

        var mainLight = GameObject.FindGameObjectsWithTag("MainLight")[0];
        RenderSettings.skybox.SetVector("_SunDirection", mainLight.transform.forward);
        RenderSettings.skybox.SetFloat("_MaxGradientTreshold", 0.25f);

        GlobalHUD.RoundTimer.enabled = false;
        StartNextRound();
    }

    private void StartNextRound()
    {
        if (collectableChips.Count == 0)
            collectableChips = FindObjectsOfType<CollectableChip>().ToList();

        players = new();
        playerById = new();
        Players = new ReadOnlyCollection<PlayerManager>(players);

        StartCoroutine(WaitForClientsAndInitialize());
    }

    private void InitializeAIPlayers()
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
        players.Add(player);
        playerById.Add(player.id, player);
    }

    // TODO give players start amount worth of chips (on match start only)
    private void InitializeRound()
    {
        LoadingScreen.Singleton.Hide();
        InitializeAIPlayers();
        MusicTrackManager.Singleton.SwitchTo(MusicType.Battle);
        onRoundStart?.Invoke();
        isAuction = false;
        GlobalHUD.RoundTimer.enabled = true;
        rounds.Add(new Round(players.ToList()));
        roundTimer.StartTimer(roundLength);
        roundTimer.OnTimerUpdate += AdjustMusic;
        roundTimer.OnTimerUpdate += HUDTimerUpdate;
        if (isServer)
            roundTimer.OnTimerRunCompleted += EndActiveRound;
        isRoundInProgress = true;
        ScoreboardManager.Singleton.SetupPosters();
    }

    private IEnumerator WaitForClientsAndInitialize()
    {
        // TODO add a timeout thingy for when one player doesn't join in time
        // TODO keep loading screen open while this while loop spins
        // Spin while waiting for players to spawn
        while (players.Count < Peer2PeerTransport.NumPlayers)
        {
#if UNITY_EDITOR
            Debug.Log($"{players.Count} of {Peer2PeerTransport.NumPlayers} players spawned");
#endif
            yield return null;
        }

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
        LoadingScreen.Singleton.Show();
        yield return new WaitForSeconds(LoadingScreen.Singleton.MandatoryDuration);

        // TODO only switch to this track after auction has loaded!
        MusicTrackManager.Singleton.SwitchTo(MusicType.Bidding);
        onBiddingStart?.Invoke();
        PlayerInputManagerController.Singleton.PlayerInputManager.splitScreen = false;
        isAuction = true;
        NetworkManager.singleton.ServerChangeScene(Scenes.Bidding);
    }

    private void LateUpdate()
    {
        if (isServer && isRoundInProgress && LastRound.CheckWinCondition())
            EndActiveRound();
    }

    [Server]
    private void EndActiveRound()
    {
        roundTimer.OnTimerRunCompleted -= EndActiveRound;
        EndActiveRoundRpc(LastRound.SummarizeRound());
    }

    [ClientRpc]
    private void EndActiveRoundRpc(NetworkRound serverRound)
    {
        rounds[^1].UpdateFromSummary(serverRound);

        isRoundInProgress = false;
        onOutcomeDecided?.Invoke();
        roundTimer.StopTimer();
        roundTimer.OnTimerUpdate -= AdjustMusic;
        roundTimer.OnTimerUpdate -= HUDTimerUpdate;
        GlobalHUD.RoundTimer.enabled = false;
        AssignRewards();
        StartCoroutine(WaitAndShowResults());
    }

    private IEnumerator WaitAndShowResults()
    {
        // Delay first so we can see who killed who
        yield return new WaitForSeconds(delayBeforeRoundResults);
        // Scoreboard subscribes here
        onRoundEnd?.Invoke();
    }

    public IEnumerator WaitAndStartNextRound()
    {
        LoadingScreen.Singleton.Show();
        yield return new WaitForSeconds(LoadingScreen.Singleton.MandatoryDuration);
        NetworkManager.singleton.ServerChangeScene(currentMapName);
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

            // TODO alternatively, instead of avoiding to show chip count here,
            //      could we show chip count going up for each kill?
            player.identity.UpdateChipSilently(reward);
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
        var winnerId = rounds.Last().Winner;
        if (winnerId is null || !playerById.TryGetValue((uint)winnerId, out var winner)) { return false; }
        var wins = PlayerWins(winner);
        Debug.Log($"Current winner ({winner.identity.playerName}) has {wins} wins.");
        if (wins >= 3)
        {
            // We have a winner!
            StartCoroutine(DisplayWinScreenAndRestart(winner.identity));
            // Remember stats from this match.
            PersistentInfo.SavePersistentData();
            return true;
        }
        return false;
    }

    public int PlayerWins(PlayerManager player)
    {
        return rounds.Count(round => round.IsWinner(player.id));
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

    private void ReturnToMainMenu()
    {
        // Remove AI identities
        FindObjectsOfType<PlayerIdentity>()
            .Where(identity => identity.IsAI)
            .ToList().ForEach(aiIdentity => Destroy(aiIdentity));

        rounds = new List<Round>();
        MusicTrackManager.Singleton.SwitchTo(MusicType.Menu);
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");

        // Mirror pulls us to the main menu automatically
        NetworkManager.singleton.StopHost();
    }
}
