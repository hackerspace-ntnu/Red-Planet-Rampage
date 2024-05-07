using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

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
    public List<string> PlayerNames = new List<string>();
    public Dictionary<int, (string, int)> PlayerDictionary = new Dictionary<int, (string, int)>();
    public delegate void LobbyEvent();
    public LobbyEvent LobbyPlayerUpdate;

    private bool shouldStoreStats = false;

    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<LobbyEnter_t> lobbyEnter;
    private Callback<GameLobbyJoinRequested_t> joinRequest;
    private Callback<LobbyChatUpdate_t> lobbyUpdate;

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

        RequestStats();
        UserName = SteamFriends.GetPersonaName();
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
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEnter(LobbyEnter_t callback)
    {
        // All users
        PlayerInputManagerController.Singleton.RemoveJoinListener();
        // TODO: set steam names over players
        var lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log("onLobbyUp");
        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobbyId); i++)
        {
            string name = SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i));
            if (!PlayerNames.Contains(name))
                PlayerNames.Add(name);
        }
        LobbyPlayerUpdate.Invoke();
        if (NetworkServer.active)
            return;
        // Only clients from here!
        transportProtocol.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), hostkey);
        transportProtocol.StartClient();
    }

    private void OnLobbyUpdate(LobbyChatUpdate_t callback)
    {
        var lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log("onLobbyUp");
        for (int i = 0; i < SteamMatchmaking.GetNumLobbyMembers(lobbyId); i++)
        {
            string name = SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, i));
            if (!PlayerNames.Contains(name))
                PlayerNames.Add(name);
        }
        LobbyPlayerUpdate.Invoke();
    }

    public void HostLobby()
    {
        if (!isSteamInitialized)
            return;

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, transportProtocol.maxConnections);
        // Disable local joining
        PlayerInputManagerController.Singleton.RemoveJoinListener();
        for (int i = 0; i < PlayerInputManagerController.Singleton.LocalPlayerInputs.Count; i++)
        {
            if (i == 0)
                continue;
            PlayerInputManagerController.Singleton.LocalPlayerInputs[i].gameObject.SetActive(false);
        } 
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

    #endregion Lobby

    public bool ChangeScene(string sceneName)
    {
        if (!isSteamInitialized || !transportProtocol.isNetworkActive)
            return false;
        transportProtocol.ServerChangeScene(sceneName);
        return true;
    }

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
