using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Selectable initiallySelectedButton;

    private PlayerManager player;

    private InputManager input;
    
    public void SetPlayerInput(InputManager input, PlayerManager player)
    {
        this.player = player;
        this.input = input;
        gameObject.SetActive(true);

        var playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.ShouldNotRespondToInputs = true;
        playerMovement.ResetZoom();
        

        EventSystem.current.SetSelectedGameObject(initiallySelectedButton.gameObject);
        PlayerInputManagerController.Singleton.ChangeInputMapForPlayer("Menu", input);
        input.onExit += Resume;
        input.onCancel += Resume;
    }

    public void Resume()
    {
        gameObject.SetActive(false);
        PlayerInputManagerController.Singleton.ChangeInputMapForPlayer("FPS", input);
        player.ReassignPlayerInput(input);
        player.GunController.SetPlayerInput(input);
        var playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.ShouldNotRespondToInputs = false;
    }

    private void Resume(InputAction.CallbackContext ctx)
    {
        Resume();
    }

    public void Leave()
    {
        MatchController.Singleton.ReturnToMainMenu();
    }
}
