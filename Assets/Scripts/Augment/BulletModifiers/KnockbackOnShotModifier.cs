using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class KnockbackOnShotModifier: GunExtension, ProjectileModifier
{
    [SerializeField]
    private float pushPower;

    [SerializeField]
    private float perBulletExtraForce = 100f;

    [SerializeField]
    private float EnemyForceMultiplier = 2f;

    private float bulletAmount = 1f;

    private GunController gunController;

    private float calculatedPushPower;

    void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Fire not attached to gun parent!");
            return;
        }
    }
public void Attach(ProjectileController projectile)
    {
        projectile.OnProjectileInit += KnockAwayOnShot;
        projectile.OnHitboxCollision += KnockAwayTargets;
        var bulletController = projectile.gameObject.GetComponent<BulletController>();
        bulletAmount = bulletController == null || bulletController.BulletsPerShot == 0 ? 1f : bulletController.BulletsPerShot;
        calculatedPushPower = (pushPower / bulletAmount) * (1f + (float)Math.Log10(bulletAmount));
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnProjectileInit -= KnockAwayOnShot;
        projectile.OnHitboxCollision -= KnockAwayTargets;
    }

    public void KnockAwayOnShot(ref ProjectileState state, GunStats stats)
    {
        Vector3 normal = -gunController.transform.forward;

        gunController.player.GetComponent<Rigidbody>().AddForce(normal * calculatedPushPower, ForceMode.Impulse);
    }

    public void KnockAwayTargets(HitboxController controller, ref ProjectileState state)
    {
        Vector3 normal = gunController.transform.forward;

        controller.health.GetComponent<Rigidbody>().AddForce(normal * calculatedPushPower * 2f, ForceMode.Impulse);
    }
}
