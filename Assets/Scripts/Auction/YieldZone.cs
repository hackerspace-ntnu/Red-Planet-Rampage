using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class YieldZone : MonoBehaviour
{
    [SerializeField]
    private TMP_Text yieldText;
    [SerializeField]
    private TMP_Text yieldInfoText;

    private void Start()
    {
        yieldText.text = "YIELD";
        yieldInfoText.text = $"0/{RPRNetworkManager.NumPlayers}";
    }

    public void Subscribe()
    {
        AuctionDriver.Singleton.OnYieldChange += SetYieldText;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerManager player))
            return;
        AuctionDriver.Singleton.AddYieldingPlayer(player);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out PlayerManager player))
            return;
        AuctionDriver.Singleton.RemoveYieldingPlayer(player);
    }

    private void SetYieldText()
    {
        yieldText.text = "YIELD";
        yieldInfoText.text = $"{AuctionDriver.Singleton.YieldingPlayerCount}/{RPRNetworkManager.NumPlayers}";
    }

    public void SetRemainingTimeText(int time)
    {
        yieldText.text = "SKIP IN";
        yieldInfoText.text = time.ToString();
    }

    private void OnDestroy()
    {
        AuctionDriver.Singleton.OnYieldChange -= SetYieldText;
        yieldText.text = "YIELD";
        yieldInfoText.text = $"0/{RPRNetworkManager.NumPlayers}";
    }
}
