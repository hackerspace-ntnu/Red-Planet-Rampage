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
    private void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            return;
        }
        gunController.onFire += Fire;
        if (!gunController.Player)
            return;
        jigglePhysics.player = gunController.Player;
        gunController.Player.overrideAimTarget = true;
        gunController.onFireStart += Aim;
        gunController.onFire += Aim;
    }

    private void OnDestroy()
    {
        if (!gunController)
            return;
        gunController.onFire -= Fire;
        if (!gunController.Player)
            return;
        gunController.onFireStart -= Aim;
        gunController.onFire -= Aim;
    }

    private void Aim(GunStats stats)
    {
        // Manual override of UpdateAimTarget
        UpdateAimTarget(stats);
    }

    private void Fire(GunStats stats)
    {
        audioSource.Play();
        jigglePhysics.AnimatePushback();
    }

    public void UpdateAimTarget(GunStats stats)
    {
        Vector3 cameraCenter = gunController.Player.inputManager.transform.position;
        Vector3 cameraDirection = gunController.Player.inputManager.transform.rotation * jigglePhysics.Direction;
        Vector3 startPoint = cameraCenter + cameraDirection * gunController.Player.TargetStartOffset;
        if (Physics.Raycast(startPoint, cameraDirection, out RaycastHit hit, maxHitDistance, gunController.Player.HitMask))
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
        if (!gunController || !gunController.Player)
            return;
        var correctedDirection = gunController.Player.inputManager.transform.rotation * jigglePhysics.NormalizedPointer;
        gunController.Player.HUDController.MoveCrosshair(correctedDirection.x, correctedDirection.y);
    }
}
