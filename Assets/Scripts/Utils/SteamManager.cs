using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class SteamManager : MonoBehaviour
{
    private const int steamAppID = 2717710;
    public static SteamManager Singleton;
    public int ConnectedPlayers => transportProtocol.numPlayers;
    private bool isSteamInitialized;
    public bool IsHosting = false;

    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<LobbyEnter_t> lobbyEnter;
    private Callback<GameLobbyJoinRequested_t> joinRequest;

    private ulong hostId;
    private const string hostkey = "HostAddress";
    [SerializeField]
    private Peer2PeerTransport transportProtocol;

    void Awake()
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
    }

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
        hostId = callback.m_ulSteamIDLobby;
        // TODO: set steam names over players

        if (NetworkServer.active)
            return;
        // Only clients from here!
        Instantiate(transportProtocol.playerPrefab);
        transportProtocol.StartClient();
    }

    public void HostLobby()
    {
        if (isSteamInitialized)
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, transportProtocol.maxConnections);
    }

    void Update()
    {
        if (isSteamInitialized)
            SteamAPI.RunCallbacks();
    }
    private void OnApplicationQuit()
    {
        if (isSteamInitialized)
            SteamAPI.Shutdown();
    }

}
