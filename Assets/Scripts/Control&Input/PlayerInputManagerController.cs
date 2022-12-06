using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerController : MonoBehaviour
{
    public static PlayerInputManagerController Singleton { get; private set; }

    public List<InputManager> playerInputs = new List<InputManager>();

    public PlayerInputManager playerInputManager;

    [SerializeField]
    private Color[] playerColors;

    [SerializeField]
    private string[] playerNames;

    public delegate void JoinEvent(InputManager inputManager);

    public JoinEvent onPlayerInputJoined;

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

    public void RemoveListeners()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        var playerIdentity = playerInput.GetComponent<PlayerIdentity>();
        playerIdentity.color = playerColors[playerInputs.Count - 1];
        playerIdentity.playerName = playerNames[playerInputs.Count - 1];
        InputManager inputManager = playerInput.GetComponent<InputManager>();
        playerInputs.Add(inputManager);
        onPlayerInputJoined(inputManager);
    }

    /// <summary>
    /// Changes the inputMap of all current playerInputs to specified inputMap
    /// </summary>
    /// <param name="mapNameOrId">Name of the inputMap you want to change to</param>
    public void ChangeInputMaps(string mapNameOrId)
    {
        foreach (InputManager input in playerInputs)
        {
            input.playerInput.SwitchCurrentActionMap(mapNameOrId);
        }
    }
}
