using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFactory : MonoBehaviour
{

    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject playerSelectItemPrefab;
    private PlayerInputManagerController playerInputManagerController;

    [SerializeField]
    private GlobalHUDController globalHUDController;

    private static readonly System.Random random = new System.Random();

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
        InstantiateInputsOnSpawnpoints(InstantiateFPSPlayer);
    }

    public void InstantiatePlayersBidding()
    {
        playerInputManagerController.ChangeInputMaps("Bidding");
        InstantiateInputsOnSpawnpoints(InstantiateBiddingPlayer);
    }
    public void InstantiatePlayerSelectItems()
    {
        playerInputManagerController.ChangeInputMaps("Menu");
        InstantiateInputsOnSpawnpoints(InstantiateItemSelectPlayer);

    }

    private void InstantiateInputsOnSpawnpoints(Action<InputManager, Transform> instantiate)
    {
        var shuffledSpawnPoints = new List<Transform>(spawnPoints);
        // Fisher-Yates shuffle
        for (int i = spawnPoints.Length - 1; i > 0; i--)
        {
            var k = random.Next(i);
            var firstSwapped = shuffledSpawnPoints[i];
            shuffledSpawnPoints[i] = shuffledSpawnPoints[k];
            shuffledSpawnPoints[k] = firstSwapped;
        }
        for (int i = 0; i < playerInputManagerController.playerInputs.Count; i++)
        {
            instantiate(playerInputManagerController.playerInputs[i], shuffledSpawnPoints[i % spawnPoints.Length]);
        }
    }


    /// <summary>
    /// Spawns a playerPrefab and attaches a playerInput to it as a child.
    /// This function is where you should add delegate events for them to be properly invoked.
    /// </summary>
    /// <param name="inputManager">PlayerInput to tie the player prefab to.</param>
    private void InstantiateFPSPlayer(InputManager inputManager, Transform spawnPoint)
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
        playerManager.SetPlayerInput(inputManager);
        playerManager.SetGun(inputManager.transform);
        // Set unique layer for player
        playerManager.SetLayer(inputManager.playerInput.playerIndex);
    }

    private void InstantiateBiddingPlayer(InputManager inputManager, Transform spawnPoint)
    {
        // Spawn player at spawnPoint's position with spawnPoint's rotation
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        Transform cameraOffset = player.transform.Find("CameraOffset");
        // Make playerInput child of player it's attached to
        inputManager.transform.parent = player.transform;
        // Make playerInput (with the gun) get placed at correct position
        inputManager.transform.localPosition = cameraOffset.localPosition;
        inputManager.transform.rotation = player.transform.rotation;
        // Disable Camera
        inputManager.GetComponent<Camera>().enabled = false;
        // Update player's movement script with which playerInput it should attach listeners to
        var playerManager = player.GetComponent<PlayerManager>();
        playerManager.SetPlayerInput(inputManager);
        // Add player UI to globalUI
        globalHUDController.SetPlayer(playerManager);
        player.GetComponent<HealthController>().enabled = false;
    }
    private void InstantiateItemSelectPlayer(InputManager inputManager, Transform spawnPoint)
    {
        Debug.Log("player instantiated");
        GameObject player = Instantiate(playerSelectItemPrefab, spawnPoint.position + new Vector3(1000f,1000f,0), spawnPoint.rotation);
        Debug.Log(player);


    }
}
