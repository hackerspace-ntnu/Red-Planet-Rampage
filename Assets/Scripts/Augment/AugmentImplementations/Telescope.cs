using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Telescope : GunExtension
{
    [SerializeField]
    private float overrideZoomFov = 10f;
    [SerializeField]
    private float overrideZoomSpeed = 0.1f;

    private GunController gunController;
    private List<MeshRenderer> gunMeshes;
    private List<SkinnedMeshRenderer> gunSkinMeshes;

    void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Telescope not attached to gun parent!");
            return;
        }

        if (!gunController.Player)
            return;

        var playerMovement = gunController.Player.GetComponent<PlayerMovement>();
        playerMovement.ZoomFov = overrideZoomFov;
        playerMovement.LookSpeedZoom = overrideZoomSpeed;

        gunController.Player.inputManager.onZoomPerformed += OnZoom;
        gunController.Player.inputManager.onZoomCanceled += OnZoomCanceled;

        gunMeshes = gunController.GetComponentsInChildren<MeshRenderer>().ToList();
        gunSkinMeshes = gunController.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += CancelZoom;
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        gunMeshes.ForEach((mesh) => mesh.enabled = false);
        gunSkinMeshes.ForEach((mesh) => mesh.enabled = false);
        gunController.Player.HUDController.TweenScope(1f, 0.2f);
    }

    private void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        CancelZoom();
    }

    private void CancelZoom()
    {
        gunMeshes.ForEach((mesh) => mesh.enabled = true);
        gunSkinMeshes.ForEach((mesh) => mesh.enabled = true);
        gunController.Player.HUDController.TweenScope(0f, 0.2f);
    }

    private void OnDestroy()
    {
        gunController.Player.inputManager.onZoomPerformed -= OnZoom;
        gunController.Player.inputManager.onZoomCanceled -= OnZoomCanceled;
    }
}