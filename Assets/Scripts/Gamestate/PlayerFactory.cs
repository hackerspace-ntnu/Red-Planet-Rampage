using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerFactory : MonoBehaviour
{

    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Transform spawnPoint;

    private PlayerInputManagerController playerInputManagerController;

    [SerializeField]
    private GlobalHUDController globalHUDController;

    private void Awake()
    {

        playerInputManagerController = PlayerInputManagerController.Singleton;

        // Enable splitscreen
        playerInputManagerController.playerInputManager.DisableJoining();
        playerInputManagerController.playerInputManager.splitScreen = true;
    }

    public void InstantiatePlayersFPS()
    {
        playerInputManagerController.ChangeInputMaps("FPS");
        foreach (InputManager inputs in playerInputManagerController.playerInputs)
        {
            inputs.RemoveListeners();
            InstantiateFPSPlayer(inputs);
            inputs.AddListeners();
        }
    }

    public void InstantiatePlayersBidding()
    {
        playerInputManagerController.ChangeInputMaps("Bidding");
        foreach (InputManager inputs in playerInputManagerController.playerInputs)
        {
            inputs.RemoveListeners();
            InstantiateBiddingPlayer(inputs);
            inputs.AddListeners();
        }
    }

    /// <summary>
    /// Spawns a playerPrefab and attaches a playerInput to it as a child.
    /// This function is where you should add delegate events for them to be properly invoked.
    /// </summary>
    /// <param name="inputManager">PlayerInput to tie the player prefab to.</param>
    private void InstantiateFPSPlayer(InputManager inputManager)
    {
        // Spawn player at spawnPoint's position with spawnPoint's rotation
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        Transform cameraOffset = player.transform.Find("CameraOffset");
        // Make playerInput child of player it's attached to
        inputManager.transform.parent = player.transform;
        // Set recieved playerInput (and most importantly its camera) at an offset from player's position
        inputManager.transform.localPosition = cameraOffset.localPosition;
        inputManager.transform.rotation = player.transform.rotation;
        // Enable Camera
        inputManager.GetComponent<Camera>().enabled = true;
        // Update player's movement script with which playerInput it should attach listeners to
        var playerManager = player.GetComponent<PlayerManager>();
        playerManager.SetPlayerInput((FPSInputManager) inputManager);
        // Set unique layer for player
        playerManager.SetLayer(inputManager.playerInput.playerIndex);
    }

    private void InstantiateBiddingPlayer(InputManager inputManager)
    {
        // Spawn player at spawnPoint's position with spawnPoint's rotation
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        // Make playerInput child of player it's attached to
        inputManager.transform.parent = player.transform;
        // Disable Camera
        inputManager.GetComponent<Camera>().enabled = false;
        // Update player's movement script with which playerInput it should attach listeners to
        var playerManager = player.GetComponent<PlayerManager>();
        playerManager.SetPlayerInput((FPSInputManager) inputManager);
        // Add player UI to globalUI
        globalHUDController.SetPlayer(playerManager);
    }
}
