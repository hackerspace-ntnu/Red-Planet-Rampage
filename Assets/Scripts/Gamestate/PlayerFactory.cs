using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using CollectionExtensions;
using Mirror;

public class PlayerFactory : MonoBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject playerSelectItemPrefab;

    private float spawnInterval = 0f;
    private PlayerInputManagerController playerInputManagerController;

    private const int NetworkPlayerLayer = 3;

    private void Awake()
    {
        if (PlayerInputManagerController.Singleton == null)
        {
            // We most likely started the game in the game scene, reload menu instead
            SceneManager.LoadSceneAsync(Scenes.Menu);
        }

        playerInputManagerController = PlayerInputManagerController.Singleton;

        // Enable splitscreen
        // TODO nullrefexcept
        playerInputManagerController.PlayerInputManager.DisableJoining();
        playerInputManagerController.PlayerInputManager.splitScreen = true;
    }

    #region Weapon construction spawning

    public void InstantiatePlayerSelectItems()
    {
        playerInputManagerController.ChangeInputMaps("Menu");
        InstantiateInputsOnSpawnpoints(InstantiateItemSelectPlayer);
    }

    private List<PlayerManager> InstantiateInputsOnSpawnpoints(Func<InputManager, Transform, PlayerManager> instantiate)
    {
        var shuffledSpawnPoints = spawnPoints.ShuffledCopy();

        var playerList = new List<PlayerManager>();
        for (int i = 0; i < playerInputManagerController.LocalPlayerInputs.Count; i++)
        {
            playerList.Add(instantiate(playerInputManagerController.LocalPlayerInputs[i], shuffledSpawnPoints[i % spawnPoints.Length]));
        }
        return playerList;
    }

    public Transform[] GetRandomSpawnpoints()
    {
        return spawnPoints.ShuffledCopy();
    }

    private PlayerManager InstantiateItemSelectPlayer(InputManager inputManager, Transform spawnPoint)
    {
        inputManager.PlayerCamera.enabled = true;
        inputManager.PlayerCamera.orthographic = true;
        spawnInterval += 10000f;
        GameObject player = Instantiate(playerSelectItemPrefab, spawnPoint.position + new Vector3(spawnInterval, spawnInterval, 0), spawnPoint.rotation);
        inputManager.transform.position = player.GetComponent<ItemSelectMenu>().CameraPosition.transform.position;
        StartCoroutine(player.GetComponent<ItemSelectMenu>().SpawnItems(inputManager));
        return null;
    }

    #endregion


    #region Initialization

    public static void InitializePlayer(InitializePlayerMessage message, PlayerManager player, PlayerDetails playerDetails, bool isBidding = false)
    {
        player.GetComponent<AmmoBoxCollector>().enabled = true;

        player.transform.position = message.position;
        player.transform.rotation = message.rotation;

        Debug.Log($"Spawning {playerDetails.type} player {playerDetails.id}");

        if (isBidding)
            InitializeBiddingPlayer(message, player, playerDetails);
        else
            InitializeFPSPlayer(message, player, playerDetails);

        player.ApplyIdentity();

        // This ensures that behaviours on the gun have identities.
        // SHOULD be safe to initialize them here as this is at roughly the same point on all clients
        player.GetComponent<NetworkIdentity>().InitializeNetworkBehaviours();

        if (MatchController.Singleton)
        {
            MatchController.Singleton.RegisterPlayer(player);
        }
    }

    private static void InitializeFPSPlayer(InitializePlayerMessage message, PlayerManager player, PlayerDetails playerDetails)
    {
        var cameraOffset = player.transform.Find("CameraOffset");
        if (playerDetails.type is PlayerType.Local)
        {
            var input = SetupLocalPlayerInput(player, playerDetails);

            // Enable Camera
            input.PlayerCamera.enabled = true;
            input.PlayerCamera.orthographic = false;

            player.HUDController.gameObject.SetActive(true);
            var movement = player.GetComponent<PlayerMovement>();

            // Update player's movement script with which playerInput it should attach listeners to
            var gunHolder = input.transform.GetChild(0);
            player.SetGun(gunHolder);

            // Set unique layer for player
            player.SetLayer(input.playerInput.playerIndex);
            movement.SetInitialRotation(message.rotation.eulerAngles.y * Mathf.Deg2Rad);

            if (GunFactory.TryGetGunAchievement(player.identity.Body, player.identity.Barrel,
                    player.identity.Extension, out var achievement))
                SteamManager.Singleton.UnlockAchievement(achievement);
        }
        else if (playerDetails.type is PlayerType.AI && NetworkServer.active)
        {
            InitializeAIPlayer(message, player, playerDetails);
        }
        else
        {
            player.identity.UpdateFromDetails(playerDetails);

            // TODO do some other version of disabling HUD completely
            Destroy(player.HUDController);

            // Disable physics
            player.GetComponent<Rigidbody>().isKinematic = true;

            // Create display gun structure
            var gunHolderParent = new GameObject("DisplayGunParent").transform;
            gunHolderParent.parent = player.transform;
            gunHolderParent.position = cameraOffset.position;
            gunHolderParent.rotation = player.transform.rotation;
            var gunHolder = new GameObject("DisplayGunHolder").transform;
            gunHolder.parent = gunHolderParent.transform;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
            player.SetLayer(NetworkPlayerLayer);
            // Can't initialize quite like the AIs because of where the GunController network behaviour is located :(
            player.SetGun(gunHolder);
        }
    }

    private static void InitializeBiddingPlayer(InitializePlayerMessage message, PlayerManager player, PlayerDetails playerDetails)
    {
        if (playerDetails.type is PlayerType.Local)
        {
            var input = SetupLocalPlayerInput(player, playerDetails);

            // Disable camera
            input.PlayerCamera.enabled = false;

            // Disable health
            player.GetComponent<HealthController>().enabled = false;
        }
        else if (playerDetails.type is PlayerType.AI && NetworkServer.active)
        {
            InitializeAIPlayer(message, player, playerDetails);
        }
        else
        {
            player.identity.UpdateFromDetails(playerDetails);

            // Disable physics
            player.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private static InputManager SetupLocalPlayerInput(PlayerManager player, PlayerDetails playerDetails)
    {
        var input = PlayerInputManagerController.Singleton.LocalPlayerInputs[playerDetails.localInputID];
        var cameraOffset = player.transform.Find("CameraOffset");

        // Reset camera transform (it may have been kerfluffled by the spectator cam thingy)
        input.transform.localPosition = Vector3.zero;
        input.transform.localRotation = Quaternion.identity;
        input.PlayerCamera.transform.localRotation = Quaternion.identity;
        input.PlayerCamera.transform.localPosition = Vector3.zero;

        // Make playerInput child of player it's attached to
        input.transform.parent = player.transform;
        // Set received playerInput (and most importantly its camera) at an offset from player's position
        input.transform.localPosition = cameraOffset.localPosition;
        input.transform.rotation = player.transform.rotation;

        // The identity sits on the input in this case, so edit that
        var identity = input.GetComponent<PlayerIdentity>();
        identity.UpdateFromDetails(playerDetails);

        player.SetPlayerInput(input);

        return input;
    }

    private static void InitializeAIPlayer(InitializePlayerMessage message, PlayerManager player, PlayerDetails playerDetails)
    {
        AIManager manager = player.GetComponent<AIManager>();
        manager.SetLayer(NetworkPlayerLayer);
        player.identity.UpdateFromDetails(playerDetails);
        manager.SetIdentity(player.identity);
        manager.GetComponent<AIMovement>().SetInitialRotation(message.rotation.eulerAngles.y * Mathf.Deg2Rad);
    }

    #endregion
}
