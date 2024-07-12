using Mirror;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerController : MonoBehaviour
{
    public static PlayerInputManagerController Singleton { get; private set; }

    public List<InputManager> LocalPlayerInputs = new();
    public Dictionary<uint, InputManager> InputByPlayer = new();

    public int NumInputs => LocalPlayerInputs.Count;

    public List<NetworkConnectionToClient> NetworkClients = new List<NetworkConnectionToClient>();
    public int PlayerCount => Peer2PeerTransport.NumPlayers;

    public PlayerInputManager PlayerInputManager;

    [SerializeField]
    private Color[] playerColors;
    public Color[] PlayerColors => playerColors;

    [SerializeField]
    private Color[] aiColors;
    public Color[] AIColors => aiColors;

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
        PlayerInputManager = PlayerInputManager.instance;
        DontDestroyOnLoad(gameObject);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void RemoveJoinListener()
    {
        PlayerInputManager.onPlayerJoined -= OnPlayerJoined;
    }

    public void AddJoinListener()
    {
        PlayerInputManager.EnableJoining();
        PlayerInputManager.onPlayerJoined += OnPlayerJoined;
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

        var inputManager = playerInput.GetComponent<InputManager>();
        inputManager.PlayerCamera.enabled = false;
        LocalPlayerInputs.Add(inputManager);
        onPlayerInputJoined?.Invoke(inputManager);
        // TODO: Make cursor visible if mouseandkeyboard input joins when our buttons can be clicked by a mouse..

        if (NetworkManager.singleton.isNetworkActive)
            NetworkClient.Send(new PlayerConnectedMessage(LocalPlayerInputs.Count - 1, SteamManager.Singleton.SteamID.m_SteamID));
    }

    public void JoinAllInputs()
    {
        for (int i = 0; i < LocalPlayerInputs.Count; i++)
        {
            NetworkClient.Send(new PlayerConnectedMessage(i, SteamManager.Singleton.SteamID.m_SteamID));
        }
    }

    /// <summary>
    /// Changes the inputMap of all current playerInputs to specified inputMap
    /// This function is used for changing inputs between scenes.
    /// </summary>
    /// <param name="mapNameOrId">Name of the inputMap you want to change to</param>
    public void ChangeInputMaps(string mapNameOrId)
    {
        Cursor.visible = LocalPlayerInputs.Any(i => i.IsMouseAndKeyboard) && mapNameOrId.Equals("Menu");
        Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;

        LocalPlayerInputs.ForEach(playerInput =>
        {
            ChangeInputMapForPlayer(mapNameOrId, playerInput);

            // Free the playerInputs from their mortail coils (Player prefab or similar assets)
            var previousParent = playerInput.transform.parent;
            playerInput.transform.parent = null;
            DontDestroyOnLoad(playerInput);
            if (previousParent)
                Destroy(previousParent.gameObject);
        });
    }

    /// <summary>
    /// Set input map for *one* player
    /// </summary>
    /// <param name="mapNameOrId"></param>
    /// <param name="playerInput"></param>
    public void ChangeInputMapForPlayer(string mapNameOrId, InputManager playerInput)
    {
        playerInput.playerInput.SwitchCurrentActionMap(mapNameOrId);
        playerInput.RemoveListeners();
        playerInput.AddListeners();
    }
}
