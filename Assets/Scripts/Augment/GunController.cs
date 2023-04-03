using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
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

    public Transform projectileOutput;

    // All the stats of the gun and projectile
    public GunStats stats { get; set; }

    // Inputs
    public bool triggerHeld, triggerPressed;

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
        onReload?.Invoke(stats);
        stats.Ammo = Mathf.Min(stats.Ammo + amount, stats.magazineSize);
    }

    private void FireGun()
    {
        if (stats.Ammo <= 0)
            return;
        stats.Ammo--;

        projectile.InitializeProjectile(stats);
    }
}
