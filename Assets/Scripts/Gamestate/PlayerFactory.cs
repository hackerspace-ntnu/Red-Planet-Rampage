using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using CollectionExtensions;

public class PlayerFactory : MonoBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject playerSelectItemPrefab;
    private float spawnInterval = 0f;
    private PlayerInputManagerController playerInputManagerController;

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

    public void InstantiatePlayerSelectItems()
    {
        playerInputManagerController.ChangeInputMaps("MenuSelect");
        InstantiateInputsOnSpawnpoints(InstantiateItemSelectPlayer);
    }

    // TODO remove this from its mortal coil, we don't really use PlayerFactory anymore :)
    private List<PlayerManager> InstantiateInputsOnSpawnpoints(Func<InputManager, Transform, PlayerManager> instantiate, Func<int, Transform, AIManager> instantiateAI = null, int aiPlayerCount = 0)
    {
        var shuffledSpawnPoints = spawnPoints.ShuffledCopy();

        var playerList = new List<PlayerManager>();
        for (int i = 0; i < playerInputManagerController.LocalPlayerInputs.Count; i++)
        {
            var input = playerInputManagerController.LocalPlayerInputs[i];
            // Note that accessing the PauseMenu instance this way is fine here
            // since we're in an auction and the instance will be set by this point.
            input.onExit += PauseMenu.Current.Open;
            playerList.Add(instantiate(input, shuffledSpawnPoints[i % spawnPoints.Length]));
        }
        for (int i = playerInputManagerController.LocalPlayerInputs.Count; i < playerInputManagerController.LocalPlayerInputs.Count + aiPlayerCount; i++)
        {
            var spawnPoint = shuffledSpawnPoints[i % spawnPoints.Length];
            var aiPlayer = instantiateAI(i, spawnPoint);

            playerList.Add(aiPlayer);
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
}
