using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public struct PlayerDetails
{
    public uint id;
    public ulong steamID;
    public PlayerType type;

    public string name;
    public Color color;

    public string body;
    public string barrel;
    public string extension;
}

public struct PlayerConnectedMessage : NetworkMessage
{
    public PlayerConnectedMessage(PlayerType type)
    {
        this.type = type;
    }

    public PlayerType type;
}

public struct InitialPlayerDetailsMessage : NetworkMessage
{
    public InitialPlayerDetailsMessage(PlayerDetails details)
    {
        this.details = details;
    }
    public PlayerDetails details;
}

public struct SpawnFPSPlayerMessage : NetworkMessage
{
    public SpawnFPSPlayerMessage(uint id)
    {
        this.id = id;
    }

    public uint id;
}

public struct InitializeFPSPlayerMessage : NetworkMessage
{
    public InitializeFPSPlayerMessage(uint id, Vector3 position, Quaternion rotation)
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
    Remote
}

public class Peer2PeerTransport : NetworkManager
{
    private PlayerFactory playerFactory;
    private static Transform[] spawnPoints;
    private static int playerIndex;
    
    private SpawnHandlerDelegate onSpawnPlayer;
    private UnSpawnDelegate onUnSpawnPlayer;
    
    private const int FPSPlayerPrefabIndex = 0;

    private readonly Dictionary<uint, PlayerDetails> players = new();
    
