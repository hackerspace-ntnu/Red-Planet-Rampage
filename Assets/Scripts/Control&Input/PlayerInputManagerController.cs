using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerController : MonoBehaviour
{
    public static PlayerInputManagerController Singleton { get; private set; }

    public List<PlayerInput> playerInputs = new List<PlayerInput>();

    public PlayerInputManager playerInputManager;

    void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate
        playerInputManager = PlayerInputManager.instance;
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInputs.Add(playerInput);
    }

    /// <summary>
    /// Changes the inputMap of all current playerInputs to specified inputMap
    /// </summary>
    /// <param name="mapNameOrId">Name of the inputMap you want to change to</param>
    public void ChangeInputMaps(string mapNameOrId)
    {
        foreach (PlayerInput input in playerInputs)
        {
            input.SwitchCurrentActionMap(mapNameOrId);
        }
    }
}
