using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenuGate : MonoBehaviour, Interactable
{
    public void Interact(PlayerManager player)
    {
        ReturnToMainMenu();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var contact = collision.GetContact(0);
        if (contact.otherCollider.TryGetComponent<PlayerManager>(out var _))
            ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        PlayerInputManagerController.Singleton.ChangeInputMaps("Menu");
        PlayerInputManagerController.Singleton.LocalPlayerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().resetItems());
        if (!SteamManager.Singleton.ChangeScene("Menu"))
            SceneManager.LoadSceneAsync("Menu");
    }
}
