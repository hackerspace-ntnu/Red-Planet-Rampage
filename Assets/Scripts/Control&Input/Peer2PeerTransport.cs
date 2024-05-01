using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

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

public enum PlayerType
{
    Local,
    Remote
}

public class Peer2PeerTransport : NetworkManager
{
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
        // TODO: Edit joined players manually here (steamname, colors, numbers etc)
        if (info.type == PlayerType.Local)
        {
            Debug.Log(PlayerInputManagerController.Singleton.LocalPlayerInputs[0].gameObject.name);
            GameObject player = Instantiate(playerPrefab);
            player.GetComponent<InputManager>().PlayerCamera.enabled = false;
            NetworkServer.AddPlayerForConnection(connection, player);
            //NetworkServer.ReplacePlayerForConnection(connection, PlayerInputManagerController.Singleton.playerInputs[0].gameObject);
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
}
