using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbySelect : DescriptionMenu
{
    [SerializeField]
    private Button queueButton;
    [SerializeField]
    private Button friendsButton;
    [SerializeField]
    private Selectable localButton;
    [SerializeField]
    private MainMenuController mainMenuController;

    private void OnEnable()
    {
        var isOnline = SteamManager.IsSteamActive;

        if (!isOnline)
            StartCoroutine(WaitAndSelectButton());
        SetOnlineButtonStates(isOnline);
    }

    private void SetOnlineButtonStates(bool enabled)
    {
        queueButton.enabled = enabled;
        friendsButton.enabled = enabled;
    }

    private IEnumerator WaitAndSelectButton()
    {
        yield return null;
        mainMenuController.SelectControl(localButton);
    }
}
