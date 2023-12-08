using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubberSniper : GunExtension
{
    [SerializeField]
    private FloppyExtensionJiggleMesh jigglePhysics;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private float maxHitDistance = 100f;

    private GunController gunController;
    void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Fire not attached to gun parent!");
            return;
        }
        jigglePhysics.player = gunController.player;
        gunController.player.overrideAimTarget = true;
        gunController.onFire += Fire;
    }

    private void Fire(GunStats stats)
    {
        // Manual override of UpdateAimTarget
        UpdateAimTarget(stats);

        audioSource.Play();
        jigglePhysics.AnimatePushback();
    }

    public void UpdateAimTarget(GunStats stats)
    {
        Vector3 cameraCenter = gunController.player.inputManager.transform.position;
        Vector3 cameraDirection = gunController.player.inputManager.transform.rotation * jigglePhysics.Direction;
        Vector3 startPoint = cameraCenter + cameraDirection * gunController.player.TargetStartOffset;
        if (Physics.Raycast(startPoint, cameraDirection, out RaycastHit hit, maxHitDistance, gunController.player.HitMask))
        {
            gunController.target = hit.point;
        }
        else
        {
            gunController.target = cameraCenter + cameraDirection * maxHitDistance;
        }
    }

    private void FixedUpdate()
    {
        if (!gunController)
            return;
        var correctedDirection = gunController.player.inputManager.transform.rotation * jigglePhysics.NormalizedPointer;
        gunController.player.HUDController.MoveCrosshair(correctedDirection.x, correctedDirection.y);
    }
}
