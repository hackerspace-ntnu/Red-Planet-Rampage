using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    private float playerBackgroundPanelAppearTime = 0.25f;

    private PlayerInputManagerController playerInputManagerController;
    private List<InputManager> playerInputs = new List<InputManager>();
    private List<GameObject> playerBackgrounds = new List<GameObject>();

    void Start()
    {
        playerInputManagerController = PlayerInputManagerController.Singleton;
        playerInputManagerController.onPlayerInputJoined += AddPlayer;
        playerInputManagerController.playerInputManager.splitScreen = false;

        if (playerInputManagerController.playerInputs.Count > 0)
        {
            TransferExistingInputs();
        }
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
    public void SelectButton(Button target)
    {
        StartCoroutine(WaitSelect(target));
    }

    //Unity for some reason can't immediately Select() button after an onclick event, so this helper function skips a single frame to do so.
    private IEnumerator WaitSelect(Button target)
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
            inputs.RemoveListeners();
            AddPlayer(inputs);
            inputs.AddListeners();
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
    }

    /// <summary>
    /// This function calls loadscene asynchronously, so it can later be expanded to show a loading screen when it's called.
    /// Function to be called as an onclick event from a button
    /// </summary>
    /// <param name="sceneName"></param>
    public void ChangeScene(string sceneName)
    {
        playerInputManagerController.RemoveListeners();
        SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Subscribes to onplayerjoined and is responsible for adapting menu to new player inputs.
    /// Function needs to force the panels to have flex-like properties as Unity doens't support dynamic "stretch" of multiple elements inside a parent container.
    /// </summary>
    /// <param name="inputManager"></param>
    private void AddPlayer(InputManager inputManager)
    {
        inputManager.GetComponent<Camera>().enabled = false;
        playerInputs.Add(inputManager);
        //Width of the panel adjusted for amount of panels
        var width = characterView.rect.width/playerInputs.Count;
        GameObject panel = Instantiate(playerBackgroundPanel, characterView);
        playerBackgrounds.Add(panel);

        //Update all panels' size, position and color
        for (int i=0; i<playerBackgrounds.Count; i++)
        {
            playerBackgrounds[i].transform.LeanMoveLocalX(width * i, playerBackgroundPanelAppearTime);
            var rectTransform = playerBackgrounds[i].GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
            //Update color
            playerBackgrounds[i].GetComponent<Image>().color = playerInputs[i].GetComponent<PlayerIdentity>().color;
        }
    }
}
