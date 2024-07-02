using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QueueMenu : MonoBehaviour
{
    [SerializeField]
    MainMenuController mainMenuController;

    private void Start()
    {
        SteamManager.Singleton.LobbyListUpdate += SetUpQueue;
    }
    private void OnDestroy()
    {
        SteamManager.Singleton.LobbyListUpdate -= SetUpQueue;
    }
    private void SetUpQueue()
    {
        var viableLobbies = SteamManager.Singleton.Lobbies
            .Where(lobby => lobby.gameMode == MatchRules.Singleton.Rules.GameMode)
            .OrderBy(lobby => lobby.availableSlots);

            var enumerator = viableLobbies.GetEnumerator();
            while (enumerator.MoveNext())
                if (SteamManager.Singleton.RequestLobbyJoin(enumerator.Current.id))
                    return;

        Debug.Log("No matching lobbies found, creating new");
        mainMenuController.StartLobby();
    }
}
