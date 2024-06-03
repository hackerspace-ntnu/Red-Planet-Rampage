using Mirror;
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
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
        PlayerInputManagerController.Singleton.LocalPlayerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().ResetItems());
        // Mirror pulls us to the main menu automatically
        NetworkManager.singleton.StopHost();
    }
}
