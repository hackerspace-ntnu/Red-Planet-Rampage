using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer handMaterial;
    [SerializeField]
    private GameObject mesh;
    [SerializeField]
    private Transform holdingPoint;
    public Transform HoldingPoint => holdingPoint;
    private Action unsubscribePlayer;

    public void SetPlayer(PlayerManager player)
    {
        handMaterial.material.color = player.identity.color;
        var cullingLayer = LayerMask.NameToLayer("Gun " + player.inputManager.playerInput.playerIndex);
        gameObject.layer = cullingLayer;
        mesh.layer = cullingLayer;

        MatchController.Singleton.onRoundEnd += DisableHand;
        player.onDeath += DisableHand;
        unsubscribePlayer = () => player.onDeath -= DisableHand;
    }

    private void OnDestroy()
    {
        MatchController.Singleton.onRoundEnd -= DisableHand;
        unsubscribePlayer?.Invoke();
    }

    private void DisableHand(PlayerManager killer, PlayerManager victim)
    {
        gameObject.SetActive(false);
    }

    private void DisableHand()
    {
        gameObject.SetActive(false);
    }
}
