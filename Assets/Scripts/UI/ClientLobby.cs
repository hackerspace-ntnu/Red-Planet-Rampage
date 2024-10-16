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
        EventLog.Singleton.transform.GetChild(0).gameObject.SetActive(true);
        environmentCamera.position = lobbyPosition.position;
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerReceived += AddPlayer;
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
        ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerReceived -= AddPlayer;
    }
}
