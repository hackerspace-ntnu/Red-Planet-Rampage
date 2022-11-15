using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFactory : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Transform spawnPoint;

    private PlayerInputManagerController playerInputManagerController;

    private void Awake()
    {
        playerInputManagerController = PlayerInputManagerController.Singleton;
        TransferExistingInputs();

        // Enable splitscreen
        playerInputManagerController.playerInputManager.DisableJoining();
        playerInputManagerController.playerInputManager.splitScreen = true;
    }

    /// <summary>
    /// Updates playerInputs to use FPS-related actionMap + update eventlisteners
    /// </summary>
    private void TransferExistingInputs()
    {
        playerInputManagerController.ChangeInputMaps("FPS");
        foreach (PlayerInput inputs in playerInputManagerController.playerInputs)
        {
            inputs.GetComponent<InputManager>().RemoveListeners();
            InstantiatePlayer(inputs);
            inputs.GetComponent<InputManager>().AddListeners();
        }
    }

    /// <summary>
    /// Spawns a playerPrefab and attaches a playerInput to it as a child.
    /// This function is where you should add delegate events for them to be properly invoked.
    /// </summary>
    /// <param name="playerInput">PlayerInput to tie the player prefab to.</param>
    private void InstantiatePlayer(PlayerInput playerInput)
    {
        // Spawn player at spawnPoint's position with spawnPoint's rotation
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        Transform cameraOffset = player.transform.Find("CameraOffset");
        // Make playerInput child of player it's attached to
        playerInput.transform.parent = player.transform;
        // Set recieved playerInput (and most importantly its camera) at an offset from player's position
        playerInput.transform.localPosition = cameraOffset.localPosition;
        playerInput.transform.rotation = player.transform.rotation;
        // Enable Camera
        playerInput.GetComponent<Camera>().enabled = true;
        // Update player's movement script with which playerInput it should attach listeners to
        player.GetComponent<PlayerMovement>().SetPlayerInput(playerInput.GetComponent<FPSInputManager>());
        var playerManager = player.GetComponent<PlayerManager>();
        playerManager.SetPlayerInput(playerInput.GetComponent<FPSInputManager>());
        // Set unique layer for player
        playerManager.SetLayer(playerInput.playerIndex);
    }
}
