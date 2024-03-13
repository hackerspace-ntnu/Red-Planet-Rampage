using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    private const int steamAppID = 2717710;
    public static SteamManager Singleton;

    void Awake()
    {
        #region Singleton boilerplate

        if (Singleton != null)
        {
            if (Singleton != this)
            {
                Debug.LogWarning($"There's more than one {Singleton.GetType()} in the scene!");
                Destroy(gameObject);
            }

            return;
        }

        Singleton = this;

        #endregion Singleton boilerplate
        DontDestroyOnLoad(this);
        try
        {
            Steamworks.SteamClient.Init(steamAppID, true);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void HostLobby()
    {
        var transportSettings = GetComponent<Peer2PeerTransport>();
        transportSettings.serverId = Steamworks.SteamClient.SteamId;
        transportSettings.StartServer();

    }

    void Update()
    {
        Steamworks.SteamClient.RunCallbacks();
    }
    private void OnApplicationQuit()
    {
        Steamworks.SteamClient.Shutdown();
    }

}
