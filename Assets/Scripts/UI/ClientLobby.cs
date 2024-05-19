using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientLobby : MonoBehaviour
{
    [SerializeField]
    private PlayerSelectManager playerSelect;
    [SerializeField]
    private Transform environmentCamera;
    [SerializeField]
    private Transform lobbyPosition;
    private int playerCount = 0;

    void Start()
    {
        environmentCamera.position = lobbyPosition.position;
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRecieved += AddPlayer;
    }

    public void QuitLobby()
    {
        Peer2PeerTransport.singleton.StopClient();
    }

    private void AddPlayer(PlayerDetails details)
    {
        playerSelect.SetupPlayerSelectModels(details.name, details.color, playerCount);
        playerCount++;
    }
}
