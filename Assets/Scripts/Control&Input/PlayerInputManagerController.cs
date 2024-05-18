using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerController : MonoBehaviour
{
    public static PlayerInputManagerController Singleton { get; private set; }

    public List<InputManager> LocalPlayerInputs = new List<InputManager>();
    public Dictionary<uint, InputManager> InputByPlayer = new();

    public List<NetworkConnectionToClient> NetworkClients = new List<NetworkConnectionToClient>();
    public int PlayerCount => NetworkClient.isConnected ? Peer2PeerTransport.NumPlayersInMatch : LocalPlayerInputs.Count;

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
        // TODO refactor this for online (should not source info from here)
        var playerIdentity = playerInput.GetComponent<PlayerIdentity>();
        playerIdentity.color = playerColors[LocalPlayerInputs.Count];
        playerIdentity.playerName = playerNames[LocalPlayerInputs.Count];
        InputManager inputManager = playerInput.GetComponent<InputManager>();
        // TODO fix camera hijinks
        // playerInput.camera.enabled = false;
        LocalPlayerInputs.Add(inputManager);
        onPlayerInputJoined(inputManager);
        if (NetworkClient.isConnected)
            NetworkClient.Send(new PlayerConnectedMessage(LocalPlayerInputs.Count - 1));
    }

    public void JoinAllInputs()
    {
        for (int i = 0; i < LocalPlayerInputs.Count; i++)
        {
            NetworkClient.Send(new PlayerConnectedMessage(i));
        }
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
