using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

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

    private Timer timer;

    private void Start()
    {
        timer = GetComponent<Timer>();
    }

    public void StartTrackingMenus()
    {
        timer.StartTimer(20f);

        timer.OnTimerRunCompleted += OnTimerRunCompleted;

        itemSelectMenus = new List<ItemSelectMenu>();
        foreach (var menu in FindObjectsOfType<ItemSelectMenu>())
        {
            itemSelectMenus.Add(menu);
            menu.OnReady += OnReady;
            menu.OnNotReady += OnNotReady;
            menu.SetTimer(timer);
        }

        if (isServer)
        {
            InitializeServerState();
        }
    }

    private void InitializeServerState()
    {
        clientReadyByID = Peer2PeerTransport.Connections
            .ToDictionary(c => c.connectionId, c => false);
        ((Peer2PeerTransport)NetworkManager.singleton).OnDisconnect += OnDisconnect;
        NetworkServer.RegisterHandler<ClientReadyMessage>(OnClientReady);
        NetworkServer.RegisterHandler<ClientNotReadyMessage>(OnClientNotReady);
    }

    private void OnDestroy()
    {
        timer.OnTimerRunCompleted -= OnTimerRunCompleted;
        if (isServer)
        {
            ((Peer2PeerTransport)Peer2PeerTransport.singleton).OnDisconnect -= OnDisconnect;
            NetworkServer.UnregisterHandler<ClientReadyMessage>();
            NetworkServer.UnregisterHandler<ClientNotReadyMessage>();
        }
    }

    [Server]
    private void Finish()
    {
        RpcFinish();
    }

    [ClientRpc]
    private void RpcFinish()
    {
        ((Peer2PeerTransport)Peer2PeerTransport.singleton).UpdateLoadout();
        StartCoroutine(MatchController.Singleton.WaitAndStartNextRound());
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

    private void OnDisconnect(int connectionID)
    {
        clientReadyByID.Remove(connectionID);
        CheckIfAllAreReady();
    }

    private void OnClientReady(NetworkConnectionToClient connection, ClientReadyMessage message)
    {
        clientReadyByID[connection.connectionId] = true;
        CheckIfAllAreReady();
    }

    private void OnTimerRunCompleted()
    {
        if (waitRoutine != null)
            StopCoroutine(waitRoutine);

        Finish();
    }

    private void CheckIfAllAreReady()
    {
        var allClientsAreReady = clientReadyByID.Values.All(m => m);
        if (allClientsAreReady)
        {
            waitRoutine = StartCoroutine(WaitAndFinish());
        }
    }
}
