using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerFactory : MonoBehaviour
{

    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject playerSelectItemPrefab;
    [SerializeField]
    private GameObject aIOpponent;
    [SerializeField]
    private GameObject aIBidder;
    [SerializeField]
    private GameObject aiIdentity;
    private List<PlayerIdentity> existingAiIdentities;
    private float spawnInterval = 0f;
    private PlayerInputManagerController playerInputManagerController;

    [SerializeField]
    private GlobalHUDController globalHUDController;
    [SerializeField]
    private bool overrideMatchManager = false;

    private static readonly System.Random random = new System.Random();

    private void Awake()
    {
        if (PlayerInputManagerController.Singleton == null)
        {
            // We most likely started the game in the game scene, reload menu instead
            SceneManager.LoadSceneAsync("Menu");
            return;
        }

        playerInputManagerController = PlayerInputManagerController.Singleton;

        // Enable splitscreen
        playerInputManagerController.playerInputManager.DisableJoining();
        playerInputManagerController.playerInputManager.splitScreen = true;

        existingAiIdentities = FindObjectsOfType<PlayerIdentity>()
            .Where(identity => identity.IsAI).ToList();

        if (!overrideMatchManager)
            return;

        playerInputManagerController.playerInputs.ForEach(input => input.GetComponent<PlayerIdentity>().resetItems());
        InstantiatePlayersFPS();
    }

    public List<PlayerManager> InstantiatePlayersFPS(int aiPlayerCount = 0)
    {
        playerInputManagerController.ChangeInputMaps("FPS");
        return InstantiateInputsOnSpawnpoints(InstantiateFPSPlayer, InstantiateFPSAI, aiPlayerCount);
    }

    public void InstantiatePlayersBidding(int aiPLayerCount = 0)
    {
        playerInputManagerController.ChangeInputMaps("Bidding");
        InstantiateInputsOnSpawnpoints(InstantiateBiddingPlayer, InstantiateBiddingAI, aiPLayerCount);
    }
    public void InstantiatePlayerSelectItems()
    {
        playerInputManagerController.ChangeInputMaps("Menu");
        InstantiateInputsOnSpawnpoints(InstantiateItemSelectPlayer);
    }

    private List<PlayerManager> InstantiateInputsOnSpawnpoints(Func<InputManager, Transform, PlayerManager> instantiate, Func<int, Transform, AIManager> instantiateAI = null, int aiPlayerCount = 0)
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
        var playerList = new List<PlayerManager>();
        for (int i = 0; i < playerInputManagerController.playerInputs.Count; i++)
        {
            playerList.Add(instantiate(playerInputManagerController.playerInputs[i], shuffledSpawnPoints[i % spawnPoints.Length]));
        }
        for (int i = playerInputManagerController.playerInputs.Count; i < playerInputManagerController.playerInputs.Count + aiPlayerCount; i++)
        {
            var spawnPoint = shuffledSpawnPoints[i % spawnPoints.Length];
            var aiPlayer = instantiateAI(i, spawnPoint);

            playerList.Add(aiPlayer);
        }
        return playerList;
    }


    /// <summary>
    /// Spawns a playerPrefab and attaches a playerInput to it as a child.
    /// This function is where you should add delegate events for them to be properly invoked.
    /// </summary>
    /// <param name="inputManager">PlayerInput to tie the player prefab to.</param>
    private PlayerManager InstantiateFPSPlayer(InputManager inputManager, Transform spawnPoint)
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
        playerManager.GetComponent<PlayerMovement>().SetInitialRotation(spawnPoint.eulerAngles.y * Mathf.Deg2Rad);
        return playerManager;
    }

    private PlayerManager InstantiateBiddingPlayer(InputManager inputManager, Transform spawnPoint)
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
        player.GetComponent<HealthController>().enabled = false;
        return playerManager;
    }

    private PlayerManager InstantiateItemSelectPlayer(InputManager inputManager, Transform spawnPoint)
    {
        inputManager.GetComponent<Camera>().enabled = true;
        spawnInterval += 10000f;
        GameObject player = Instantiate(playerSelectItemPrefab, spawnPoint.position + new Vector3(spawnInterval, spawnInterval, 0), spawnPoint.rotation);
        Camera playerCamera = inputManager.GetComponent<Camera>();
        playerCamera.transform.position = player.GetComponent<ItemSelectMenu>().CameraPosition.transform.position;
        StartCoroutine(player.GetComponent<ItemSelectMenu>().SpawnItems(inputManager));
        return null;
    }

    private AIManager InstantiateFPSAI(int index, Transform spawnPoint)
    {

        PlayerIdentity identity = null;

        if (existingAiIdentities.Count == 0)
        {
            var identityObject = Instantiate(aiIdentity);
            identity = identityObject.GetComponent<PlayerIdentity>();
            DontDestroyOnLoad(identityObject);
        }

        var aiOpponent = Instantiate(aIOpponent, spawnPoint.position, spawnPoint.rotation);
        AIManager manager = aiOpponent.GetComponent<AIManager>();
        manager.SetLayer(index);
        manager.SetIdentity(identity ? identity : existingAiIdentities[index - playerInputManagerController.playerInputs.Count]);
        return manager;
    }

    private AIManager InstantiateBiddingAI(int index, Transform spawnPoint)
    {
        var aiOpponent = Instantiate(aIBidder, spawnPoint.position, spawnPoint.rotation);
        AIManager manager = aiOpponent.GetComponent<AIManager>();
        manager.SetLayer(index);
        manager.SetIdentity(existingAiIdentities[index - playerInputManagerController.playerInputs.Count]);
        return manager;
    }
}
