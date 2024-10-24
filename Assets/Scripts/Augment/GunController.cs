using System;
using System.Collections;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : NetworkBehaviour
{
    private const float outputTransitionDistance = 2;
    public const float InvulnerabilityDistance = 3;

    [HideInInspector]
    public ProjectileController projectile;

    // Where the gun fires the bullets from
    // Is automatically set by barrel or extension (if one i available)
    public Transform[] outputs;

    // Where players will hold the gun
    public Transform RightHandTarget;
    public Transform LeftHandTarget;
    private float localGunXOffset;
    private float localGunZOffset;

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
    public bool TargetIsTooClose;
    public bool AimCorrectionEnabled = true;

    public delegate void GunEvent(GunStats gunStats);

    public GunEvent onReload;
    public GunEvent onFireStart;
    /// <summary>
    /// Invoked on each shot
    /// </summary>
    public GunEvent onFire;
    public GunEvent onFireEnd;
    /// <summary>
    /// Invoked when trying to fire a shot while the magazine is empty
    /// </summary>
    public GunEvent onFireNoAmmo;
    public GunEvent onInitializeGun;
    /// <summary>
    /// Invoked when a projectile is initialized
    /// </summary>
    public GunEvent onInitializeBullet;

    private uint nextShotID = 0;
    public uint CurrentShotID => nextShotID;

    public void SetPlayer(PlayerManager player)
    {
        Player = player;
        if (!Player.inputManager)
            return;
        SetPlayerInput(player.inputManager);
        Player.inputManager.GetComponentInChildren<JiggleBone>().body = Player.GetComponent<PlayerMovement>().Body;
        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd += CancelZoom;
    }

    public void SetPlayerInput(InputManager input)
    {
        input.onZoomPerformed += OnZoom;
        input.onZoomCanceled += OnZoomCanceled;
    }

    private AugmentAnimator barrelAnimator;

    private bool isFiring = false;

    private int recoilTween;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        var barrel = GetComponentInChildren<GunBarrel>();
        if (!barrel)
            return;
        barrelAnimator = barrel.GetComponentInChildren<AugmentAnimator>();
        barrelAnimator.OnShotFiredAnimation += PlayRecoil;
        barrelAnimator.OnShotFiredAnimation += ShotFired;
        barrelAnimator.OnAnimationEnd += FireEnd;

        localGunXOffset = transform.localPosition.x;
        localGunZOffset = transform.localPosition.z;
    }

    private void OnDestroy()
    {
        if (!barrelAnimator)
            return;

        if (HasRecoil)
            barrelAnimator.OnShotFiredAnimation -= PlayRecoil;
        barrelAnimator.OnShotFiredAnimation -= ShotFired;
        barrelAnimator.OnAnimationEnd -= FireEnd;

        if (!Player || !Player.inputManager)
            return;

        Player.inputManager.onZoomPerformed -= OnZoom;
        Player.inputManager.onZoomCanceled -= OnZoomCanceled;

        if (MatchController.Singleton)
            MatchController.Singleton.onRoundEnd -= CancelZoom;
    }

    private bool ShouldFire() => !isFiring && fireRateController.shouldFire(triggerPressed, triggerHeld);

    [Client]
    private void FixedUpdate()
    {
        if (fireRateController == null)
        {
            // No fireratecontroller exists
            return;
        }
        if (ShouldFire())
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
        int amount = Mathf.Max(1, Mathf.FloorToInt(stats.MagazineSize * fractionNormalized));
        stats.Ammo = Mathf.Min(stats.Ammo + amount, stats.MagazineSize);
        onReload?.Invoke(stats);
    }

    public void OnZoom(InputAction.CallbackContext ctx)
    {
        if (gameObject)
            gameObject.LeanMoveLocalX(0f, 0.2f).setEaseInOutCubic();
    }

    public void OnZoomCanceled(InputAction.CallbackContext ctx)
    {
        CancelZoom();
    }

    private void CancelZoom()
    {

        if (gameObject)
            gameObject.LeanMoveLocalX(localGunXOffset, 0.2f).setEaseInOutCubic();
    }

    [Command]
    private void CmdFire(Quaternion rotation)
    {
        // TODO Only go forth with firing if timing is right!
        // (requires rewriting some stuff in FireRateController to not mutate unless the check is real)
        RpcFire(rotation);
    }

    [ClientRpc(includeOwner = false)]
    private void RpcFire(Quaternion rotation)
    {
        try
        {
            onFireStart?.Invoke(stats);
            projectile.projectileOutput = outputs[0];
            projectile.projectileRotation = rotation;
            ActuallyFire();
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to fire gun!");
            Debug.LogError(e);
        }
    }

    [Command]
    private void CmdFireWithNoAmmo()
    {
        RpcFireWithNoAmmo();
    }

    [ClientRpc(includeOwner = false)]
    private void RpcFireWithNoAmmo()
    {
        try
        {
            onFireNoAmmo?.Invoke(stats);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to fire gun w/o ammo!");
            Debug.LogError(e);
        }
    }

    private void FireGun()
    {
        if (stats.Ammo <= 0)
        {
            CmdFireWithNoAmmo();
            // Handle this immediately yourself
            onFireNoAmmo?.Invoke(stats);
            return;
        }

        try
        {
            onFireStart?.Invoke(stats);
            AimAtTarget();
            // Tell server to fire
            CmdFire(projectile.projectileRotation);
            // but fire immediately yourself!
            ActuallyFire();
        }
        catch (Exception e)
        {
            // Hopefully recoverable error. Firing has had lots of bugs before,
            // hopefully we avoid displaying them in their gruesome nature to the user this way.
            Debug.LogError("Failed to fire gun on owner's client!");
            Debug.LogError(e);
        }
    }

    private void ActuallyFire()
    {
        projectile.InitializeProjectile(stats, nextShotID++);
        onInitializeBullet?.Invoke(stats);
    }

    private void AimAtTarget()
    {
        if (!AimCorrectionEnabled)
        {
            projectile.projectileOutput = outputs[0];
            projectile.projectileRotation = Quaternion.identity;
            return;
        }
        // Output become camera when distance hit is closer than weaponOutput
        if (Player)
            projectile.projectileOutput = TargetIsTooClose ? Player.inputManager.transform : outputs[0];

        // Aim at target but lerp in original direction if target is close
        Vector3 targetedOutput = (target - projectile.projectileOutput.position).normalized;
        Vector3 defaultOutput = projectile.projectileOutput.forward;
        float distanceToTarget = Vector3.Distance(projectile.projectileOutput.position, target);
        Vector3 lerpedOutput = Vector3.Lerp(defaultOutput, targetedOutput, distanceToTarget / outputTransitionDistance);
        projectile.projectileRotation = Quaternion.AngleAxis(Vector3.Angle(defaultOutput, lerpedOutput), Vector3.Cross(defaultOutput, lerpedOutput));
    }

    public void PlayRecoil()
    {
        if (HasRecoil)
            PlayRecoil(stats);
    }

    public void PlayRecoil(GunStats stats)
    {
        if (LeanTween.isTweening(recoilTween))
        {
            LeanTween.cancel(recoilTween);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, localGunZOffset);
        }
        var moveAmount = Player != null ? 0.3f : 0.6f;
        // TODO reduce tween time based on fire rate
        recoilTween = gameObject.LeanMoveLocalZ(moveAmount, 0.2f).setEasePunch().id;
    }

    private void ShotFired()
    {
        onFire?.Invoke(stats);
        AimAtTarget();
    }

    private void FireEnd()
    {
        stats.Ammo = Mathf.Clamp(stats.Ammo - 1, 0, stats.MagazineSize);
        isFiring = false;
        onFireEnd?.Invoke(stats);
    }
}
