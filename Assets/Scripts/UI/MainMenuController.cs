using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuWrapper;
    [SerializeField]
    private GameObject characterMenuWrapper;
    [SerializeField]
    private RectTransform characterView;
    private PlayerInputManager playerInputManager;

    [SerializeField]
    private GameObject playerBackgroundPanel;

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

    public void ToggleMenu()
    {
        mainMenuWrapper.SetActive(!mainMenuWrapper.activeSelf);
        characterMenuWrapper.SetActive(!characterMenuWrapper.activeSelf);
    }

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
}
