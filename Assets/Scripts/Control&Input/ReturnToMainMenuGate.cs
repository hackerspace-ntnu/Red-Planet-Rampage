using Mirror;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenuGate : MonoBehaviour, Interactable
{
    public void Interact(PlayerManager player)
    {
        ReturnToMainMenu();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerManager>(out var _))
            ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        // Mirror pulls us to the main menu automatically
        RPRNetworkManager.LocalPlayerInstances.ToList().ForEach(player => player.inputManager.PlayerCamera.enabled = false);
        NetworkManager.singleton.StopHost();
    }
}
