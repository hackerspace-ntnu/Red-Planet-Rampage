using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using System.Collections.ObjectModel;

public enum SecretCombinationType
{
    None,
    DiscoInferno,
    WireFraud,
    SchizoidMan,
    BlueHat,
    BlackHat,
    EagleEyed,
    PogoStick,
    Skater,
    SprayNPray,
    PingPonginator,
    Flamethrower,
    OrbitalTrashCannon,
    ItalianPlumber,
    HatTrick,
    LongShot,
    RemoteWorker,
    BigIron,
    Nuke,
    MadHatter,
}

public enum AchievementType
{
    None,
    WinARound,
    Win30Chips,
    WinSixRounds,
    WinThreeStrikes,
    SitThroughCredits,
    AllIn,
    Clutch,
    MatchPoint,
}

public class Lobby
{
    public Lobby(CSteamID id)
    {
        this.id = id;
        UpdateMetadata();
    }

    public void UpdateMetadata()
    {
        host = SteamMatchmaking.GetLobbyOwner(id);
        name = SteamMatchmaking.GetLobbyData(id, NameProperty);

        availableSlots = 0;
        if (int.TryParse(SteamMatchmaking.GetLobbyData(id, AvailableSlotsProperty), out var slots))
            availableSlots = slots;
        if (int.TryParse(SteamMatchmaking.GetLobbyData(id, GameModeProperty), out var type))
            gameMode = (GameModeVariant)type;
    }

    public static string NameProperty = "name";
    public static string AvailableSlotsProperty = "availableSlots";
    public static string GameModeProperty = "gameMode";
    public static string GameVersionProperty = "version";

    public CSteamID id;
    public CSteamID host;

    public string name;
    public int availableSlots;
    public GameModeVariant gameMode;
}

public class SteamManager : MonoBehaviour
{
    private const int steamAppID = 2717710;
    public static SteamManager Singleton;
    public int ConnectedPlayers => NetworkManager.singleton.numPlayers;

    private static bool isSteamInitialized;
    public static bool IsSteamActive => isSteamInitialized;
    public bool IsHosting { get; private set; } = false;
    public bool IsInLobby { get; private set; } = false;

    public string UserName;
    public CSteamID SteamID;

    public delegate void LobbyEvent();
    public LobbyEvent LobbyPlayerUpdate;
    public LobbyEvent LobbyListUpdate;

    private CSteamID lobbyID;
    public Dictionary<ulong, string> PlayerNameById = new();
    public List<ulong> PlayerIDs = new();

    private List<Lobby> lobbies = new();
    public ReadOnlyCollection<Lobby> Lobbies;
    public Dictionary<ulong, Lobby> lobbiesById = new();

    private bool shouldStoreStats = false;

    // Lobby events
    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<LobbyEnter_t> lobbyEnter;
    private Callback<GameLobbyJoinRequested_t> joinRequest;
    private Callback<LobbyChatUpdate_t> lobbyUpdate;
    // Lobby finding
    private Callback<LobbyMatchList_t> lobbyListRequest;
    private Callback<LobbyDataUpdate_t> lobbyDataRequest;

