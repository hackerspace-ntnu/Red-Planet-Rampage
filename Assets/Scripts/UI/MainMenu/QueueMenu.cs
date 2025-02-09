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
            {
                // TODO Should not be showing it from this point
                //      as the host menu shows up briefly first
                LoadingScreen.Singleton.Show(null);
                // Avoid going here multiple times, we found a lobby!
                // SteamManager.Singleton.LobbyListUpdate -= SetUpQueue;
                return;
            }

        Debug.Log("No matching lobbies found, creating new");
        // TODO should be here when loading screen doesn't kill map cards
        // LoadingScreen.Singleton.Hide(mainMenuController.MainMenuCamera);
        mainMenuController.StartLobby();
    }
}
