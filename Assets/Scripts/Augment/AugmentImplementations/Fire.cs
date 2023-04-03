using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : GunExtension
{
    [SerializeField]
    private GameObject fire;

    private GunController gunController;

    void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Fire not attached to gun parent!");
            return;
        }
        gunController.onInitializeGun += AddFireToProjectile;
    }

    private void AddFireToProjectile(GunStats gunstats)
    {
        Instantiate(fire, gunController.projectile.transform);
    }
}
