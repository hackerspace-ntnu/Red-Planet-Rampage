using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject MainMenuWrapper;
    [SerializeField]
    private GameObject CharacterMenuWrapper;
    [SerializeField]
    private GameObject CharacterView;
    private PlayerInputManager playerInputManager;

    void Start()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    public void ToggleMenu()
    {
        MainMenuWrapper.SetActive(!MainMenuWrapper.activeSelf);
        CharacterMenuWrapper.SetActive(!CharacterMenuWrapper.activeSelf);
    }

    private void AddPlayer(PlayerInput playerInput)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
