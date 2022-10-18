using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuWrapper;
    [SerializeField]
    private GameObject characterMenuWrapper;
    [SerializeField]
    private RectTransform characterView;
    [SerializeField]
    private GameObject playerBackgroundPanel;

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

    /// <summary>
    /// Toggles between mainMenu and characterMenu.
    /// </summary>
    public void ToggleMenu()
    {
        mainMenuWrapper.SetActive(!mainMenuWrapper.activeSelf);
        characterMenuWrapper.SetActive(!characterMenuWrapper.activeSelf);
    }

    /// <summary>
    /// Subscribes to onplayerjoined and is responsible for adapting menu to new player inputs.
    /// </summary>
    /// <param name="playerInput"></param>
    private void AddPlayer(PlayerInput playerInput)
    {
        playerInputs.Add(playerInput);
        var width = characterView.rect.width/playerInputs.Count;
        GameObject panel = Instantiate(playerBackgroundPanel, characterView);
        playerBackgrounds.Add(panel);
        for (int i=0; i<playerBackgrounds.Count; i++)
        {
            playerBackgrounds[i].transform.LeanMoveLocalX(width * i, 0.25f);
            var rectTransform = playerBackgrounds[i].GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
        }
    }

    //Unity for some reason can't immediately Select() button after an onclick event, so this helper function skips a single frame to do so.
    private IEnumerator WaitSelect(Button target)
    {
        yield return null;
        target.Select();
    }
}
