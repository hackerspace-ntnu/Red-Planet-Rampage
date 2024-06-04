using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListMenu : MonoBehaviour
{
    [SerializeField]
    private Button[] lobbies;

    private const string defaultMessage = "No lobby found...";

    void Start()
    {
        SteamManager.Singleton.LobbyListUpdate += LobbyListUpdate;
    }

    private void OnDestroy()
    {
        if (SteamManager.Singleton)
            SteamManager.Singleton.LobbyListUpdate -= LobbyListUpdate;
    }

    private void LobbyListUpdate()
    {
        int count = 0;
        foreach (Lobby lobby in SteamManager.Singleton.Lobbies)
        {
            if (!(count < lobbies.Length))
                break;
            lobbies[count].GetComponentInChildren<TMP_Text>().text = $"{lobby.name} ({4 - lobby.availableSlots}/4)";
            lobbies[count].onClick.RemoveAllListeners();
            lobbies[count].onClick.AddListener(() => SteamManager.Singleton.RequestLobbyJoin(lobby.id));
            count++;
        }
        for (int i = count; i < lobbies.Length; i++)
        {
            lobbies[i].GetComponentInChildren<TMP_Text>().text = defaultMessage;
            lobbies[i].onClick.RemoveAllListeners();
        }
    }

}
