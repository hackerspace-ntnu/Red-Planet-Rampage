using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    private const int BiddingPlayerPrefabIndex = 1;
    private const int AIFPSPlayerPrefabIndex = 2;
    private const int AIBiddingPlayerPrefabIndex = 3;

    private PlayerFactory playerFactory;
    private static Transform[] spawnPoints;
    private static int playerIndex;

    private static Dictionary<uint, PlayerDetails> players = new();
    public static int NumPlayersInMatch => players.Count();
    public static IEnumerable<PlayerDetails> PlayerDetails => players.Values;

    private static List<uint> localPlayerIds = new();

    public delegate void LobbyPlayerEvent(PlayerDetails details);
    public LobbyPlayerEvent OnPlayerRecieved;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnSpawnPlayerInput);

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
        details.type = details.steamID == 0 || SteamManager.Singleton.SteamID.m_SteamID == details.steamID ? PlayerType.Local : PlayerType.Remote;
        if (details.type is not PlayerType.Remote && !localPlayerIds.Contains(details.id))
        {
            localPlayerIds.Add(details.id);
        }
        Debug.Log($"Received info for player {details.id}: name=<color=#{details.color.ToHexString()}>{details.name}</color> type={details.type}");
        players.Add(details.id, details);
        OnPlayerRecieved?.Invoke(details);
    }

    #endregion

    #region Scene changes

    public override void OnServerChangeScene(string newSceneName)
    {
        base.OnServerChangeScene(newSceneName);
        playerIndex = 0;
        var needsExtraAiPlayers = PlayerInputManagerController.Singleton.MatchHasAI && !MatchController.Singleton;
        if (needsExtraAiPlayers)
        {
            for (var i = players.Count; i < 4; i++)
            {
                players.Add((uint)i, new PlayerDetails
                {
                    id = (uint)i,
                    localInputID = i,
                    steamID = 0,
                    name = "HCU",
                    type = PlayerType.AI,
                    color = PlayerInputManagerController.Singleton.AIColors[i - players.Count + 1],
                });
            }
        }

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

        foreach (var p in players.Values.Where(p => p.type == PlayerType.AI))
        {
            NetworkClient.Send(new SpawnPlayerMessage(p.id, PlayerType.AI));
        }
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
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
            playerIndex = 0;
        }
        Debug.Log($"Spawning player {message.id}");

        var spawnPoint = spawnPoints[playerIndex];
        playerIndex++;

        var prefabIndex = playerDetails.type is PlayerType.AI && NetworkServer.active ? AIFPSPlayerPrefabIndex : FPSPlayerPrefabIndex;
        var prefab = spawnPrefabs[prefabIndex + prefabIndexOffset];
        var player = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<PlayerManager>().id = message.id;
        if (players[message.id].localInputID == 0)
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

        var playerName = playerDetails.name;
        if (players.Values.Where(p => p.steamID == playerDetails.steamID).Count() > 1)
            playerName = $"{playerName} {playerDetails.localInputID + 1}";

        playerManager.identity.playerName = playerName;
        playerManager.identity.color = playerDetails.color;

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
            identity.playerName = playerName;
            identity.color = playerDetails.color;

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
            AIManager manager = player.GetComponent<AIManager>();
            manager.SetLayer(3);
            // TODO same as for the else clause
            playerManager.identity.SetLoadout(StaticInfo.Singleton.StartingBody, StaticInfo.Singleton.StartingBarrel,
                StaticInfo.Singleton.StartingExtension);
            manager.SetIdentity(playerManager.identity);
            manager.GetComponent<AIMovement>().SetInitialRotation(message.rotation.eulerAngles.y * Mathf.Deg2Rad);
        }
        else
        {
            // TODO: Set up networkPlayers akin to AI players (no control)
            Debug.Log($"Spawning network player {playerDetails.id}");

            // TODO set based on playerdetails (and edit playerdetails)
            playerManager.identity.SetLoadout(StaticInfo.Singleton.StartingBody, StaticInfo.Singleton.StartingBarrel,
                StaticInfo.Singleton.StartingExtension);

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
            playerManager.SetLayer(3);
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
        SpawnPlayer(connection, message, 1);
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

        var playerName = playerDetails.name;
        if (players.Values.Where(p => p.steamID == playerDetails.steamID).Count() > 1)
            playerName = $"{playerName} {playerDetails.localInputID + 1}";

        playerManager.identity.playerName = playerName;
        playerManager.identity.color = playerDetails.color;

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
            identity.playerName = playerName;
            identity.color = playerDetails.color;
            // TODO set remaining properties like chips and so on :)
        }
        else if (playerDetails.type is PlayerType.AI && NetworkServer.active)
        {

            AIManager manager = player.GetComponent<AIManager>();
            manager.SetLayer(3);
            manager.SetIdentity(manager.identity);
        }
        else
        {
            // TODO: Set up networkPlayers akin to AI players (no control)
            Debug.Log($"Spawning network player {playerDetails.id}");

            // TODO set based on playerdetails (and edit playerdetails)
            playerManager.identity.SetLoadout(StaticInfo.Singleton.StartingBody, StaticInfo.Singleton.StartingBarrel,
                StaticInfo.Singleton.StartingExtension);

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
