using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    private const int NetworkPlayerLayer = 3;

    private PlayerFactory playerFactory;
    private static Transform[] spawnPoints;
    private static int playerIndex;

    private static Dictionary<uint, PlayerDetails> players = new();
    public static int NumPlayersInMatch => players.Count;
    public static IEnumerable<PlayerDetails> PlayerDetails => players.Values;

    private static List<uint> localPlayerIds = new();

    public delegate void LobbyPlayerEvent(PlayerDetails details);
    public LobbyPlayerEvent OnPlayerRecieved;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnSpawnPlayerInput);
        NetworkServer.RegisterHandler<UpdateLoadoutMessage>(OnReceiveUpdateLoadout);

        // Reinitialize player lookups
        players = new();
        playerIndex = 0;
    }

    #region Player joining 

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        NetworkClient.RegisterHandler<InitialPlayerDetailsMessage>(OnReceivePlayerDetails);
        NetworkClient.RegisterHandler<InitializePlayerMessage>(InitializeFPSPlayer);
        NetworkClient.RegisterHandler<UpdatedPlayerDetailsMessage>(OnReceiveUpdatedPlayerDetails);

        // Send message for the input that we assume is registered
        // TODO doesn't work if players haven't pressed a key yet
        PlayerInputManagerController.Singleton.JoinAllInputs();

        players = new();
        playerIndex = 0;
    }

    public void JoinLobby(string address = "127.0.0.1")
    {
        if (NetworkServer.active)
            return;
        // Only clients from here!
        PlayerInputManagerController.Singleton.RemoveJoinListener();
        SceneManager.LoadScene("ClientLobby");
        networkAddress = address;
        StartClient();
    }

    private void OnSpawnPlayerInput(NetworkConnectionToClient connection, PlayerConnectedMessage message)
    {
        // Avoid adding more than the four allowed players
        // TODO prevent this in some other more sustainable way :)))))
        if (PlayerInputManagerController.Singleton.NetworkClients.Count >= 4)
            return;
        PlayerInputManagerController.Singleton.NetworkClients.Add(connection);

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

        var player = Instantiate(playerPrefab);
        var playerInput = player.GetComponent<InputManager>();
        playerInput.PlayerCamera.enabled = false;
        playerInput.enabled = connection.connectionId == NetworkClient.connection.connectionId;
        // Without this, the host doesn't function as a client
        if (message.inputID == 0)
            NetworkServer.AddPlayerForConnection(connection, player);
        else
            NetworkServer.Spawn(player, connection);


        // Send information about existing players to the new one
        foreach (var existingPlayer in players.Values)
        {
            connection.Send(new InitialPlayerDetailsMessage(existingPlayer));
        }

        // TODO consider just putting this stuff into the InitialPlayerDetailsMessage
        var details = new PlayerDetails
        {
            id = (uint)playerIndex,
            localInputID = message.inputID,
            steamID = steamID,
            type = playerType,
            name = playerName,
            color = PlayerInputManagerController.Singleton.PlayerColors[playerIndex],
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
            return;
        // Use the player type that we've already figured out
        details.type = players[details.id].type;
        Debug.Log($"Received updated info for player {details.id}: chips={details.chips}, body={details.body}, barrel={details.barrel}, extension={details.extension}");
        players[details.id] = details;
    }

    #endregion

    #region Scene changes

    private void AddAiPlayers()
    {
        for (var i = players.Count; i < 4; i++)
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

    private void UpdatePlayerDetails()
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

            playerDetails.chips = identity.chips;
            playerDetails.bodies = identity.Bodies.Select(item => item.id).ToArray();
            playerDetails.barrels = identity.Barrels.Select(item => item.id).ToArray();
            playerDetails.extensions = identity.Extensions.Select(item => item.id).ToArray();
            players[identity.id] = playerDetails;
            NetworkServer.SendToAll(new UpdatedPlayerDetailsMessage(playerDetails));
        }
    }

    public override void OnServerChangeScene(string newSceneName)
    {
        var needsExtraAiPlayers = PlayerInputManagerController.Singleton.MatchHasAI && !MatchController.Singleton;
        if (needsExtraAiPlayers)
        {
            AddAiPlayers();
        }

        UpdatePlayerDetails();

        switch (newSceneName)
        {
            case "Bidding":
                NetworkServer.ReplaceHandler<SpawnPlayerMessage>(OnSpawnBiddingPlayer);
                break;
            case "Menu":
                NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnSpawnPlayerInput);
                break;
            default:
                NetworkServer.ReplaceHandler<SpawnPlayerMessage>(OnSpawnFPSPlayer);
                break;
        }

        base.OnServerChangeScene(newSceneName);
    }

    private void UpdateLoadout()
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
        UpdateLoadout();
        switch (newSceneName)
        {
            case "Bidding":
                NetworkClient.ReplaceHandler<InitializePlayerMessage>(InitializeBiddingPlayer);
                PlayerInputManagerController.Singleton.ChangeInputMaps("Bidding");
                break;
            case "Menu":
                PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
                break;
            default:
                NetworkClient.ReplaceHandler<InitializePlayerMessage>(InitializeFPSPlayer);
                PlayerInputManagerController.Singleton.ChangeInputMaps("FPS");
                break;
        }

        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

        // Remove existing network inputs since they're in DontDestroyOnLoad
        if (!MatchController.Singleton || MatchController.Singleton.RoundCount <= 1)
        {
            foreach (var networkInput in FindObjectsOfType<PlayerInput>().Select(pi => pi.GetComponent<NetworkIdentity>())
                        .Where(id => id != null))
            {
                Destroy(networkInput);
            }
        }
        foreach (var id in localPlayerIds)
        {
            NetworkClient.Send(new SpawnPlayerMessage(id, PlayerType.Local));
        }
        foreach (var p in players.Values.Where(p => p.type == PlayerType.AI))
        {
            NetworkClient.Send(new SpawnPlayerMessage(p.id, PlayerType.AI));
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
        if (!playerFactory)
        {
            playerFactory = FindAnyObjectByType<PlayerFactory>();
            if (!playerFactory) // TODO shouldn't happen, seems to occur when you go back to menu after end of match
                return;
            spawnPoints = playerFactory.GetRandomSpawnpoints();
        }
        Debug.Log($"Spawning player {message.id}");

        var spawnPoint = spawnPoints[message.id];

        var prefabIndex = playerDetails.type is PlayerType.AI && NetworkServer.active ? AIFPSPlayerPrefabIndex : FPSPlayerPrefabIndex;
        var prefab = spawnPrefabs[prefabIndex + prefabIndexOffset];
        var player = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<PlayerManager>().id = message.id;
        if (playerDetails.type is not PlayerType.AI && playerDetails.localInputID == 0)
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

    // TODO move this down to PlayerIdentity for better encapsulation!
    private void UpdateIdentityFromDetails(PlayerIdentity identity, PlayerDetails playerDetails)
    {
        identity.id = playerDetails.id;
        var playerName = playerDetails.name;
        if (players.Values.Count(p => p.steamID == playerDetails.steamID) > 1)
            playerName = $"{playerName} {playerDetails.localInputID + 1}";

        identity.playerName = playerName;
        identity.color = playerDetails.color;

        identity.chips = playerDetails.chips;
        identity.SetItems(playerDetails.bodies, playerDetails.barrels, playerDetails.extensions);
        identity.SetLoadout(playerDetails.body, playerDetails.barrel, playerDetails.extension);
    }

    private IEnumerator WaitAndInitializeFPSPlayer(InitializePlayerMessage message)
    {
        // Wait until players must've been spawned
        // TODO find a better way to wait for that
        // TODO that is, move to OnWhateverAuthority overrid on PlayerManager :)
        yield return null;
        yield return null;

        var player = FindObjectsOfType<PlayerManager>()
            .FirstOrDefault(p => p.id == message.id);

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


        // This ensures that behaviours on the gun have identities.
        // SHOULD be safe to initialize them here as this is at roughly the same point on all clients
        player.GetComponent<NetworkIdentity>().InitializeNetworkBehaviours();

        if (MatchController.Singleton)
        {
            MatchController.Singleton.RegisterPlayer(playerManager);
        }
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
        // TODO find a better way to wait for that
        // TODO that is, move to OnWhateverAuthority overrid on PlayerManager :)
        yield return null;
        yield return null;

        var player = FindObjectsOfType<PlayerManager>()
            .FirstOrDefault(p => p.id == message.id);

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

        // This ensures that behaviours on the gun have identities.
        // SHOULD be safe to initialize them here as this is at roughly the same point on all clients
        player.GetComponent<NetworkIdentity>().InitializeNetworkBehaviours();

        if (MatchController.Singleton)
        {
            MatchController.Singleton.RegisterPlayer(playerManager);
        }
    }

    #endregion
}
