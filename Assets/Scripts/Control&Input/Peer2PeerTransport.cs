using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CollectionExtensions;

using Mirror;
using UnityEngine;

public struct PlayerInfo : NetworkMessage
{
    public PlayerInfo(PlayerType type)
    {
        this.type = type; 
    }

    public PlayerType type;
}

public struct PlayerFPSInfo : NetworkMessage
{
    public PlayerFPSInfo(PlayerType type)
    {
        this.type = type;
    }
    // TODO: Add stuff like colors and other identity parameters
    public PlayerType type;
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

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerInfo>(OnSpawnPlayerInput);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();
        // TODO: A better check than this so we can have multiple local players again online
        if (NetworkServer.connections.Count == 1)
        {
            NetworkClient.Send(new PlayerInfo(PlayerType.Local));
        }
        else
        {
            NetworkClient.Send(new PlayerInfo(PlayerType.Remote));
        }
    }

    private void OnSpawnPlayerInput(NetworkConnectionToClient connection, PlayerInfo info)
    {

        PlayerInputManagerController.Singleton.NetworkClients.Add(connection);
        var steamName = SteamManager.Singleton.PlayerNames[SteamManager.Singleton.PlayerNames.Count - 1];
        SteamManager.Singleton.PlayerDictionary.Add(connection.connectionId, (steamName, SteamManager.Singleton.PlayerNames.Count - 1));
        if (info.type == PlayerType.Local)
        {
            GameObject player = Instantiate(playerPrefab);
            player.GetComponent<InputManager>().PlayerCamera.enabled = false;
            NetworkServer.AddPlayerForConnection(connection, player);
        }
        else
        {
            GameObject player = Instantiate(playerPrefab);
            var playerInput = player.GetComponent<InputManager>();
            playerInput.PlayerCamera.enabled = false;
            playerInput.enabled = false;
            NetworkServer.AddPlayerForConnection(connection, player);
        }
            
    }

    public override void OnServerChangeScene(string newSceneName)
    {
        base.OnServerChangeScene(newSceneName);
        switch (newSceneName)
        {
            case "Bidding":
                // TODO: Handle bidding properly
                NetworkServer.RegisterHandler<PlayerFPSInfo>(OnSpawnFPSPlayers);
                break;
            case "Menu":
                NetworkServer.RegisterHandler<PlayerInfo>(OnSpawnPlayerInput);
                break;
            default:
                NetworkServer.RegisterHandler<PlayerFPSInfo>(OnSpawnFPSPlayers);
                break;
        }
        
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
        NetworkClient.Send(new PlayerFPSInfo(PlayerType.Remote));
    }

    private void OnSpawnFPSPlayers(NetworkConnectionToClient connection, PlayerFPSInfo info)
    {
        if (!playerFactory)
        {
            playerFactory = FindAnyObjectByType<PlayerFactory>();
            spawnPoints = playerFactory.GetRandomSpawnpoints();
            playerIndex = 0;
        }
        var spawn = spawnPoints[playerIndex];
        playerIndex++;
        var player = Instantiate(spawnPrefabs[0], spawn.position, spawn.rotation);
        if (!player)
            return;
        var playerManager = player.GetComponent<PlayerManager>();
        bool isLocalClient = false;
        string playerName = "";
        if (SteamManager.Singleton.PlayerDictionary.TryGetValue(connection.connectionId, out var playerDetails))
        {
            playerManager.identity.playerName = playerDetails.Item1;
            isLocalClient = playerDetails.Item1.Equals(SteamManager.Singleton.UserName);
            playerName = playerDetails.Item1;
            playerManager.identity.color = PlayerInputManagerController.Singleton.PlayerColors[playerDetails.Item2];
        }
        //NetworkServer.AddPlayerForConnection(connection, playerManager.gameObject);
        player.transform.position = spawn.position;
        player.transform.rotation = spawn.rotation;
        if (isLocalClient)
        {
            Debug.Log("Is local!!");
            Transform cameraOffset = player.transform.Find("CameraOffset");
            var input = PlayerInputManagerController.Singleton.LocalPlayerInputs[0];
            // Make playerInput child of player it's attached to
            input.transform.parent = player.transform;
            // Set recieved playerInput (and most importantly its camera) at an offset from player's position
            input.transform.localPosition = cameraOffset.localPosition;
            input.transform.rotation = player.transform.rotation;

            // Enable Camera
            input.PlayerCamera.enabled = true;
            input.PlayerCamera.orthographic = false;

            input.GetComponent<PlayerIdentity>().playerName = SteamManager.Singleton.PlayerNames[0];

            playerManager.GetComponent<AmmoBoxCollector>().enabled = true;
            playerManager.HUDController.gameObject.SetActive(true);
            var movement = player.GetComponent<PlayerMovement>();
            movement.enabled = true;

            // Update player's movement script with which playerInput it should attach listeners to
            playerManager.SetPlayerInput(input);
            playerManager.SetGun(input.transform.GetChild(0));

            if (GunFactory.TryGetGunAchievement(playerManager.identity.Body, playerManager.identity.Barrel, playerManager.identity.Extension, out var achievement))
                SteamManager.Singleton.UnlockAchievement(achievement);

            // Set unique layer for player
            playerManager.SetLayer(input.playerInput.playerIndex);
            movement.SetInitialRotation(spawn.eulerAngles.y * Mathf.Deg2Rad);
            NetworkServer.Spawn(player, connection);
        }
        else
        {
            // TODO: Set up networkPlayers akin to AI players (no control)
            Debug.Log("Not local!");
            if (!playerName.Equals(""))
                playerManager.identity.playerName = playerName;
        }
        //TODO: Properly update MatchManager with async joined clients
        MatchController.Singleton?.Players.Add(new Player(playerManager.identity, playerManager, MatchController.Singleton.StartAmount));
    }
}
