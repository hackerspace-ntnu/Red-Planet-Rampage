using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [HideInInspector]
    public GameObject projectile;

    // Where the gun fires the bullets from
    // Is automatically set by barrel or extension (if one i available)
    public Transform[] outputs;

    // Keeps track of when gun should be fired
    [HideInInspector]
    public FireRateController fireRateController;

    public PlayerManager player;

    // All the stats of the gun and projectile
    public GunStats stats { get; set; }

    // Inputs
    public bool triggerHeld, triggerPressed;

    public delegate void GunEvent(GunStats gunStats);

    public GunEvent onInitialize;
    public GunEvent onFire;
    public GunEvent onReload;

    private void FixedUpdate()
    {
        if (fireRateController.shouldFire(triggerPressed, triggerHeld))
        {
            FireGun();
        }
    }

    /// <summary>
    /// Expects a percentage of ammunition to be reloaded.
    /// This percentage is normalized eg. min = 0, max = 1.
    /// </summary>
    /// <param name="percentageNormalized">Percentage of ammunition to be reloaded.</param>
    public void Reload(float percentageNormalized)
    {
        int amount = Mathf.Max(1, Mathf.FloorToInt(stats.magazineSize * percentageNormalized));
        onReload?.Invoke(stats);
        stats.Ammo = Mathf.Min(stats.Ammo + amount, stats.magazineSize);
    }

    private void FireGun()
    {
        if (stats.Ammo <= 0)
            return;
        stats.Ammo--;
        onFire?.Invoke(stats);
        foreach (var output in outputs)
        {
            for (int i = 0; i < Mathf.Max((int)stats.ProjectilesPerShot.Value(), 1); i++)
            {
                // Adds spread
                Quaternion dir = output.rotation;
                if (stats.ProjectileSpread > 0)
                {
                    Vector2 rand = Random.insideUnitCircle * stats.ProjectileSpread;
                    dir *= Quaternion.Euler(rand.x, rand.y, 0f);
                }
                // Makes projectile 
                // TODO: generalize this so that different methods of "Creating" bullets can be used to save performance
                var firedProjectile = Instantiate(projectile, output.position, dir, transform);
                firedProjectile.GetComponent<ProjectileController>().OnClone(projectile.GetComponent<ProjectileController>());
                firedProjectile.GetComponent<ProjectileDamageController>().player = player;
                firedProjectile.SetActive(true);
            }
        }
    }
}
