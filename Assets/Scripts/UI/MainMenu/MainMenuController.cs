using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [ReadOnly, SerializeField]
    private GameObject currentMenu;
    [SerializeField]
    private RectTransform characterView;
    [SerializeField]
    private GameObject playerBackgroundPanel;

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
    private PlayerSelectManager playerSelectManager;
    [SerializeField]
    private ToggleButton aIButton;
    private Vector3 aiButtonOriginalPosition;
    private int aiButtonTween;
    [SerializeField] 
    private Button startButton;

    [SerializeField]
    private string[] mapNames;

    private PlayerInputManagerController playerInputManagerController;
    private List<InputManager> playerInputs = new List<InputManager>();
    private List<GameObject> playerBackgrounds = new List<GameObject>();

    void Start()
    {

        playerInputManagerController = PlayerInputManagerController.Singleton;
        playerInputManagerController.playerInputManager.splitScreen = false;
        playerInputManagerController.onPlayerInputJoined += AddPlayer;
        if (playerInputManagerController.playerInputs.Count > 0)
        {
            TransferExistingInputs();
        }
        else
        {
            DontDestroyOnLoad(EventSystem.current);
        }

        SelectControl(defaultButton);
        aiButtonOriginalPosition = aIButton.transform.localPosition;
    }

    private void OnDestroy()
    {
        playerInputManagerController.onPlayerInputJoined -= AddPlayer;
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
        target.Select();
    }

    /// <summary>
    /// Updates playerInputs to use Menu-related actionMap + update eventlisteners
    /// </summary>
    private void TransferExistingInputs()
    {
        playerInputManagerController.ChangeInputMaps("Menu");
        foreach (InputManager inputs in playerInputManagerController.playerInputs)
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
        SelectControl(
            menu.GetComponentsInChildren<Selectable>()
            .Where((selectable) => selectable.GetComponent<Button>().enabled)
            .FirstOrDefault());
    }

    /// <summary>
    /// This function calls loadscene asynchronously, so it can later be expanded to show a loading screen when it's called.
    /// Function to be called as an onclick event from a button
    /// </summary>
    /// <param name="sceneName"></param>
    public void ChangeScene(string name)
    {
        playerInputManagerController.RemoveListeners();
        SceneManager.LoadSceneAsync(name);
    }

    /// <summary>
    /// Subscribes to onplayerjoined and is responsible for adapting menu to new player inputs.
    /// Function needs to force the panels to have flex-like properties as Unity doesn't support dynamic "stretch" of multiple elements inside a parent container.
    /// </summary>
    /// <param name="inputManager"></param>
    private void AddPlayer(InputManager inputManager)
    {
        inputManager.PlayerCamera.enabled = false;
        playerInputs.Add(inputManager);

        bool canPlay = playerInputs.Count > 1;
        var colors = startButton.colors;
        colors.normalColor = canPlay ? colors.highlightedColor : colors.disabledColor;
        startButton.colors = colors;

        foreach (TabGroup t in tabGroups)
        {
            t.SetPlayerInput(inputManager);
        }

        galleryMenu.SetPlayerInput(inputManager);
        creditsMenu.SetPlayerInput(inputManager);

        for (int i = 0; i < playerInputs.Count; i++)
        {
            PlayerIdentity playerIdentity = playerInputs[i].GetComponent<PlayerIdentity>();
            playerSelectManager.SetupPlayerSelectModels(playerIdentity.playerName, playerIdentity.color, i);
        }
    }

    public void ReturnToMainMenu()
    {
        SwitchToMenu(defaultMenu);
    }

    public void StartGameButton(Selectable target)
    {
        bool canPlay = (playerInputManagerController.MatchHasAI || playerInputs.Count > 1);
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
        aiButtonTween = aIButton.gameObject.LeanMoveLocal(aiButtonOriginalPosition * 1.1f, 0.3f).setEasePunch().id;
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

    public void Quit()
    {
        Application.Quit();
    }
}
