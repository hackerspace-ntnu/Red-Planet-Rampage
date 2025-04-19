using UnityEngine;

public class PlayerHand : MonoBehaviour, PlayerSubscriber
{
    [SerializeField]
    private SkinnedMeshRenderer handMaterial;
    [SerializeField]
    private GameObject mesh;
    [SerializeField]
    private Transform holdingPoint;
    public Transform HoldingPoint => holdingPoint;

    public void Subscribe(PlayerManager player)
    {
        handMaterial.material.color = player.identity.color;
        if (player.inputManager)
        {
            var cullingLayer = LayerMask.NameToLayer("Gun " + player.inputManager.playerInput.playerIndex);
            gameObject.layer = cullingLayer;
            mesh.layer = cullingLayer;
        }
        else
        {
            handMaterial.enabled = false;
        }
        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += DisableHand;
        player.onDeath += DisableHand;
    }

    public void Unsubscribe(PlayerManager player)
    {
        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd -= DisableHand;
        player.onDeath -= DisableHand;
    }

    private void DisableHand(PlayerManager killer, PlayerManager victim, DamageInfo info)
    {
        DisableHand();
    }

    private void DisableHand()
    {
        if (this && gameObject)
            gameObject.SetActive(false);
    }
}