    private const string hostkey = "HostAddress";

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
        DontDestroyOnLoad(this);
        try
        {
            //SteamAPI.RestartAppIfNecessary((AppId_t)steamAppID);
            isSteamInitialized = SteamAPI.Init();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private void Start()
    {
        if (!isSteamInitialized)
            return;

        Lobbies = new(lobbies);

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnlobbyCreated);
        lobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEnter);
        joinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        lobbyUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyUpdate);
        lobbyListRequest = Callback<LobbyMatchList_t>.Create(OnLobbiesFetched);
        lobbyDataRequest = Callback<LobbyDataUpdate_t>.Create(OnLobbyInfo);

        RequestStats();
        UserName = SteamFriends.GetPersonaName();
        SteamID = SteamUser.GetSteamID();
    }

    private void RequestStats()
    {
        if (!isSteamInitialized)
            return;

        SteamUserStats.RequestCurrentStats();
    }

    #region Achievements

    private readonly Dictionary<SecretCombinationType, string> combinationAchievementNames = new()
    {
        { SecretCombinationType.DiscoInferno, "WEAPON_DISCO_INFERNO" },
        { SecretCombinationType.WireFraud, "WEAPON_WIRE_FRAUD" },
        { SecretCombinationType.SchizoidMan, "WEAPON_SCHIZOID_MAN" },
        { SecretCombinationType.BlueHat, "WEAPON_BLUE_HAT" },
        { SecretCombinationType.BlackHat, "WEAPON_BLACK_HAT" },
        { SecretCombinationType.EagleEyed, "WEAPON_EAGLE_EYED" },
        { SecretCombinationType.PogoStick, "WEAPON_POGO_STICK" },
        { SecretCombinationType.Skater, "WEAPON_SKATER" },
        { SecretCombinationType.SprayNPray, "WEAPON_SPRAY_N_PRAY" },
        { SecretCombinationType.PingPonginator, "WEAPON_PING_PONG" },
        { SecretCombinationType.Flamethrower, "WEAPON_FLAMETHROWER" },
        { SecretCombinationType.OrbitalTrashCannon, "WEAPON_ORBITAL_TRASH_CANNON" },
        { SecretCombinationType.ItalianPlumber, "WEAPON_ITALIAN_PLUMBER" },
        { SecretCombinationType.HatTrick, "WEAPON_HAT_TRICK" },
        { SecretCombinationType.LongShot, "WEAPON_LONG_SHOT" },
        { SecretCombinationType.RemoteWorker, "WEAPON_REMOTE_WORKER" },
        { SecretCombinationType.BigIron, "WEAPON_BIG_IRON" },
        { SecretCombinationType.Nuke, "WEAPON_NUKE" },
        { SecretCombinationType.MadHatter, "WEAPON_MAD_HATTER" },
    };

    private readonly Dictionary<AchievementType, string> achievementNames = new()
    {
        { AchievementType.WinARound, "WIN_A_ROUND" },
        { AchievementType.Win30Chips, "WIN_30_CHIPS" },
        { AchievementType.WinSixRounds, "WIN_SIX_ROUNDS" },
        { AchievementType.WinThreeStrikes, "WIN_THREE_STRIKES" },
        { AchievementType.AllIn, "ACHIEVEMENT_ALL_IN" },
        { AchievementType.SitThroughCredits, "ACHIEVEMENT_CREDITS" },
        { AchievementType.Clutch, "ACHIEVEMENT_CLUTCH" },
        { AchievementType.MatchPoint, "ACHIEVEMENT_MATCH_POINT" },
    };

    public void UnlockAchievement(SecretCombinationType type)
    {
        if (type is SecretCombinationType.None || !combinationAchievementNames.TryGetValue(type, out var name))
            return;

        UnlockAchievement(name);
    }

    public void UnlockAchievement(AchievementType type)
    {
        if (type is AchievementType.None || !achievementNames.TryGetValue(type, out var name))
            return;

#if UNITY_EDITOR
        Debug.Log($"Attempting to unlock achievement {name}");
#endif
        UnlockAchievement(name);
    }

    private void UnlockAchievement(string name)
    {
        if (!isSteamInitialized)
            return;

        SteamUserStats.GetAchievement(name, out var isAlreadyUnlocked);

        if (isAlreadyUnlocked)
            return;

#if UNITY_EDITOR
        Debug.Log($"Unlocked achievement {name}");
#endif

        SteamUserStats.SetAchievement(name);
        shouldStoreStats = true;
    }

    #endregion Achievements

    #region Lobby

    private void OnlobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;
        NetworkManager.singleton.StartHost();
        IsHosting = true;

        lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyID, hostkey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, Lobby.NameProperty, UserName);
        SteamMatchmaking.SetLobbyData(lobbyID, Lobby.AvailableSlotsProperty, Peer2PeerTransport.NumAvailableSlots.ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, Lobby.GameModeProperty, ((int)MatchRules.Singleton.Rules.GameMode).ToString());
        SteamMatchmaking.SetLobbyData(lobbyID, Lobby.GameVersionProperty, Application.version);
        // Update filterable information when necessary
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerReceived += UpdateAvailableSlots;
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRemoved += UpdateAvailableSlots;
        ((Peer2PeerTransport)NetworkManager.singleton).OnMatchStart += SetAsNotJoinable;
        ((Peer2PeerTransport)NetworkManager.singleton).OnMatchEnd += SetAsJoinable;
    }

    private void UpdateAvailableSlots(PlayerDetails player)
    {
        SteamMatchmaking.SetLobbyData(lobbyID, Lobby.AvailableSlotsProperty, Peer2PeerTransport.NumAvailableSlots.ToString());
    }

    // TODO We wanna do this differently for rejoining disconnected player
    private void SetAsNotJoinable()
    {
        SteamMatchmaking.SetLobbyJoinable(lobbyID, false);
    }

    private void SetAsJoinable()
    {
        SteamMatchmaking.SetLobbyJoinable(lobbyID, true);
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        if (Peer2PeerTransport.NumPlayers >= Peer2PeerTransport.MaxPlayers || Peer2PeerTransport.IsInMatch)
            NetworkManager.singleton.StopClient();
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void UpdateLobbyData(ulong rawLobbyID)
    {
        Debug.Log("Lobby entered");
        lobbyID = new CSteamID(rawLobbyID);
        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobbyID); i++)
        {
            var id = SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i);
            var name = SteamFriends.GetFriendPersonaName(id);

            if (PlayerIDs.Contains(id.m_SteamID))
                continue;

            Debug.Log($"Steam user {name} (id={id.m_SteamID}) entered lobby");

            PlayerNameById[id.m_SteamID] = name;
            PlayerIDs.Add(id.m_SteamID);
        }
        LobbyPlayerUpdate?.Invoke();
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            // Stop networking and return to main menu if rejected
            NetworkManager.singleton.StopHost();
            return;
        }

        // All users
        IsInLobby = true;
        UpdateLobbyData(callback.m_ulSteamIDLobby);
        if (NetworkServer.active)
            return;
        // Only clients from here!
        ((Peer2PeerTransport)NetworkManager.singleton).JoinLobby(SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostkey));
    }

    private void OnLobbyUpdate(LobbyChatUpdate_t callback)
    {
        Debug.Log("Lobby updated");
        UpdateLobbyData(callback.m_ulSteamIDLobby);
    }

    public void HostLobby(ELobbyType type = ELobbyType.k_ELobbyTypePublic)
    {
        if (!isSteamInitialized)
            return;

        // TODO support public and friend lobbies
        SteamMatchmaking.CreateLobby(type, NetworkManager.singleton.maxConnections);
        // TODO use this for something?
        SteamMatchmaking.SetLobbyMemberLimit(lobbyID, 4);
    }

    public void LeaveLobby()
    {
        if (!isSteamInitialized)
            return;

        if (IsHosting)
        {
            ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerReceived -= UpdateAvailableSlots;
            ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRemoved -= UpdateAvailableSlots;
            ((Peer2PeerTransport)NetworkManager.singleton).OnMatchStart -= SetAsNotJoinable;
            ((Peer2PeerTransport)NetworkManager.singleton).OnMatchEnd -= SetAsJoinable;
        }

        Debug.Log("Left steam lobby");

        PlayerNameById = new();
        PlayerIDs = new();

        IsHosting = false;
        IsInLobby = false;

        SteamMatchmaking.LeaveLobby(lobbyID);
    }

    public void FetchLobbyInfo()
    {
        if (!IsSteamActive)
            return;

        FetchFriendLobbyInfo();
        // Filter lobbies of same game version
        SteamMatchmaking.AddRequestLobbyListStringFilter(Lobby.GameVersionProperty,
                                                            Application.version,
                                                            ELobbyComparison.k_ELobbyComparisonEqual);
        // Filter lobbies with available player slots
        SteamMatchmaking.AddRequestLobbyListNumericalFilter(Lobby.AvailableSlotsProperty,
                                                            PlayerInputManagerController.Singleton.NumInputs,
                                                            ELobbyComparison.k_ELobbyComparisonEqualToOrGreaterThan);
        SteamMatchmaking.RequestLobbyList();
    }

    public void FetchQueueLobbyInfo()
    {
        if (!IsSteamActive)
            return;

        // Filter lobbies of same game version
        SteamMatchmaking.AddRequestLobbyListStringFilter(Lobby.GameVersionProperty,
                                                            Application.version,
                                                            ELobbyComparison.k_ELobbyComparisonEqual);
        // Filter lobbies according to gamemodes
        var selectedGameMode = (int)MatchRules.Singleton.Rules.GameMode;
        SteamMatchmaking.AddRequestLobbyListStringFilter(Lobby.GameModeProperty,
                                                    selectedGameMode.ToString(),
                                                    ELobbyComparison.k_ELobbyComparisonEqual);
        // Filter lobbies with available player slots
        SteamMatchmaking.AddRequestLobbyListNumericalFilter(Lobby.AvailableSlotsProperty,
                                                    PlayerInputManagerController.Singleton.NumInputs,
                                                    ELobbyComparison.k_ELobbyComparisonEqualToOrGreaterThan);

        SteamMatchmaking.RequestLobbyList();
    }

    private void FetchFriendLobbyInfo()
    {
        int availableFriends = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
        for (int i = 0; i < availableFriends; i++)
        {
            FriendGameInfo_t friendGameInfo = new FriendGameInfo_t();
            CSteamID steamIDFriend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
            if (SteamFriends.GetFriendGamePlayed(steamIDFriend, out friendGameInfo) && friendGameInfo.m_steamIDLobby.IsValid())
            {
                lobbies.Add(new Lobby(friendGameInfo.m_steamIDLobby));
                SteamMatchmaking.RequestLobbyData(friendGameInfo.m_steamIDLobby);
            }
        }
    }

    private void OnLobbiesFetched(LobbyMatchList_t lobbyList)
    {
        lobbies = new();
        Lobbies = new(lobbies);
        lobbiesById = new();

        for (int i = 0; i < lobbyList.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            var lobby = new Lobby(lobbyID);
            lobbies.Add(lobby);
            lobbiesById.Add(lobby.id.m_SteamID, lobby);

            SteamMatchmaking.RequestLobbyData(lobbyID);
        }

        LobbyListUpdate?.Invoke();
    }

    private void OnLobbyInfo(LobbyDataUpdate_t result)
    {
        if (!lobbiesById.TryGetValue(result.m_ulSteamIDLobby, out var lobby))
            return;

        lobby.UpdateMetadata();
        LobbyListUpdate?.Invoke();
    }

    public bool RequestLobbyJoin(CSteamID lobbyId)
    {
        var hasLessCapacityThanNeeded = !int.TryParse(SteamMatchmaking.GetLobbyData(lobbyId, Lobby.AvailableSlotsProperty), out var slots)
                                        || slots < PlayerInputManagerController.Singleton.NumInputs;
        if (hasLessCapacityThanNeeded)
            return false;
        SteamMatchmaking.JoinLobby(lobbyId);
        return true;
    }

    #endregion Lobby

    private void Update()
    {
        if (!isSteamInitialized)
            return;

        SteamAPI.RunCallbacks();

        if (shouldStoreStats)
        {
            // Try storing stats again if this attempt failed.
            shouldStoreStats = !SteamUserStats.StoreStats();
        }
    }

    private void OnApplicationQuit()
    {
        if (isSteamInitialized)
            SteamAPI.Shutdown();
    }

}
