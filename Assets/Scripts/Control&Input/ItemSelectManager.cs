using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

// TODO remove first two structs if unused
internal struct StartTrackingMessage : NetworkMessage
{
}

internal struct ReadyToTrackMessage : NetworkMessage
{
}

internal struct ClientReadyMessage : NetworkMessage
{
}

internal struct ClientNotReadyMessage : NetworkMessage
{
}

public class ItemSelectManager : NetworkBehaviour
{
    [SerializeField] private float graceTime = 1;

    private List<ItemSelectMenu> itemSelectMenus;
    private Dictionary<int, bool> clientReadyByID = new();

    private Coroutine waitRoutine;

    [SerializeField] private GameObject loadingScreen;

    private int delayDuration = 2;

    // TODO network version of this thing
    public void StartTrackingMenus()
    {
        itemSelectMenus = new List<ItemSelectMenu>();
        foreach (var menu in FindObjectsOfType<ItemSelectMenu>())
        {
            itemSelectMenus.Add(menu);
            menu.OnReady += OnReady;
            menu.OnNotReady += OnNotReady;
        }

        if (isServer)
        {
            InitializeServerData();
            NetworkServer.RegisterHandler<ClientReadyMessage>(OnClientReady);
            NetworkServer.RegisterHandler<ClientNotReadyMessage>(OnClientNotReady);
        }
    }

    private void OnDestroy()
    {
        if (isServer)
        {
            NetworkServer.UnregisterHandler<ClientReadyMessage>();
            NetworkServer.UnregisterHandler<ClientNotReadyMessage>();
        }
    }

    private void InitializeServerData()
    {
        clientReadyByID = Peer2PeerTransport.Connections
            .ToDictionary(c => c.connectionId, c => false);
    }

    private void Finish()
    {
        StartCoroutine(LoadAndChangeScene());
    }

    private IEnumerator LoadAndChangeScene()
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(delayDuration);
        AuctionDriver.Singleton.ChangeScene();
    }

    private IEnumerator WaitAndFinish()
    {
        yield return new WaitForSeconds(graceTime);
        Finish();
    }

    private void OnNotReady(ItemSelectMenu menu)
    {
        NetworkClient.Send(new ClientNotReadyMessage());
    }

    private void OnReady(ItemSelectMenu menu)
    {
        var allPlayersAreReady = itemSelectMenus.All(m => m.IsReady);
        if (allPlayersAreReady)
        {
            NetworkClient.Send(new ClientReadyMessage());
        }
    }

    private void OnClientNotReady(NetworkConnectionToClient connection, ClientNotReadyMessage message)
    {
        clientReadyByID[connection.connectionId] = false;
        if (waitRoutine is not null)
        {
            StopCoroutine(waitRoutine);
        }
    }

    private void OnClientReady(NetworkConnectionToClient connection, ClientReadyMessage message)
    {
        clientReadyByID[connection.connectionId] = true;
        var allClientsAreReady = clientReadyByID.Values.All(m => m);
        if (allClientsAreReady)
        {
            waitRoutine = StartCoroutine(WaitAndFinish());
        }
    }
}
