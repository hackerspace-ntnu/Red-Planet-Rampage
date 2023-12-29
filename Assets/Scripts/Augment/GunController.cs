using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    private const float outputTransitionDistance = 2;

    [HideInInspector]
    public ProjectileController projectile;

    // Where the gun fires the bullets from
    // Is automatically set by barrel or extension (if one i available)
    public Transform[] outputs;

    // Where players will hold the gun
    public Transform RightHandTarget;
    public Transform LeftHandTarget;
    private float localGunXOffset;

    // Keeps track of when gun should be fired
    [HideInInspector]
    public FireRateController fireRateController;

    [HideInInspector]
    public PlayerManager Player { get; private set; }

    // All the stats of the gun and projectile
    public GunStats stats { get; set; }
    public bool HasRecoil = true;

    // Inputs
    public bool triggerHeld, triggerPressed;
    public Vector3 target;

    public delegate void GunEvent(GunStats gunStats);

    public GunEvent onReload;
    public GunEvent onFireStart;
    public GunEvent onFire;
    public GunEvent onFireEnd;
    public GunEvent onInitializeGun;
    public GunEvent onInitializeBullet;

    public void SetPlayer(PlayerManager player)
    {
        Player = player;
        Player.inputManager.onZoomPerformed += OnZoom;
        Player.inputManager.onZoomCanceled += OnZoomCanceled;

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += CancelZoom;
    }

    private AugmentAnimator barrelAnimator;

    private bool isFiring = false;

    private void Start()
    {
        var barrel = GetComponentInChildren<GunBarrel>();
        barrelAnimator = barrel.GetComponentInChildren<AugmentAnimator>();
        if (HasRecoil)
            barrelAnimator.OnShotFiredAnimation += PlayRecoil;
        barrelAnimator.OnShotFiredAnimation += ShotFired;
        barrelAnimator.OnAnimationEnd += FireEnd;

        localGunXOffset = transform.localPosition.x;
    }

    private void OnDestroy()
    {
        if (HasRecoil)
            barrelAnimator.OnShotFiredAnimation -= PlayRecoil;
        barrelAnimator.OnShotFiredAnimation -= ShotFired;
        barrelAnimator.OnAnimationEnd -= FireEnd;
    }

    private void FixedUpdate()
    {
        if (fireRateController == null)
        {
            // No fireratecontroller exists
            return;
        }
        if (!isFiring && fireRateController.shouldFire(triggerPressed, triggerHeld))
        {
            FireGun();
        }
    }
    /// <summary>
    /// Expects a fraction of ammunition to be reloaded.
    /// This fraction is normalized eg. min = 0, max = 1.
    /// </summary>
    /// <param name="fractionNormalized">Percentage of ammunition to be reloaded.</param>
    public void Reload(float fractionNormalized)
    {
        int amount = Mathf.Max(1, Mathf.FloorToInt(stats.magazineSize * fractionNormalized));
        stats.Ammo = Mathf.Min(stats.Ammo + amount, stats.magazineSize);
        onReload?.Invoke(stats);
    }

    public void OnZoom(InputAction.CallbackContext ctx)
    {
        gameObject.LeanMoveLocalX(0f, 0.2f).setEaseInOutCubic();
    }

    public void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        CancelZoom();
    }

    private void CancelZoom()
    {
        gameObject.LeanMoveLocalX(localGunXOffset, 0.2f).setEaseInOutCubic();
    }

    private void FireGun()
    {
        if (stats.Ammo <= 0)
        {
            return;
        }

        isFiring = true;
        onFireStart?.Invoke(stats);
        AimAtTarget();
        projectile.InitializeProjectile(stats);
        onInitializeBullet?.Invoke(stats);
    }

    private void AimAtTarget()
    {
        // Aim at target but lerp in original direction if target is close
        Vector3 targetedOutput = (target - projectile.projectileOutput.position).normalized;
        Vector3 defaultOutput = projectile.projectileOutput.forward;
        float distanceToTarget = Vector3.Distance(projectile.projectileOutput.position, target);
        Vector3 lerpedOutput = Vector3.Lerp(defaultOutput, targetedOutput, distanceToTarget / outputTransitionDistance);
        projectile.projectileRotation = Quaternion.AngleAxis(Vector3.Angle(defaultOutput, lerpedOutput), Vector3.Cross(defaultOutput, lerpedOutput));
    }

    public void PlayRecoil()
    {
        PlayRecoil(stats);
    }

    public void PlayRecoil(GunStats stats)
    {
        gameObject.LeanMoveLocalZ(0.3f, 0.2f).setEasePunch();
    }

    private void ShotFired()
    {
        onFire?.Invoke(stats);
        AimAtTarget();
    }

    private void FireEnd()
    {
        stats.Ammo = Mathf.Clamp(stats.Ammo - 1, 0, stats.magazineSize);
        onFireEnd?.Invoke(stats);
        isFiring = false;
    }
}
