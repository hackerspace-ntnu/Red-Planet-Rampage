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

    private void LobbyListUpdate()
    {
        int count = 0;
        foreach (CSteamID lobby in SteamManager.Singleton.Lobbies.Keys)
        {
            if (!(count < lobbies.Length))
                break;
            lobbies[count].GetComponentInChildren<TMP_Text>().text = SteamManager.Singleton.Lobbies[lobby] + "'s lobby";
            lobbies[count].onClick.RemoveAllListeners();
            lobbies[count].onClick.AddListener(() => SteamManager.Singleton.RequestLobbyJoin(lobby));
            count++;
        }
        for (int i = count; i < lobbies.Length; i++)
        {
            lobbies[i].GetComponentInChildren<TMP_Text>().text = defaultMessage;
            lobbies[i].onClick.RemoveAllListeners();
        }
    }

}
