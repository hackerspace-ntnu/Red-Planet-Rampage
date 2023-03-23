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

    public GunEvent onInitialize;
    public GunEvent onFire;

    private void FixedUpdate()
    {
        if (fireRateController.shouldFire(triggerPressed, triggerHeld))
        {
            FireGun();
        }
    }

    private void FireGun()
    {
        onFire?.Invoke(stats);

        projectile.InitializeProjectile(stats);

        //foreach (var output in outputs)
        //{
        //    for (int i = 0; i < Mathf.Max((int)stats.ProjectilesPerShot.Value(), 1); i++)
        //    {
        //        // Adds spread
        //        Quaternion dir = output.rotation;
        //        if (stats.ProjectileSpread > 0)
        //        {
        //            Vector2 rand = Random.insideUnitCircle * stats.ProjectileSpread;
        //            dir = dir * Quaternion.Euler(rand.x, rand.y, 0f);
        //        }
        //        // Makes projectile 
        //        // TODO: generalize this so that different methods of "Creating" bullets can be used to save performance
        //        var firedProjectile = Instantiate(projectile, output.position, dir);
        //        firedProjectile.GetComponent<ProjectileDamageController>().player = player;
        //        firedProjectile.SetActive(true);
        //    }
        //}
    }
}
