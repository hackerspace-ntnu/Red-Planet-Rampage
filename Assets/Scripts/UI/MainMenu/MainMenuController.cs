using CollectionExtensions;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

internal enum LobbyType
{
    Local,
    Public,
    Friends
}

public class MainMenuController : MonoBehaviour
{
    [Unity.Collections.ReadOnly, SerializeField]
    private GameObject currentMenu;

    [SerializeField]
    private VideoPlayer introVideo;
    [SerializeField]
    private float introVideoTransitionTime = 4;
    [SerializeField]
    private GameObject introVideoFirstFrame;
    [SerializeField]
    private TMP_Text skipIntroText;
    [SerializeField]
    private MainMenuSunMovement sun;

    [SerializeField]
    private List<TabGroup> tabGroups;
    [SerializeField]
    private Selectable defaultButton;
    [SerializeField]
    private GameObject defaultMenu;
    [SerializeField]
    private GalleryMenu galleryMenu;
    [SerializeField]
    private CreditsMenu creditsMenu;
    [SerializeField]
    private GameObject mapSelectMenu;
    [SerializeField]
    private OptionsMenu optionsMenu;
    [SerializeField]
    private LevelSelectManager levelSelectManager;
    [SerializeField]
    private PlayerSelectManager playerSelectManager;
    [SerializeField]
    private ToggleButton aIButton;
    private Vector3 aiButtonOriginalPosition;
    private int aiButtonTween;
    [SerializeField]
    private Button startButton;
    [SerializeField]
    private GameObject innputManagerPrefab;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] uiSelectSounds;
    [SerializeField]
    private AudioGroup uiChooseSounds;

    [SerializeField]
    private TMP_Text versionText;

    private PlayerInputManagerController playerInputManagerController;
    private List<InputManager> playerInputs = new List<InputManager>();

    private Coroutine introRoutine;

    [SerializeField]
    private GameObject mainMenuCamera;

    [SerializeField]
    private GameObject videoPlayerCamera;

    private LobbyType lobbyType;

    private InputManager firstInputJoined;

    private void Awake()
    {
        if (!FindAnyObjectByType<PlayerInputManagerController>())
            Instantiate(innputManagerPrefab);
        introVideo.Prepare();
    }

    private void Start()
    {
        // Set inactive since it blocks the ui elements for tabs.
        EventLog.Singleton.transform.GetChild(0).gameObject.SetActive(false);

        aiButtonOriginalPosition = aIButton.transform.localPosition;
        PlayerInputManagerController.Singleton.MatchHasAI = false;
        audioSource = GetComponent<AudioSource>();
        versionText.text = $"Early Access {Application.version}";

        playerInputManagerController = PlayerInputManagerController.Singleton;
        playerInputManagerController.AddJoinListener();
        playerInputManagerController.PlayerInputManager.splitScreen = false;
        playerInputManagerController.onPlayerInputJoined += AddPlayer;
        if (SceneManager.GetActiveScene().name == "Menu")
            ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerReceived += UpdateStartButton;

        if (playerInputManagerController.LocalPlayerInputs.Count > 0)
        {
            // Already played, just show the menu.
            TransferExistingInputs();
            SelectControl(defaultButton);
            introVideo.Stop();
            introVideo.gameObject.SetActive(false);
            introVideoFirstFrame.SetActive(false);
            videoPlayerCamera.SetActive(false);
            mainMenuCamera.SetActive(true);
            // Reset loading screen
            LoadingScreen.ResetCounter();


            if (firstInputJoined.IsMouseAndKeyboard) ShowMouse();
            else HideMouse();
        }
        else
        {
            // First time in menu, play intro video.
            HideMouse();
            introVideo.started += StopFirstFrame;
            playerInputManagerController.onPlayerInputJoined += ShowSkipText;
            defaultMenu.SetActive(false);
            introRoutine = StartCoroutine(WaitForIntroVideoToEnd());
        }
    }

    private void StopFirstFrame(VideoPlayer source)
    {
        introVideoFirstFrame.SetActive(false);
        introVideo.started -= StopFirstFrame;
    }

    private IEnumerator WaitForIntroVideoToEnd()
    {
        while (!introVideo.isPlaying)
        {
            yield return null;
        }
        yield return new WaitForSecondsRealtime((float)introVideo.length - introVideoTransitionTime);
        MusicTrackManager.Singleton.SwitchTo(MusicType.Menu, FadeMode.FadeIn);
        while (introVideo.isPlaying)
        {
            yield return null;
        }
        EndIntro();
    }

    private void ShowSkipText(InputManager inputManager)
    {
        skipIntroText.gameObject.SetActive(true);
        playerInputManagerController.onPlayerInputJoined -= ShowSkipText;
        inputManager.onAnyKey += SkipIntro;
    }

    private void SkipIntro(InputAction.CallbackContext ctx)
    {
        playerInputs[0].onAnyKey -= SkipIntro;
        introVideo.started -= StopFirstFrame;
        if (!introVideo.isPlaying)
            return;
        SkipIntro();
    }

    private void SkipIntro()
    {
        introVideo.Stop();
        StopCoroutine(introRoutine);
        EndIntro();
        if (!MusicTrackManager.Singleton.IsPlaying)
            MusicTrackManager.Singleton.SwitchTo(MusicType.Menu, FadeMode.None);
    }

    private void EndIntro()
    {
        sun.Restart();
        if (firstInputJoined != null)
            if(firstInputJoined.IsMouseAndKeyboard) ShowMouse();
        playerInputManagerController.onPlayerInputJoined -= ShowSkipText;
        skipIntroText.gameObject.SetActive(false);
        introVideo.gameObject.SetActive(false);
        videoPlayerCamera.SetActive(false);
        mainMenuCamera.SetActive(true);
        defaultMenu.SetActive(true);
        SelectControl(defaultButton);
    }

    private void OnDestroy()
    {
        playerInputManagerController.onPlayerInputJoined -= AddPlayer;
        playerInputManagerController.onPlayerInputJoined -= ShowSkipText;
        if (SceneManager.GetActiveScene().name == "Menu" && NetworkManager.singleton)
            ((Peer2PeerTransport)NetworkManager.singleton).OnPlayerReceived -= UpdateStartButton;
    }

    /// <summary>
    /// Call this function for selecting a specific button in scene.
    /// Typically used for jumping to a new button with onclick after toggleMenu()
    /// </summary>
    /// <param name="target"></param>
    public void SelectControl(Selectable target)
    {
        StartCoroutine(WaitSelect(target));
    }

    //Unity for some reason can't immediately Select() button after an onclick event, so this helper function skips a single frame to do so.
    private IEnumerator WaitSelect(Selectable target)
    {
        yield return null;
        target?.Select();
    }

    public void DeselectControl()
    {
        if (EventSystem.current != null)
        {
            // I find no better way to deselect game objects. This is mainly used so you don't have to click twice in tabs.
            StartCoroutine(WaitUnselect());
        }
    }

    private IEnumerator WaitUnselect()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Updates playerInputs to use Menu-related actionMap + update eventlisteners
    /// </summary>
    private void TransferExistingInputs()
    {
        playerInputManagerController.ChangeInputMaps("Menu");
        foreach (InputManager inputs in playerInputManagerController.LocalPlayerInputs)
        {
            AddPlayer(inputs);
        }
    }

    /// <summary>
    /// Switches between menus by disabling previous menu and setting new menu to active.
    /// Should be called by onclick events on buttons in canvas.
    /// </summary>
    /// <param name="menu">The gameObject of the UI-wrapper that should be set to active onclick</param>    
    public void SwitchToMenu(GameObject menu)
    {
        currentMenu.SetActive(false);
        menu.SetActive(true);
        currentMenu = menu;
        SelectControl(menu.GetComponentInChildren<Selectable>());

        //Change camera angle to level select. Must be done here to not bypass AI-check in playerselect
        if (menu == mapSelectMenu)
        {
            mainMenuCamera.GetComponentInChildren<MainMenuMoveCamera>().MoveToLevelSelect();
        }
    }

    /// <summary>
    /// Starts a match in the specified scene.
    /// Function to be called as an onclick event from a button.
    /// </summary>
    /// <param name="sceneName"></param>
    public void ChangeScene(string name)
    {
        PlayerInputManagerController.Singleton.RemoveJoinListener();
        ((Peer2PeerTransport)NetworkManager.singleton).StartMatch(name);
    }

    /// <summary>
    /// Disable menus we may enter a new scene from.
    /// </summary>
    public void DisableSceneSwitching()
    {
        defaultMenu.SetActive(false);
        mapSelectMenu.SetActive(false);
    }

    /// <summary>
    /// Subscribes to onplayerjoined and is responsible for adapting menu to new player inputs.
    /// </summary>
    /// <param name="inputManager"></param>
    private void AddPlayer(InputManager inputManager)
    {
        playerInputs.Add(inputManager);

        if (firstInputJoined == null)
        {
            firstInputJoined = inputManager;
            if (firstInputJoined.IsMouseAndKeyboard && !introVideo.isActiveAndEnabled) ShowMouse();
        }

        inputManager.onMovePerformed += PlayUISelectAudio;
        inputManager.onSelect += PlayChooseAudio;

        foreach (TabGroup t in tabGroups)
        {
            t.SetPlayerInput(inputManager);
        }

        galleryMenu.SetPlayerInput(inputManager);
        creditsMenu.SetPlayerInput(inputManager);
        levelSelectManager.SetPlayerInput(inputManager);
    }

    private void ShowMouse()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideMouse()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void PlayUISelectAudio(InputAction.CallbackContext ctx)
    {
        audioSource.clip = uiSelectSounds.RandomElement();
        audioSource.Play();
    }

    private void PlayChooseAudio(InputAction.CallbackContext ctx)
    {
        uiChooseSounds.Play(audioSource);
    }

    public void ReturnToMainMenu()
    {
        SwitchToMenu(defaultMenu);
    }

    public void StartGameButton(Selectable target)
    {
        bool canPlay = playerInputManagerController.MatchHasAI || PlayerInputManagerController.Singleton.PlayerCount > 1;
        if (canPlay)
        {
            SwitchToMenu(mapSelectMenu);
            SelectControl(target);
            return;
        }
        SelectControl(startButton);
        if (LeanTween.isTweening(aiButtonTween))
        {
            LeanTween.cancel(aiButtonTween);
            aIButton.transform.localPosition = aiButtonOriginalPosition;
        }
        aiButtonTween = aIButton.gameObject.LeanMoveLocal(aiButtonOriginalPosition * 1.05f, 0.3f).setEasePunch().id;
    }

    public void ToggleAI()
    {
        aIButton.Toggle();
        SelectControl(aIButton.Button);
        playerInputManagerController.MatchHasAI = !playerInputManagerController.MatchHasAI;
        SetStartButtonState();
    }

    public void UpdateStartButton(PlayerDetails details)
    {
        SetStartButtonState();
    }

    private void SetStartButtonState()
    {
        bool canPlay = playerInputManagerController.MatchHasAI || Peer2PeerTransport.NumPlayers > 1;
        var colors = startButton.colors;
        colors.normalColor = canPlay ? colors.highlightedColor : colors.disabledColor;
        startButton.colors = colors;
    }

    public void HostLocalLobby()
    {
        lobbyType = LobbyType.Local;
    }

    public void HostFriendsOnlyLobby()
    {
        lobbyType = LobbyType.Friends;
    }

    public void StartTrainingMode()
    {
        EventLog.Singleton.transform.GetChild(0).gameObject.SetActive(false);
        PlayerInputManagerController.Singleton.RemoveJoinListener();
        Peer2PeerTransport.StartTrainingMode();
        playerSelectManager.UpdateLobby();
    }

    public void HostSteamLobby()
    {
        lobbyType = LobbyType.Public;
    }

    public void FetchLobbyInfo()
    {
        SteamManager.Singleton.FetchLobbyInfo();
    }

    public void FetchQueueLobbyInfo()
    {
        SteamManager.Singleton.FetchQueueLobbyInfo();
    }

    public void SetGamemode(Ruleset gamemode)
    {
        MatchRules.Singleton.SetCreatedRuleset(gamemode);
    }

    public void StartLobby()
    {
        EventLog.Singleton.transform.GetChild(0).gameObject.SetActive(true);

        if (!SteamManager.IsSteamActive)
        {
            NetworkManager.singleton.StartHost();
        }
        else
        {
            switch (lobbyType)
            {
                case LobbyType.Public:
                    SteamManager.Singleton.HostLobby();
                    break;
                case LobbyType.Friends:
                    SteamManager.Singleton.HostLobby(Steamworks.ELobbyType.k_ELobbyTypeFriendsOnly);
                    break;
                case LobbyType.Local:
                default:
                    NetworkManager.singleton.StartHost();
                    break;
            }
        }
        playerSelectManager.UpdateLobby();
    }

    public void LeaveLobby()
    {
        NetworkManager.singleton.StopHost();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
