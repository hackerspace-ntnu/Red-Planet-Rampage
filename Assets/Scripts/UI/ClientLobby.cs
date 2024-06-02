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

    private int joinedPlayers = 0;
    private void Start()
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

        joinedPlayers++;
        if (joinedPlayers > PlayerInputManagerController.Singleton.LocalPlayerInputs.Count)
            // More than just the local players present, so the loading screen is no longer needed
            LoadingScreen.Singleton.Hide();
    }

    private void OnDestroy()
    {
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerRecieved -= AddPlayer;
    }
}
