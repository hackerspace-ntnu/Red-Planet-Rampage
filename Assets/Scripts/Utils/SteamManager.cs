using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.SceneManagement;
using System.Collections.ObjectModel;

public enum AchievementType
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
    ItalianPlumber
}

public class SteamManager : MonoBehaviour
{
    private const int steamAppID = 2717710;
    public static SteamManager Singleton;
    public int ConnectedPlayers => transportProtocol.numPlayers;
    private static bool isSteamInitialized;
    public static bool IsSteamActive => isSteamInitialized;
    public bool IsHosting = false;
    public string UserName;
    public CSteamID SteamID;
    public List<string> PlayerNames = new();
    public List<ulong> PlayerIDs = new();
    public delegate void LobbyEvent();
    public LobbyEvent LobbyPlayerUpdate;
    public LobbyEvent LobbyListUpdate;
    private List<CSteamID> lobbies = new();
    public Dictionary<CSteamID, string> Lobbies = new();

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
    [SerializeField]
    private Peer2PeerTransport transportProtocol;

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

    private readonly Dictionary<AchievementType, string> achievementNames = new()
    {
        { AchievementType.DiscoInferno, "WEAPON_DISCO_INFERNO" },
        { AchievementType.WireFraud, "WEAPON_WIRE_FRAUD" },
        { AchievementType.SchizoidMan, "WEAPON_SCHIZOID_MAN" },
        { AchievementType.BlueHat, "WEAPON_BLUE_HAT" },
        { AchievementType.BlackHat, "WEAPON_BLACK_HAT" },
        { AchievementType.EagleEyed, "WEAPON_EAGLE_EYED" },
        { AchievementType.PogoStick, "WEAPON_POGO_STICK" },
        { AchievementType.Skater, "WEAPON_SKATER" },
        { AchievementType.SprayNPray, "WEAPON_SPRAY_N_PRAY" },
        { AchievementType.PingPonginator, "WEAPON_PING_PONG" },
        { AchievementType.Flamethrower, "WEAPON_FLAMETHROWER" },
        { AchievementType.OrbitalTrashCannon, "WEAPON_ORBITAL_TRASH_CANNON" },
        { AchievementType.ItalianPlumber, "WEAPON_ITALIAN_PLUMBER" }
    };

    public void UnlockAchievement(AchievementType type)
    {
        if (!isSteamInitialized)
            return;
        if (type is AchievementType.None || !achievementNames.TryGetValue(type, out var name))
            return;

        SteamUserStats.GetAchievement(name, out var isAlreadyUnlocked);

        if (isAlreadyUnlocked)
            return;

        SteamUserStats.SetAchievement(name);
        shouldStoreStats = true;
    }

    #endregion Achievements

    #region Lobby

    private void OnlobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;
        transportProtocol.StartHost();
        IsHosting = true;
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostkey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", UserName);
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void UpdateLobbyData(ulong lobbyID)
    {
        var lobbyId = new CSteamID(lobbyID);
        Debug.Log("Lobby entered");
        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobbyId); i++)
        {
            var id = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i);
            var name = SteamFriends.GetFriendPersonaName(id);

            if (PlayerIDs.Contains(id.m_SteamID))
                continue;
            Debug.Log($"Steam user {name} (id={id.m_SteamID}) entered lobby");

            // TODO replace these separate lists with *one*
            PlayerNames.Add(name);
            PlayerIDs.Add(id.m_SteamID);
        }
        LobbyPlayerUpdate?.Invoke();
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        // All users
        UpdateLobbyData(callback.m_ulSteamIDLobby);
        if (NetworkServer.active)
            return;
        // Only clients from here!
        transportProtocol.JoinLobby(SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostkey));
    }

    private void OnLobbyUpdate(LobbyChatUpdate_t callback)
    {
        Debug.Log("Lobby updated");
        UpdateLobbyData(callback.m_ulSteamIDLobby);
    }

    public void HostLobby()
    {
        if (!isSteamInitialized)
            return;

        // TODO support public and friend lobbies
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, transportProtocol.maxConnections);
    }

    public void LeaveLobby()
    {
        if (!isSteamInitialized)
            return;
        if (IsHosting)
        {
            transportProtocol.StopHost();
            IsHosting = false;
        }
        else
        {
            if (transportProtocol.isNetworkActive)
                transportProtocol.StopClient();
        }
    }

    public void FetchLobbyInfo()
    {
        FetchFriendLobbyInfo();
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
                lobbies.Add(friendGameInfo.m_steamIDLobby);
                SteamMatchmaking.RequestLobbyData(friendGameInfo.m_steamIDLobby);
            }
        }
    }

    private void OnLobbiesFetched(LobbyMatchList_t lobbyList)
    {
        for (int i = 0; i < lobbyList.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbies.Add(lobbyID);
            Lobbies[lobbyID] = SteamMatchmaking.GetLobbyData(lobbyID, "name");
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
        LobbyListUpdate?.Invoke();
    }

    private void OnLobbyInfo(LobbyDataUpdate_t result)
    {
        for (int i = 0; i < lobbies.Count; i++)
        {
            if (lobbies[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                Lobbies[(CSteamID)lobbies[i].m_SteamID] = SteamMatchmaking.GetLobbyData((CSteamID)lobbies[i].m_SteamID, "name");
                return;
            }
        }
        LobbyListUpdate?.Invoke();
    }

    public void RequestLobbyJoin(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
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
