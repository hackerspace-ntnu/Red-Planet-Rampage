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

public class MainMenuController : MonoBehaviour
{
    [Unity.Collections.ReadOnly, SerializeField]
    private GameObject currentMenu;
    [SerializeField]
    private RectTransform characterView;
    [SerializeField]
    private GameObject playerBackgroundPanel;

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
    [SerializeField]
    private string[] mapNames;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] uiSelectSounds;
    [SerializeField]
    private AudioGroup uiChooseSounds;

    private PlayerInputManagerController playerInputManagerController;
    private List<InputManager> playerInputs = new List<InputManager>();

    private int loadingDuration = 6;

    private Coroutine introRoutine;

    [SerializeField]
    private GameObject mainMenuCamera;

    private void Awake()
    {
        if (!FindAnyObjectByType<PlayerInputManagerController>())
            Instantiate(innputManagerPrefab);
        introVideo.Prepare();
    }

    private void Start()
    {
        aiButtonOriginalPosition = aIButton.transform.localPosition;
        PlayerInputManagerController.Singleton.MatchHasAI = false;
        audioSource = GetComponent<AudioSource>();

        playerInputManagerController = PlayerInputManagerController.Singleton;
        playerInputManagerController.AddJoinListener();
        playerInputManagerController.PlayerInputManager.splitScreen = false;
        playerInputManagerController.onPlayerInputJoined += AddPlayer;
        if (playerInputManagerController.LocalPlayerInputs.Count > 0)
        {
            // Already played, just show the menu.
            TransferExistingInputs();
            SelectControl(defaultButton);
            introVideo.Stop();
            introVideo.gameObject.SetActive(false);
            introVideoFirstFrame.SetActive(false);
            // Reset loading screen
            LoadingScreen.ResetCounter();
        }
        else
        {
            // First time in menu, play intro video.
            introVideo.started += StopFirstFrame;
            DontDestroyOnLoad(EventSystem.current);
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
        playerInputManagerController.onPlayerInputJoined -= ShowSkipText;
        skipIntroText.gameObject.SetActive(false);
        introVideo.gameObject.SetActive(false);
        defaultMenu.SetActive(true);
        SelectControl(defaultButton);
    }

    private void OnDestroy()
    {
        playerInputManagerController.onPlayerInputJoined -= AddPlayer;
        playerInputManagerController.onPlayerInputJoined -= ShowSkipText;
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

        bool canPlay = playerInputs.Count > 1;
        var colors = startButton.colors;
        colors.normalColor = canPlay ? colors.highlightedColor : colors.disabledColor;
        startButton.colors = colors;
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
        bool canPlay = (playerInputManagerController.MatchHasAI || playerInputs.Count > 1);
        var colors = startButton.colors;
        colors.normalColor = canPlay ? colors.highlightedColor : colors.disabledColor;
        startButton.colors = colors;
    }

    // Currently invoked when entering characterselect menu
    // TODO: Make dedicated hosting UI instead.
    public void HostLocalLobby()
    {
        NetworkManager.singleton.StartHost();
        playerSelectManager.UpdateLobby();
    }

    public void StartTrainingMode()
    {
        Peer2PeerTransport.StartTrainingMode();
        playerSelectManager.UpdateLobby();
    }

    public void HostSteamLobby()
    {
        if (!SteamManager.IsSteamActive)
            return;
        SteamManager.Singleton.HostLobby();
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