    private uint myId; // TODO this is inflexible for multiple local players

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnSpawnPlayerInput);
        
        
        // We don't receive a join message for the host, so it should add its own info.
        var hostDetails = new PlayerDetails
        {
            id = 0,
            type = PlayerType.Local,
            name = SteamManager.Singleton.UserName,
            color = PlayerInputManagerController.Singleton.PlayerColors[0],
        };
        players.Add(hostDetails.id, hostDetails);
    }
    
    #region Player joining 
    
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        
        NetworkClient.RegisterHandler<InitialPlayerDetailsMessage>(OnReceivePlayerDetails);
        NetworkClient.RegisterHandler<InitializeFPSPlayerMessage>(InitializeFPSPlayer);

        // TODO: A better check than this so we can have multiple local players again online
        if (NetworkServer.connections.Count == 1)
        {
            NetworkClient.Send(new PlayerConnectedMessage(PlayerType.Local));
        }
        else
        {
            NetworkClient.Send(new PlayerConnectedMessage(PlayerType.Remote));
        }
    }

    private void OnSpawnPlayerInput(NetworkConnectionToClient connection, PlayerConnectedMessage message)
    {
        PlayerInputManagerController.Singleton.NetworkClients.Add(connection);
        var index = SteamManager.Singleton.PlayerNames.Count - 1;
        var steamName = SteamManager.Singleton.PlayerNames[index];
        var steamID = SteamManager.Singleton.PlayerIDs[index];

        var player = Instantiate(playerPrefab);
        var playerInput = player.GetComponent<InputManager>();
        playerInput.PlayerCamera.enabled = false;
        playerInput.enabled = message.type is PlayerType.Local;
        NetworkServer.AddPlayerForConnection(connection, player);
        
        
        // Send information about existing players to the new one
        foreach (var existingPlayer in players.Values)
        {
            connection.Send(new InitialPlayerDetailsMessage(existingPlayer));
        }
        
        // TODO consider just putting this stuff into the InitialPlayerDetailsMessage
        var details = new PlayerDetails
        {
            id = (uint)index,
            steamID = steamID,
            type = message.type,
            name = steamName,
            color = PlayerInputManagerController.Singleton.PlayerColors[index],
        };
        // Send information about this player to all
        NetworkServer.SendToAll(new InitialPlayerDetailsMessage(details));
    }

    private void OnReceivePlayerDetails(InitialPlayerDetailsMessage message)
    {
        var details = message.details;
        if (players.ContainsKey(details.id))
            return;
        details.type = SteamManager.Singleton.SteamID.m_SteamID == details.steamID ? PlayerType.Local : PlayerType.Remote;
        if (details.type is PlayerType.Local) myId = details.id;
        Debug.Log($"Received info for player {details.id}: name={details.name} type={details.type} color={details.color}");
        players.Add(details.id, details);
    }

    #endregion
    
    #region Spawn in match

    public override void OnServerChangeScene(string newSceneName)
    {
        base.OnServerChangeScene(newSceneName);
        playerIndex = 0;

        switch (newSceneName)
        {
            case "Bidding":
                // TODO: Handle bidding properly
                NetworkServer.RegisterHandler<SpawnFPSPlayerMessage>(OnSpawnFPSPlayer);
                break;
            case "Menu":
                NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnSpawnPlayerInput);
                break;
            default:
                NetworkServer.RegisterHandler<SpawnFPSPlayerMessage>(OnSpawnFPSPlayer);
                break;
        }
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        // Remove the other playerinputs???
        foreach (var networkInput in FindObjectsOfType<PlayerInput>().Select(pi => pi.GetComponent<NetworkIdentity>())
                     .Where(id => id != null))
        {
            Destroy(networkInput);
        }
        NetworkClient.Send(new SpawnFPSPlayerMessage(myId));
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.Log("I HAVE THE POWAH");
    }

    private void OnSpawnFPSPlayer(NetworkConnectionToClient connection, SpawnFPSPlayerMessage message)
    {
        Debug.Log("Spawning player");
        if (!playerFactory)
        {
            playerFactory = FindAnyObjectByType<PlayerFactory>();
            spawnPoints = playerFactory.GetRandomSpawnpoints();
            playerIndex = 0;
        }

        var spawnPoint = spawnPoints[playerIndex];
        playerIndex++;

        var player = Instantiate(spawnPrefabs[FPSPlayerPrefabIndex], spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<PlayerManager>().id = message.id;
        NetworkServer.AddPlayerForConnection(connection, player);
        NetworkServer.SendToAll(new InitializeFPSPlayerMessage(message.id, spawnPoint.position, spawnPoint.rotation));
    }

    private void InitializeFPSPlayer(InitializeFPSPlayerMessage message)
    {
        StartCoroutine(WaitAndInitializeFPSPlayer(message));
    }

    private IEnumerator WaitAndInitializeFPSPlayer(InitializeFPSPlayerMessage message)
    {
        // Wait until things should be synced
        yield return new WaitForSeconds(.2f);

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
        
        
        playerManager.identity.playerName = playerDetails.name;
        playerManager.identity.color = playerDetails.color;
        
        player.transform.position = message.position;
        player.transform.rotation = message.rotation;

        var cameraOffset = player.transform.Find("CameraOffset");
        playerManager.GetComponent<AmmoBoxCollector>().enabled = true;

        if (playerDetails.type is PlayerType.Local)
        {
            Debug.Log($"Spawning local player {playerDetails.id}");
            var input = PlayerInputManagerController.Singleton.LocalPlayerInputs[0];
            // Make playerInput child of player it's attached to
            input.transform.parent = player.transform;
            // Set received playerInput (and most importantly its camera) at an offset from player's position
            input.transform.localPosition = cameraOffset.localPosition;
            input.transform.rotation = player.transform.rotation;

            // Enable Camera
            input.PlayerCamera.enabled = true;
            input.PlayerCamera.orthographic = false;

            input.GetComponent<PlayerIdentity>().playerName = SteamManager.Singleton.PlayerNames[0];

            playerManager.HUDController.gameObject.SetActive(true);
            var movement = player.GetComponent<PlayerMovement>();
            movement.enabled = true;

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
        // SHOULD be safe to initialize them here as this is before we spawn 'em.
        player.GetComponent<NetworkIdentity>().InitializeNetworkBehaviours();

        //TODO: Properly update MatchManager with async joined clients
        if (MatchController.Singleton)
        {
            MatchController.Singleton.Players.Add(new Player(playerManager.identity, playerManager,
                MatchController.Singleton.StartAmount));
        }
    }

    #endregion
}
