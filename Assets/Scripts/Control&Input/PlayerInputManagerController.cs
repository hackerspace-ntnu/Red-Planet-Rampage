using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerController : MonoBehaviour
{
    public static PlayerInputManagerController Singleton { get; private set; }

    public List<InputManager> LocalPlayerInputs = new List<InputManager>();
    public List<PlayerIdentityNetwork> NetworkClients = new List<PlayerIdentityNetwork>();
    public int PlayerCount => NetworkClients.Count > 0 ? NetworkClients.Count : LocalPlayerInputs.Count;

    public PlayerInputManager PlayerInputManager;

    [SerializeField]
    private Color[] playerColors;
    public Color[] PlayerColors => playerColors;

    [SerializeField]
    private string[] playerNames;

    public delegate void JoinEvent(InputManager inputManager);

    public JoinEvent onPlayerInputJoined;

    public bool MatchHasAI = false;

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
        this.PlayerInputManager = PlayerInputManager.instance;
        DontDestroyOnLoad(gameObject);
    }

    public void RemoveJoinListener()
    {
        this.PlayerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    public void AddJoinListener()
    {
        this.PlayerInputManager.EnableJoining();
        this.PlayerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    public void RemoveListeners()
    {
        LocalPlayerInputs.ForEach(playerInput => playerInput.RemoveListeners());
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        var playerIdentity = playerInput.GetComponent<PlayerIdentity>();
        playerIdentity.color = playerColors[LocalPlayerInputs.Count];
        playerIdentity.playerName = playerNames[LocalPlayerInputs.Count];
        InputManager inputManager = playerInput.GetComponent<InputManager>();
        LocalPlayerInputs.Add(inputManager);
        onPlayerInputJoined(inputManager);
    }

    /// <summary>
    /// Changes the inputMap of all current playerInputs to specified inputMap
    /// This function is used for changing inputs between scenes.
    /// </summary>
    /// <param name="mapNameOrId">Name of the inputMap you want to change to</param>
    public void ChangeInputMaps(string mapNameOrId)
    {
        LocalPlayerInputs.ForEach(playerInput =>
        {
            playerInput.playerInput.SwitchCurrentActionMap(mapNameOrId);
            playerInput.RemoveListeners();
            playerInput.AddListeners();

            // Free the playerInputs from their mortail coils (Player prefab or similar assets)
            var previousParent = playerInput.transform.parent;
            playerInput.transform.parent = null;
            DontDestroyOnLoad(playerInput);
            if (previousParent)
                Destroy(previousParent.gameObject);
        });
    }
}
