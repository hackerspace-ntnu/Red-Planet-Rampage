using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CollectionExtensions;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO consider splitting this into match-specific state and general player metadata
public struct PlayerDetails
{
    public uint id;
    public ulong steamID;
    public int localInputID;
    public PlayerType type;

    public string name;
    public Color color;

    public int chips;
    public string[] bodies;
    public string[] barrels;
    public string[] extensions;
    public string body;
    public string barrel;
    public string extension;
}

public struct PlayerConnectedMessage : NetworkMessage
{
    public PlayerConnectedMessage(int inputID)
    {
        this.inputID = inputID;
    }

    public int inputID;
}

public struct PlayerLeftMessage : NetworkMessage
{
    public PlayerLeftMessage(uint id)
    {
        this.id = id;
    }

    public uint id;
}

public struct InitialPlayerDetailsMessage : NetworkMessage
{
    public InitialPlayerDetailsMessage(PlayerDetails details)
    {
        this.details = details;
    }
    public PlayerDetails details;
}

public struct UpdatedPlayerDetailsMessage : NetworkMessage
{
    public UpdatedPlayerDetailsMessage(PlayerDetails details)
    {
        this.details = details;
    }
    public PlayerDetails details;
}

public struct UpdateLoadoutMessage : NetworkMessage
{
    public UpdateLoadoutMessage(uint id, string body, string barrel, string extension)
    {
        this.id = id;
        this.body = body;
        this.barrel = barrel;
        this.extension = extension;
    }
    public uint id;
    public string body;
    public string barrel;
    public string extension;
}

public struct StartMatchMessage : NetworkMessage { }

public struct SpawnPlayerMessage : NetworkMessage
{
    public SpawnPlayerMessage(uint id, PlayerType type)
    {
        this.id = id;
        this.type = type;
    }

    public uint id;
    public PlayerType type;
}

public struct InitializePlayerMessage : NetworkMessage
{
    public InitializePlayerMessage(uint id, Vector3 position, Quaternion rotation)
    {
        this.id = id;
        this.position = position;
        this.rotation = rotation;
    }

    public uint id;
    public Vector3 position;
    public Quaternion rotation;
}

public enum PlayerType
{
    Local,
    AI,
    Remote
}

public class Peer2PeerTransport : NetworkManager
{
    private const int FPSPlayerPrefabIndex = 0;
    private const int BiddingPlayerPrefabIndexOffset = 1;
    private const int AIFPSPlayerPrefabIndex = 2;
    private const int TrainingPlayerPrefabIndex = 4;

    private const int NetworkPlayerLayer = 3;

    private PlayerFactory playerFactory;
    private static Transform[] spawnPoints;
    private static Stack<Transform> spawnPointStack;
    private static int playerIndex;
    private static Stack<Color> availableColors = new();

    private static bool isInMatch;
    public static bool IsInMatch => isInMatch;

    private static Dictionary<uint, PlayerDetails> players = new();
    public static int NumPlayers => players.Count;
    public const int MaxPlayers = 4;
    public static IEnumerable<PlayerDetails> PlayerDetails => players.Values;

    private static List<uint> localPlayerIds = new();

    private static Dictionary<uint, PlayerManager> playerInstances = new();
    public static ReadOnlyDictionary<uint, PlayerManager> PlayerInstanceByID;

    /// <summary>
    /// List of client connections. Will be uninitialized on clients.
    /// </summary>
    private static List<NetworkConnectionToClient> connections = new();
    private static Dictionary<int, List<uint>> playersForConnection = new();
    private static List<uint> connectedPlayers = new();
    public static ReadOnlyCollection<NetworkConnectionToClient> Connections;

    public delegate void LobbyPlayerEvent(PlayerDetails details);
    public LobbyPlayerEvent OnPlayerRecieved;
    public LobbyPlayerEvent OnPlayerRemoved;

    public delegate void ConnectionEvent(int connectionID);
    public ConnectionEvent OnDisconnect;

    private void ResetState()
    {
        isInMatch = false;
        playerIndex = 0;
        players = new();
        playerInstances = new();
        PlayerInstanceByID = new(playerInstances);
        localPlayerIds = new();
        availableColors = new(PlayerInputManagerController.Singleton.PlayerColors.Reverse());

        connections = new();
        connectedPlayers = new();
        playersForConnection = new();
        Connections = new(connections);
    }

