using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer handMaterial;
    [SerializeField]
    private GameObject mesh;

    public void SetPlayer(PlayerManager player)
    {
        handMaterial.material.color = player.identity.color;
        var cullingLayer = LayerMask.NameToLayer("Gun " + player.inputManager.playerInput.playerIndex);
        gameObject.layer = cullingLayer;
        mesh.layer = cullingLayer;
    }

}
