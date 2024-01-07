using System;
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

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += DisableHand;
        player.onDeath += DisableHand;
        unsubscribePlayer = () => player.onDeath -= DisableHand;
    }

    private void OnDestroy()
    {
        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd -= DisableHand;
        unsubscribePlayer?.Invoke();
    }

    private void DisableHand(PlayerManager killer, PlayerManager victim)
    {
        DisableHand();
    }

    private void DisableHand()
    {
        if (this && gameObject)
            gameObject.SetActive(false);
    }
}
