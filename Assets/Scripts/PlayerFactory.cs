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
    private PlayerInputManager playerInputManager;


    private void Awake()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += InstantiatePlayer;
    }

    private void OnDestroy()
    {
        playerInputManager.onPlayerJoined -= InstantiatePlayer;
    }

    private void InstantiatePlayer(PlayerInput playerInput)
    {
        // Spawn player at spawnPoint's position with spawnPoint's rotation
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        // Set recieved playerInput (and most importantly it's camera) at player's position with player's rotation
        playerInput.transform.position = player.transform.position;
        playerInput.transform.rotation = player.transform.rotation;
        // Make playerInput child of player it's attached to
        playerInput.transform.parent = player.transform;
        // Update player's movement script with which playerInput it should attach listeners to
        player.GetComponent<PlayerMovement>().SetPlayerInput(playerInput.GetComponent<FPSInputManager>());
    }
}
