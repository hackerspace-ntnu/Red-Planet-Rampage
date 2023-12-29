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

        gunController.Player.inputManager.onZoomPerformed += Zoom;
        gunController.Player.inputManager.onZoomCanceled += CancelZoom;

        gunMeshes = gunController.GetComponentsInChildren<MeshRenderer>().ToList();
        gunSkinMeshes = gunController.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
    }

    private void Zoom(InputAction.CallbackContext ctx)
    {
        gunMeshes.ForEach((mesh) => mesh.enabled = false);
        gunSkinMeshes.ForEach((mesh) => mesh.enabled = false);
        gunController.Player.HUDController.TweenScope(1f, 0.2f);
    }

    private void CancelZoom(InputAction.CallbackContext ctx)
    {
        gunMeshes.ForEach((mesh) => mesh.enabled = true);
        gunSkinMeshes.ForEach((mesh) => mesh.enabled = true);
        gunController.Player.HUDController.TweenScope(0f, 0.2f);
    }

    private void OnDestroy()
    {
        gunController.Player.inputManager.onZoomPerformed -= Zoom;
        gunController.Player.inputManager.onZoomCanceled -= CancelZoom;
    }
}
