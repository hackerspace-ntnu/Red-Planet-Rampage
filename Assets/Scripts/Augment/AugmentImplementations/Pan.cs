using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pan : GunExtension
{
    [SerializeField]
    private MeshCollider hitboxCollider;
    [SerializeField]
    private Transform panModel;
    private PlayerMovement playerMovement;
    private const float skateJumpForce = 5f;
    private GunController gunController;
    private const int hitBoxLayer = 3;
    void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;
        if (!gunController.Player)
        {
            gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
            hitboxCollider.gameObject.layer = hitBoxLayer;
            return;
        }
        panModel.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        panModel.localPosition = new Vector3(panModel.localPosition.x, panModel.localPosition.y -0.3f, panModel.localPosition.z);
        gunController.Player.inputManager.onSelect += TryTrickJump;
        hitboxCollider.enabled = false;
        playerMovement = gunController.Player.GetComponent<PlayerMovement>();

    }

    private void TryTrickJump(InputAction.CallbackContext ctx)
    {
        if (playerMovement)
            if (playerMovement.StateIsAir)
                if (Mathf.Abs(playerMovement.Body.velocity.y) < 0.01f)
                    if (playerMovement.Body.velocity.magnitude > 1f)
                        playerMovement.Body.AddForce(Vector3.up * skateJumpForce, ForceMode.VelocityChange);
    }

    private void OnDestroy()
    {
        if (gunController)
            if (gunController.Player)
                gunController.Player.inputManager.onSelect -= TryTrickJump;
    }

}