    public override void OnStartServer()
    {
        NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnSpawnPlayerInput);
        NetworkServer.RegisterHandler<UpdateLoadoutMessage>(OnReceiveUpdateLoadout);

        ResetState();
    }

    #region Player joining 

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        NetworkClient.RegisterHandler<StartMatchMessage>(OnStartMatch);
        NetworkClient.RegisterHandler<PlayerLeftMessage>(OnPlayerLeft);
        NetworkClient.RegisterHandler<InitialPlayerDetailsMessage>(OnReceivePlayerDetails);
        NetworkClient.RegisterHandler<InitializePlayerMessage>(InitializeFPSPlayer);
        NetworkClient.RegisterHandler<UpdatedPlayerDetailsMessage>(OnReceiveUpdatedPlayerDetails);

        // Send message for the input that we assume is registered
        // TODO doesn't work if players haven't pressed a key yet
        PlayerInputManagerController.Singleton.JoinAllInputs();

        ResetState();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient connection)
    {
        if (!playersForConnection.ContainsKey(connection.connectionId))
            return;
        Debug.Log($"Removed connection {connections.IndexOf(connection)} which has players {playersForConnection[connection.connectionId].ToCommaSeparatedString()}");
        connections.Remove(connection);
        var playerIDs = playersForConnection[connection.connectionId];
        connectedPlayers.RemoveAll(id => playerIDs.Contains(id));
        playersForConnection.Remove(connection.connectionId);
        if (!isInMatch)
        {
            foreach (var id in playerIDs)
            {
                if (!players.TryGetValue(id, out var details))
                    continue;
                availableColors.Push(details.color);
                NetworkServer.SendToAll(new PlayerLeftMessage(id));
            }
        }
        OnDisconnect?.Invoke(connection.connectionId);
    }

    public override void OnStopServer()
    {
        Debug.Log("Stopped server");
        if (SteamManager.IsSteamActive && SteamManager.Singleton.IsInLobby)
            SteamManager.Singleton.LeaveLobby();
        ResetState();
    }

    public override void OnStopClient()
    {
        Debug.Log("Stopped client");
        if (SteamManager.IsSteamActive && SteamManager.Singleton.IsInLobby)
            SteamManager.Singleton.LeaveLobby();
        ResetState();
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("Disconnected as client");
        LoadingScreen.Singleton.Hide();
        MusicTrackManager.Singleton.SwitchTo(MusicType.Menu);
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
        SceneManager.LoadSceneAsync(Scenes.Menu);
        if (NetworkClient.active)
            StopClient();
        else
            ResetState();
    }

    private void OnPlayerLeft(PlayerLeftMessage message)
    {
        if (!players.TryGetValue(message.id, out var playerDetails))
        {
            Debug.LogError($"Received leave message for invalid player {message.id}");
            return;
        }

        Debug.Log($"Player {message.id} {playerDetails.name} left");

        players.Remove(message.id);
        if (playerInstances.ContainsKey(message.id))
            playerInstances.Remove(message.id);

        OnPlayerRemoved?.Invoke(playerDetails);
    }

    public void JoinLobby(string address = "127.0.0.1")
    {
        if (NetworkServer.active)
            return;
        LoadingScreen.Singleton.Show();
        // Only clients from here!
        PlayerInputManagerController.Singleton.RemoveJoinListener();
        SceneManager.LoadScene(Scenes.ClientLobby);
        networkAddress = address;
        StartClient();
    }

    public static void StartTrainingMode()
    {
        singleton.StartHost();
        players = new();

        for (var i = 0; i < PlayerInputManagerController.Singleton.LocalPlayerInputs.Count; i++)
        {
            var details = new PlayerDetails
            {
                id = (uint)i,
                localInputID = i,
                steamID = 0,
                name = "Player",
                type = PlayerType.Local,
                color = PlayerInputManagerController.Singleton.PlayerColors[i],
            };
            players.Add((uint)i, details);
            connectedPlayers.Add((uint)i);
            NetworkServer.SendToAll(new InitialPlayerDetailsMessage(details));
        }

        singleton.StartCoroutine(WaitAndSwitchToTrainingMode());
    }

    private static IEnumerator WaitAndSwitchToTrainingMode()
    {
        LoadingScreen.Singleton.Show();

        // Wait for player details to be populated
        while (!NetworkClient.isConnected && !singleton.isNetworkActive && players.Count < PlayerInputManagerController.Singleton.LocalPlayerInputs.Count)
            yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(LoadingScreen.Singleton.MandatoryDuration);
        singleton.ServerChangeScene(Scenes.TrainingMode);

        // Wait for player(s) to have spawned
        // TODO add a timeout for these wait-for-spawn spins
        while (FindObjectsByType<PlayerManager>(FindObjectsSortMode.None).Count() < players.Count)
            yield return new WaitForEndOfFrame();
        LoadingScreen.Singleton.Hide();
    }

    // TODO custom method for leaving training mode :)
    // TODO handle leaving of match/lobby better

    public void StartMatch(string mapName)
    {
        NetworkServer.SendToAll(new StartMatchMessage());
        StartCoroutine(WaitAndStartMatch(mapName));
    }

    private IEnumerator WaitAndStartMatch(string mapName)
    {
        yield return new WaitForSeconds(LoadingScreen.Singleton.MandatoryDuration);
        ServerChangeScene(mapName);
    }

    private void OnStartMatch(StartMatchMessage message)
    {
        var mainMenuController = FindAnyObjectByType<MainMenuController>();
        if (mainMenuController)
            mainMenuController.DisableSceneSwitching();
        LoadingScreen.Singleton.Show();
    }

    private void OnSpawnPlayerInput(NetworkConnectionToClient connection, PlayerConnectedMessage message)
    {
        // Avoid adding more than the four allowed players
        // TODO prevent this in some other more sustainable way :)))))
        if (NumPlayers >= MaxPlayers)
            return;

        // Register connection
        PlayerInputManagerController.Singleton.NetworkClients.Add(connection);
        var isConnectionAlreadyPresent = true;
        if (!connections.Contains(connection))
        {
            isConnectionAlreadyPresent = false;
            connections.Add(connection);
            connectedPlayers.Add((uint)playerIndex);
        }
        if (!playersForConnection.ContainsKey(connection.connectionId))
            playersForConnection[connection.connectionId] = new();
        playersForConnection[connection.connectionId].Add((uint)playerIndex);

        // Determine metadata
        var playerType = PlayerType.Local;
        var playerName = "Player";
        ulong steamID = 0;
        if (SteamManager.IsSteamActive && SteamManager.Singleton.IsHosting)
        {
            var steamIndex = SteamManager.Singleton.PlayerNames.Count - 1;
            playerName = SteamManager.Singleton.PlayerNames[steamIndex];
            steamID = SteamManager.Singleton.PlayerIDs[steamIndex];
            playerType = steamID == SteamManager.Singleton.SteamID.m_SteamID ? PlayerType.Local : PlayerType.Remote;
        }

        // Send information about existing players to the new one
        if (!isConnectionAlreadyPresent)
        {
            foreach (var existingPlayer in players.Values)
            {
                connection.Send(new InitialPlayerDetailsMessage(existingPlayer));
            }
        }

        // Pick among the available colors
        if (!availableColors.TryPop(out var color))
        {
            // Recycle colors if necessary
            availableColors = new(PlayerInputManagerController.Singleton.PlayerColors.Reverse());
            color = availableColors.Pop();
        }

        var details = new PlayerDetails
        {
            id = (uint)playerIndex,
            localInputID = message.inputID,
            steamID = steamID,
            type = playerType,
            name = playerName,
            color = color,
        };

        // Send information about this player to all
        NetworkServer.SendToAll(new InitialPlayerDetailsMessage(details));
        playerIndex++;
    }

    private void OnReceivePlayerDetails(InitialPlayerDetailsMessage message)
    {
        var details = message.details;
        if (players.ContainsKey(details.id))
            return;
        details.type =
            details.type is PlayerType.AI
                ? PlayerType.AI
                : details.steamID == 0 || SteamManager.Singleton.SteamID.m_SteamID == details.steamID
                    ? PlayerType.Local
                    : PlayerType.Remote;
        if (details.type is PlayerType.Local && !localPlayerIds.Contains(details.id))
        {
            localPlayerIds.Add(details.id);
        }
        // TODO move this bit!
        details.body = StaticInfo.Singleton.StartingBody.id;
        details.barrel = StaticInfo.Singleton.StartingBarrel.id;
        details.extension = StaticInfo.Singleton.StartingExtension ? StaticInfo.Singleton.StartingExtension.id : null;
        details.bodies = new string[] { details.body };
        details.barrels = new string[] { details.barrel };
        details.extensions = details.extension != null ? new string[] { details.extension } : new string[] { };
        Debug.Log($"Received info for player {details.id}: name=<color=#{details.color.ToHexString()}>{details.name}</color> type={details.type}");
        players.Add(details.id, details);
        OnPlayerRecieved?.Invoke(details);
    }

    private void OnReceiveUpdatedPlayerDetails(UpdatedPlayerDetailsMessage message)
    {
        var details = message.details;
        if (!players.ContainsKey(details.id))
        {
            Debug.LogError($"Received updated info for invalid player {details.id}");
            return;
        }
        // Use the player type that we've already figured out
        details.type = players[details.id].type;
        Debug.Log($"Received updated info for player {details.id}: chips={details.chips}, bodies={details.bodies?.ToCommaSeparatedString()}, barrels={details.barrels?.ToCommaSeparatedString()}, extensions={details.extensions?.ToCommaSeparatedString()}");
        players[details.id] = details;
    }

    #endregion

    #region Scene changes

    private void AddAiPlayers()
    {
        for (var i = players.Count; i < MaxPlayers; i++)
        {
            var details = new PlayerDetails
            {
                id = (uint)i,
                localInputID = i,
                steamID = 0,
                name = "HCU",
                type = PlayerType.AI,
                color = PlayerInputManagerController.Singleton.AIColors[i - players.Count + 1],
            };
            NetworkServer.SendToAll(new InitialPlayerDetailsMessage(details));
        }
    }

    // Called after shooting rounds (TODO just use the same matchcontroller stuff???)
    private static void UpdatePlayerDetailsAfterShootingRound()
    {
        Debug.Log($"Found {FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Count()} identitites!");
        foreach (var identity in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (identity.Body == null)
                continue;

            UpdatePlayerInventoryForIdentity(identity);
        }
    }

    // Called from auction driver after players have received their new items
    // Separated from loadout update due to inconsistent timing :)))))
    public static void UpdatePlayerDetailsAfterAuction()
    {
        foreach (var player in playerInstances.Values)
        {
            UpdatePlayerInventoryForIdentity(player.identity);
        }
    }

    private static void UpdatePlayerInventoryForIdentity(PlayerIdentity identity)
    {
        if (!players.TryGetValue(identity.id, out var playerDetails))
        {
            Debug.LogError($"Invalid player to update: id={identity.id}");
            return;
        }

        playerDetails.chips = identity.chips;
        playerDetails.bodies = identity.Bodies.Select(item => item.id).ToArray();
        playerDetails.barrels = identity.Barrels.Select(item => item.id).ToArray();
        playerDetails.extensions = identity.Extensions.Select(item => item.id).ToArray();
        players[identity.id] = playerDetails;
        NetworkServer.SendToAll(new UpdatedPlayerDetailsMessage(playerDetails));
    }

    public override void OnServerChangeScene(string newSceneName)
    {
        var needsExtraAiPlayers = PlayerInputManagerController.Singleton.MatchHasAI && !MatchController.Singleton;
        if (needsExtraAiPlayers)
        {
            AddAiPlayers();
        }

        switch (newSceneName)
        {
            case Scenes.Bidding:
                isInMatch = true;
                UpdatePlayerDetailsAfterShootingRound();
                NetworkServer.ReplaceHandler<SpawnPlayerMessage>(OnSpawnBiddingPlayer);
                break;
            case Scenes.Menu:
                isInMatch = false;
                NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnSpawnPlayerInput);
                break;
            default:
                isInMatch = true;
                NetworkServer.ReplaceHandler<SpawnPlayerMessage>(OnSpawnFPSPlayer);
                break;
        }
    }

    public void UpdateLoadout()
    {
        Debug.Log($"Found {FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Count()} identitites!");
        foreach (var identity in FindObjectsByType<PlayerIdentity>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (identity.Body == null)
                continue;

            if (!players.TryGetValue(identity.id, out var playerDetails))
            {
                Debug.LogError($"Invalid player to update: id={identity.id}");
                continue;
            }

            // Only update local players!
            if (playerDetails.type is PlayerType.Remote)
                continue;

            NetworkClient.Send(new UpdateLoadoutMessage(identity.id, identity.Body.id, identity.Barrel.id, identity.Extension ? identity.Extension.id : null));
        }
    }

    private void OnReceiveUpdateLoadout(NetworkConnectionToClient connection, UpdateLoadoutMessage message)
    {
        if (!players.TryGetValue(message.id, out var playerDetails))
            return;
        // TODO verify that body, barrel, extension is in the available items for the player!
        playerDetails.body = message.body;
        playerDetails.barrel = message.barrel;
        playerDetails.extension = message.extension;
        Debug.Log($"Received updated loadout for player {message.id}: body={message.body}, barrel={message.barrel}, extension={message.extension}");
        players[playerDetails.id] = playerDetails;
        NetworkServer.SendToAll(new UpdatedPlayerDetailsMessage(playerDetails));
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        var originalSceneName = SceneManager.GetActiveScene().name;
        switch (newSceneName)
        {
            // TODO consider just pushing music change into this switch block
            case Scenes.Bidding:
                isInMatch = true;
                NetworkClient.ReplaceHandler<InitializePlayerMessage>(InitializeBiddingPlayer);
                PlayerInputManagerController.Singleton.ChangeInputMaps("Bidding");
                break;
            case Scenes.Menu:
                isInMatch = false;
                PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
                break;
            default:
                isInMatch = true;
                NetworkClient.ReplaceHandler<InitializePlayerMessage>(InitializeFPSPlayer);
                PlayerInputManagerController.Singleton.ChangeInputMaps("FPS");
                break;
        }

        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

        StartCoroutine(SendSpawnRequestsAfterSceneLoad(originalSceneName));
    }

    private IEnumerator SendSpawnRequestsAfterSceneLoad(string originalSceneName)
    {
        while (SceneManager.GetActiveScene().name == originalSceneName)
            yield return null;

        // Spawn players for our inputs (and bots)
        foreach (var id in localPlayerIds)
        {
            NetworkClient.Send(new SpawnPlayerMessage(id, PlayerType.Local));
        }

        if (!NetworkServer.active)
            yield break;

        // TODO rework this for dedicated server stuff
        //      (where it needs to do this on server change scene, and without a network client)
        //      (perhaps you could call the spawn methods directly?)
        foreach (var p in players.Values.Where(p => p.type == PlayerType.AI))
        {
            NetworkClient.Send(new SpawnPlayerMessage(p.id, PlayerType.AI));
        }

        foreach (var p in players.Values.Where(p => p.type is PlayerType.Remote && !connectedPlayers.Contains(p.id)))
        {
            NetworkClient.Send(new SpawnPlayerMessage(p.id, PlayerType.Local));
        }
    }

    #endregion

    #region Spawn FPS players


    private void SpawnPlayer(NetworkConnectionToClient connection, SpawnPlayerMessage message, int prefabIndexOffset = 0)
    {
        if (!players.TryGetValue(message.id, out var playerDetails))
        {
            Debug.LogError($"No such player: id={message.id}");
            return;
        }
        Debug.Log($"Spawning player {message.id}");

        if (!playerFactory)
        {
            playerFactory = FindAnyObjectByType<PlayerFactory>();
            if (!playerFactory) // TODO shouldn't happen, seems to occur when you go back to menu after end of match
                return;
            spawnPoints = playerFactory.GetRandomSpawnpoints();
            spawnPointStack = new(spawnPoints);
        }

        // Ensure we aren't providing invalid spawnpoints.
        // We should never run out of spawnpoints in normal circumstances, but you never know 💀
        if (!spawnPointStack.TryPop(out var spawnPoint))
            spawnPoint = spawnPoints.RandomElement();

        // Instantiate correct prefab for player and mode.
        var prefabIndex = playerDetails.type is PlayerType.AI && NetworkServer.active ? AIFPSPlayerPrefabIndex : FPSPlayerPrefabIndex;
        if (SceneManager.GetActiveScene().name == Scenes.TrainingMode)
            prefabIndex = TrainingPlayerPrefabIndex;
        var prefab = spawnPrefabs[prefabIndex + prefabIndexOffset];
        var player = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<PlayerManager>().id = message.id;

        // Spawn player, setting it as the player for a connection only for the first local player for each connection.
        var isNotAI = playerDetails.type is not PlayerType.AI;
        var isFirstLocalPlayer = playerDetails.localInputID == 0;
        var isNotDisconnected = playerDetails.type is not PlayerType.Remote || connectedPlayers.Contains(playerDetails.id);
        if (isNotAI && isFirstLocalPlayer && isNotDisconnected)
            NetworkServer.AddPlayerForConnection(connection, player);
        else
            NetworkServer.Spawn(player, connection);

        NetworkServer.SendToAll(new InitializePlayerMessage(message.id, spawnPoint.position, spawnPoint.rotation));
    }

    private void OnSpawnFPSPlayer(NetworkConnectionToClient connection, SpawnPlayerMessage message)
    {
        SpawnPlayer(connection, message, 0);
    }

    private void InitializeFPSPlayer(InitializePlayerMessage message)
    {
        StartCoroutine(WaitAndInitializeFPSPlayer(message));
    }

    // TODO move this somewhere else?
    public static string PlayerNameWithIndex(PlayerDetails playerDetails)
    {
        var playerName = playerDetails.name;
        if (players.Values.Count(p => p.steamID == playerDetails.steamID) > 1)
            playerName = $"{playerName} {playerDetails.localInputID + 1}";
        return playerName;
    }

    private void UpdateIdentityFromDetails(PlayerIdentity identity, PlayerDetails playerDetails)
    {
        identity.UpdateFromDetails(playerDetails, PlayerNameWithIndex(playerDetails));
    }

    private IEnumerator WaitAndInitializeFPSPlayer(InitializePlayerMessage message)
    {
        // Wait until player object is spawned
        PlayerManager player = null;
        while (player == null)
        {
            player = FindObjectsOfType<PlayerManager>()
               .FirstOrDefault(p => p.id == message.id);
            yield return null;
        }

        if (!player)
        {
            Debug.LogError($"Could not find player object for id {message.id}");
            yield break;
        }

        if (!players.TryGetValue(message.id, out var playerDetails))
        {
            Debug.LogError($"Could not find player details for id {message.id}");
            yield break;
        }

        var playerManager = player.GetComponent<PlayerManager>();

        player.transform.position = message.position;
        player.transform.rotation = message.rotation;

        var cameraOffset = player.transform.Find("CameraOffset");
        playerManager.GetComponent<AmmoBoxCollector>().enabled = true;

        if (playerDetails.type is PlayerType.Local)
        {
            Debug.Log($"Spawning local player {playerDetails.id}");
            var input = PlayerInputManagerController.Singleton.LocalPlayerInputs[playerDetails.localInputID];
            // Make playerInput child of player it's attached to
            input.transform.parent = player.transform;
            // Set received playerInput (and most importantly its camera) at an offset from player's position
            input.transform.localPosition = cameraOffset.localPosition;
            input.transform.rotation = player.transform.rotation;

            // Enable Camera
            input.PlayerCamera.enabled = true;
            input.PlayerCamera.orthographic = false;

            playerManager.HUDController.gameObject.SetActive(true);
            var movement = player.GetComponent<PlayerMovement>();

            // The identity sits on the input in this case, so edit that
            var identity = input.GetComponent<PlayerIdentity>();
            UpdateIdentityFromDetails(identity, playerDetails);

            // Update player's movement script with which playerInput it should attach listeners to
            playerManager.SetPlayerInput(input);
            var gunHolder = input.transform.GetChild(0);
            playerManager.SetGun(gunHolder);

            // Set unique layer for player
            playerManager.SetLayer(input.playerInput.playerIndex);
            movement.SetInitialRotation(message.rotation.eulerAngles.y * Mathf.Deg2Rad);

            // TODO jeez this should stay elsewhere
            if (GunFactory.TryGetGunAchievement(playerManager.identity.Body, playerManager.identity.Barrel,
                    playerManager.identity.Extension, out var achievement))
                SteamManager.Singleton.UnlockAchievement(achievement);
        }
        else if (playerDetails.type is PlayerType.AI && NetworkServer.active)
        {
            Debug.Log($"Spawning AI player {playerDetails.id}");
            AIManager manager = player.GetComponent<AIManager>();
            manager.SetLayer(NetworkPlayerLayer);
            UpdateIdentityFromDetails(playerManager.identity, playerDetails);
            manager.SetIdentity(playerManager.identity);
            manager.GetComponent<AIMovement>().SetInitialRotation(message.rotation.eulerAngles.y * Mathf.Deg2Rad);
        }
        else
        {
            Debug.Log($"Spawning network player {playerDetails.id}");

            UpdateIdentityFromDetails(playerManager.identity, playerDetails);

            // TODO do some other version of disabling HUD completely
            Destroy(playerManager.HUDController);

            // Disable physics
            playerManager.GetComponent<Rigidbody>().isKinematic = true;

            // Create display gun structure
            var gunHolderParent = new GameObject("DisplayGunParent").transform;
            gunHolderParent.parent = player.transform;
            gunHolderParent.position = cameraOffset.position;
            gunHolderParent.rotation = player.transform.rotation;
            var gunHolder = new GameObject("DisplayGunHolder").transform;
            gunHolder.parent = gunHolderParent.transform;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
            playerManager.SetLayer(NetworkPlayerLayer);
            // Can't initialize quite like the AIs because of where the GunController network behaviour is located :(
            playerManager.SetGun(gunHolder);
        }

        playerManager.ApplyIdentity();

        // This ensures that behaviours on the gun have identities.
        // SHOULD be safe to initialize them here as this is at roughly the same point on all clients
        player.GetComponent<NetworkIdentity>().InitializeNetworkBehaviours();

        if (MatchController.Singleton)
        {
            MatchController.Singleton.RegisterPlayer(playerManager);
        }

        playerInstances[player.id] = player;
    }

    #endregion

    #region Bidding spawning

    private void OnSpawnBiddingPlayer(NetworkConnectionToClient connection, SpawnPlayerMessage message)
    {
        SpawnPlayer(connection, message, BiddingPlayerPrefabIndexOffset);
    }

    private void InitializeBiddingPlayer(InitializePlayerMessage message)
    {
        StartCoroutine(WaitAndInitializeBiddingPlayer(message));
    }

    private IEnumerator WaitAndInitializeBiddingPlayer(InitializePlayerMessage message)
    {
        // Wait until players must've been spawned
        PlayerManager player = null;
        while (player == null)
        {
            player = FindObjectsOfType<PlayerManager>()
               .FirstOrDefault(p => p.id == message.id);
            yield return null;
        }

        if (!player)
        {
            Debug.LogError($"Could not find player object for id {message.id}");
            yield break;
        }

        if (!players.TryGetValue(message.id, out var playerDetails))
        {
            Debug.LogError($"Could not find player details for id {message.id}");
            yield break;
        }

        var playerManager = player.GetComponent<PlayerManager>();

        player.transform.position = message.position;
        player.transform.rotation = message.rotation;

        var cameraOffset = player.transform.Find("CameraOffset");
        playerManager.GetComponent<AmmoBoxCollector>().enabled = true;

        if (playerDetails.type is PlayerType.Local)
        {
            Debug.Log($"Spawning local player {playerDetails.id}");
            var input = PlayerInputManagerController.Singleton.LocalPlayerInputs[playerDetails.localInputID];

            // Make playerInput child of player it's attached to
            input.transform.parent = player.transform;
            // Set received playerInput (and most importantly its camera) at an offset from player's position
            input.transform.localPosition = cameraOffset.localPosition;
            input.transform.rotation = player.transform.rotation;

            // Disable Camera
            input.PlayerCamera.enabled = false;

            // Update player's movement script with which playerInput it should attach listeners to
            playerManager.SetPlayerInput(input);
            player.GetComponent<HealthController>().enabled = false;

            // The identity sits on the input in this case, so edit that
            var identity = input.GetComponent<PlayerIdentity>();
            UpdateIdentityFromDetails(identity, playerDetails);
        }
        else if (playerDetails.type is PlayerType.AI && NetworkServer.active)
        {
            Debug.Log($"Spawning AI player {playerDetails.id}");
            AIManager manager = player.GetComponent<AIManager>();
            manager.SetLayer(NetworkPlayerLayer);
            UpdateIdentityFromDetails(playerManager.identity, playerDetails);
            manager.SetIdentity(manager.identity);
        }
        else
        {
            Debug.Log($"Spawning network player {playerDetails.id}");

            UpdateIdentityFromDetails(playerManager.identity, playerDetails);

            // Disable physics
            playerManager.GetComponent<Rigidbody>().isKinematic = true;
        }

        playerManager.ApplyIdentity();

        // This ensures that behaviours on the gun have identities.
        // SHOULD be safe to initialize them here as this is at roughly the same point on all clients
        player.GetComponent<NetworkIdentity>().InitializeNetworkBehaviours();

        if (MatchController.Singleton)
        {
            MatchController.Singleton.RegisterPlayer(playerManager);
        }

        playerInstances[player.id] = player;
    }

    #endregion
}
