using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    private PlayerInputManager playerInputManager;
    private List<PlayerInput> playerInputs = new List<PlayerInput>();
    private List<GameObject> playerBackgrounds = new List<GameObject>();

    void Start()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDestroy()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
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
    /// Subscribes to onplayerjoined and is responsible for adapting menu to new player inputs.
    /// Function needs to force the panels to have flex-like properties as Unity doens't support dynamic "stretch" of multiple elements inside a parent container.
    /// </summary>
    /// <param name="playerInput"></param>
    private void AddPlayer(PlayerInput playerInput)
    {
        playerInputs.Add(playerInput);
        //Width of the panel adjusted for amount of panels
        var width = characterView.rect.width/playerInputs.Count;
        GameObject panel = Instantiate(playerBackgroundPanel, characterView);
        playerBackgrounds.Add(panel);

        //Update all panels' size and position
        for (int i=0; i<playerBackgrounds.Count; i++)
        {
            playerBackgrounds[i].transform.LeanMoveLocalX(width * i, playerBackgroundPanelAppearTime);
            var rectTransform = playerBackgrounds[i].GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
        }
    }
}
