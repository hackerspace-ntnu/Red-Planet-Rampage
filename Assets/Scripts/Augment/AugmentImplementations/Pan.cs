using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pan : GunExtension
{
    [SerializeField]
    private MeshCollider hitboxCollider;
    [SerializeField]
    private GameObject explosionColliders;
    [SerializeField]
    private Transform panModel;
    private PlayerMovement playerMovement;
    private const float skateJumpForce = 11f;
    private const float skateMoveForce = 2f;
    private GunController gunController;
    private const int hitBoxLayer = 3;
    private const int aiExplosionLayer = 15;
    void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;
        if (!gunController.Player)
        {
            gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
            hitboxCollider.gameObject.layer = hitBoxLayer;
            explosionColliders.SetActive(false);
            return;
        }
        panModel.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        panModel.localPosition = new Vector3(panModel.localPosition.x, panModel.localPosition.y -0.3f, panModel.localPosition.z);
        var health = gunController.Player.GetComponent<HealthController>();

        if (gunController.Player is AIManager)
        {
            gunController.GetComponentsInChildren<PanHitbox>()
                .ToList().ForEach(box =>
                {
                    box.gameObject.layer = aiExplosionLayer;
                    box.health = health;
                });
            hitboxCollider.gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
            hitboxCollider.gameObject.layer = hitBoxLayer;
            return;
        }

        if (gunController.Player.inputManager)
            gunController.Player.inputManager.onSelect += TryTrickJump;
        hitboxCollider.enabled = false;
        playerMovement = gunController.Player.GetComponent<PlayerMovement>();
        gunController.Player.GunOrigin.GetComponentsInChildren<PanHitbox>()
            .ToList().ForEach(box =>  box.health = health);
        if (playerMovement)
            playerMovement.OnMove += TryPanSkateBoost;
    }

    private void TryTrickJump(InputAction.CallbackContext ctx)
    {
        bool isCorrectMovement = playerMovement && playerMovement.StateIsAir && playerMovement.IsCrouching;
        var isNotFlying = Mathf.Abs(playerMovement.Body.velocity.y) < 0.01f;
        if (isCorrectMovement && isNotFlying && playerMovement.Body.velocity.magnitude > 1f)
            playerMovement.Body.AddForce(
                new Vector3(gunController.Player.transform.forward.x, 1f, gunController.Player.transform.forward.z) * skateJumpForce,
                ForceMode.VelocityChange);
    }

    private void TryPanSkateBoost(Rigidbody body)
    {
        bool isSkating = (playerMovement.StateIsAir && playerMovement.IsCrouching && Mathf.Abs(body.velocity.y) < 0.01f);
        bool isMoving = (gunController.Player.inputManager.moveInput.magnitude > 0.5f);
        if (!isMoving || !isSkating)
            return;
        Vector3 moveDirection = gunController.Player.transform.forward * gunController.Player.inputManager.moveInput.y + gunController.Player.transform.right * gunController.Player.inputManager.moveInput.x;
        body.AddForce(moveDirection * skateMoveForce, ForceMode.VelocityChange);
    }
    private void OnDestroy()
    {
        if (gunController)
            if (gunController.Player)
                if (gunController.Player.inputManager)
                    gunController.Player.inputManager.onSelect -= TryTrickJump;
    }

}
