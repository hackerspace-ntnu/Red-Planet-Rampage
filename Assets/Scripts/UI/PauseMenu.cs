using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField]
    private RectTransform panel;

    [SerializeField]
    private Button buttonToFocus;

    /// <summary>
    /// Set in Start so not quite a singleton.
    /// </summary>
    public static PauseMenu Current { get; private set; }

    private void Start()
    {
        foreach (var input in PlayerInputManagerController.Singleton.LocalPlayerInputs)
        {
            input.onExit += Open;
        }
        Current = this;
    }

    private void OnDestroy()
    {
        foreach (var input in PlayerInputManagerController.Singleton.LocalPlayerInputs)
        {
            input.onExit -= Open;
            input.onExit -= Continue;
            input.onCancel -= Continue;
        }
    }


    public void Open(InputAction.CallbackContext ctx)
    {
        panel.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(buttonToFocus.gameObject);

        // TODO: Make mouse visible and interact with buttons.
        // To achieve this the globalHUD needs to assign it's canvas to the player's camera.
        //Cursor.visible = PlayerInputManagerController.Singleton.LocalPlayerInputs.Any(i => i.IsMouseAndKeyboard);
        //Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;

        foreach (var player in Peer2PeerTransport.LocalPlayerInstances)
        {
            player.inputManager.BackupListeners();
            PlayerInputManagerController.Singleton.ChangeInputMapForPlayer("Menu", player.inputManager);
            var playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement.CanMove = false;
            playerMovement.CanLook = false;
            playerMovement.ResetZoom();
            player.inputManager.onExit += Continue;
            player.inputManager.onCancel += Continue;
        }
    }

    private void Continue(InputAction.CallbackContext ctx) =>
        Continue();

    public void Continue()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        panel.gameObject.SetActive(false);

        var inputMap = SceneManager.GetActiveScene().name == Scenes.Bidding ? "Bidding" : "FPS";
        foreach (var player in Peer2PeerTransport.LocalPlayerInstances)
        {
            PlayerInputManagerController.Singleton.ChangeInputMapForPlayer(inputMap, player.inputManager);
            player.inputManager.RestoreListeners();
            var playerMovement = player.GetComponent<PlayerMovement>();
            playerMovement.CanMove = true;
            playerMovement.CanLook = true;
        }
    }

    public void Leave()
    {
        NetworkManager.singleton.StopHost();
    }
}
