using Mirror;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManagerController : MonoBehaviour
{
    public static PlayerInputManagerController Singleton { get; private set; }

    public List<InputManager> LocalPlayerInputs = new();
    public Dictionary<uint, InputManager> InputByPlayer = new();

    public int PlayerCount => RPRNetworkManager.NumPlayers;

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

    public JoinEvent OnPlayerInputJoined;
    public JoinEvent OnPlayerInputLeft;

    public bool MatchHasAI = false;

    private void Awake()
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
        Cursor.visible = false;
    }

    public void RemoveJoinListener()
    {
        PlayerInputManager.DisableJoining();
        PlayerInputManager.onPlayerJoined -= OnPlayerJoined;
        PlayerInputManager.onPlayerLeft -= OnPlayerLeft;
    }

    public void AddJoinListener()
    {
        PlayerInputManager.EnableJoining();
        PlayerInputManager.onPlayerJoined += OnPlayerJoined;
        PlayerInputManager.onPlayerLeft += OnPlayerLeft;
    }

    public void RemoveListeners()
    {
        LocalPlayerInputs.ForEach(playerInput => playerInput.RemoveListeners());
    }

    private void OnPlayerJoined(PlayerInput playerInput)
    {
        var inputManager = playerInput.GetComponent<InputManager>();
        inputManager.PlayerCamera.enabled = false;
        LocalPlayerInputs.Add(inputManager);
        OnPlayerInputJoined?.Invoke(inputManager);
        // TODO: Make cursor visible if mouseandkeyboard input joims when our buttons can be clicked by a mouse..

        if (NetworkManager.singleton.isNetworkActive)
            NetworkClient.Send(new PlayerConnectedMessage(LocalPlayerInputs.Count - 1));
    }

    private void OnPlayerLeft(PlayerInput playerInput)
    {
        var inputManager = playerInput.GetComponent<InputManager>();
        OnPlayerInputLeft?.Invoke(inputManager);
        LocalPlayerInputs.Remove(inputManager);

        if (NetworkManager.singleton.isNetworkActive)
            NetworkClient.Send(new PlayerDisconnectedInputMessage());
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
