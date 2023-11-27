using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer handMaterial;

    public void SetPlayer(PlayerManager player)
    {
        handMaterial.material.color = player.identity.color;
        gameObject.layer = LayerMask.NameToLayer("Gun " + player.inputManager.playerInput.playerIndex);
    }

}
