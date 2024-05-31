using Mirror;
using UnityEngine;

public class ClientLobby : MonoBehaviour
{
    [SerializeField]
    private PlayerSelectManager playerSelect;
    [SerializeField]
    private Transform environmentCamera;
    [SerializeField]
    private Transform lobbyPosition;

    void Start()
    {
        environmentCamera.position = lobbyPosition.position;
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRecieved += AddPlayer;
    }

    public void QuitLobby()
    {
        NetworkManager.singleton.StopClient();
    }

    private void AddPlayer(PlayerDetails details)
    {
        playerSelect.UpdateLobby();
    }

    private void OnDestroy()
    {
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRecieved -= AddPlayer;
    }
}
