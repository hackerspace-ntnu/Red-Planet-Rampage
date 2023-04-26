using UnityEngine;

public class GunController : MonoBehaviour
{
    private const float outputTransitionDistance = 2;

    [HideInInspector]
    public ProjectileController projectile;

    // Where the gun fires the bullets from
    // Is automatically set by barrel or extension (if one i available)
    public Transform[] outputs;

    // Keeps track of when gun should be fired
    [HideInInspector]
    public FireRateController fireRateController;

    [HideInInspector]
    public PlayerManager player;

    // All the stats of the gun and projectile
    public GunStats stats { get; set; }

    // Inputs
    public bool triggerHeld, triggerPressed;
    public Vector3 target;

    public delegate void GunEvent(GunStats gunStats);

    public GunEvent onReload;
    public GunEvent onFire;
    public GunEvent onInitializeGun;

    private void FixedUpdate()
    {
        if (fireRateController.shouldFire(triggerPressed, triggerHeld))
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

    private void FireGun()
    {
        if (stats.Ammo <= 0)
        {
            return;
        }

        stats.Ammo = Mathf.Clamp(stats.Ammo - 1, 0, stats.magazineSize);

        onFire?.Invoke(stats);

        // Aim at target but lerp in original direction if target is close
        Vector3 targetedOutput = (target - projectile.projectileOutput.position).normalized;
        Vector3 defaultOutput = projectile.projectileOutput.forward;
        float distanceToTarget = Vector3.Distance(projectile.projectileOutput.position, target);
        Vector3 lerpedOutput = Vector3.Lerp(defaultOutput, targetedOutput, distanceToTarget / outputTransitionDistance);
        projectile.projectileRotation = Quaternion.AngleAxis(Vector3.Angle(defaultOutput, lerpedOutput), Vector3.Cross(defaultOutput, lerpedOutput));

        projectile.InitializeProjectile(stats);
    }
}
