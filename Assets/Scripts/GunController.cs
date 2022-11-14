using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [HideInInspector]
    public GameObject projectile;

    // Where the gun fires the bullets from
    // Is automatically set by barrel or extension (if one i available)
    [HideInInspector]
    public Transform[] outputs;

    // Keeps track of when gun should be fired
    [HideInInspector]
    public FireRateController fireRateController;

    // All the stats of the gun and projectile
    public GunStats stats { get; set; }

    // Inputs
    public bool triggerHeld, triggerPressed;

    private void FixedUpdate()
    {
        if (fireRateController.shouldFire(triggerPressed, triggerHeld))
        {
            FireGun();
        }
    }
    private void FireGun()
    {
        foreach (var output in outputs)
        {
            for (int i = 0; i < Mathf.Max((int)stats.projectilesPerShot.Value(), 1); i++)
            {
                // Adds spread
                Quaternion dir = output.rotation;
                if (stats.projectileSpread > 0)
                {
                    Vector2 rand = Random.insideUnitCircle * stats.projectileSpread;
                    dir = dir * Quaternion.Euler(rand.x, rand.y, 0f);
                }
                // Makes projectile 
                // TODO: generalize this so that different methods of "Creating" bullets can be used to save performance
                Instantiate(projectile, output.position, dir).SetActive(true);
            }
        }
    }
}
