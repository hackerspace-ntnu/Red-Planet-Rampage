using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class KnockbackOnShotModifier: MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private float pushPower;

    [SerializeField]
    private float radius;

    [SerializeField]
    private KnockbackEffect knockBackEffect;

    [SerializeField]
    private float perBulletExtraForce = 100f;

    private float bulletAmount = 1f;

    [SerializeField]
    private Transform[] knockbackNormal;

    private PlayerManager source;


    public void Attach(ProjectileController projectile)
    {
        projectile.OnProjectileInit += KnockAwayOnShot;
        var bulletController = projectile.gameObject.GetComponent<BulletController>();
        bulletAmount = bulletController == null || bulletController.BulletsPerShot == 0 ? 1f : bulletController.BulletsPerShot;

        source = projectile.player;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnProjectileInit -= KnockAwayOnShot;
    }

    public void KnockAwayOnShot(ref ProjectileState state, GunStats stats)
    {
        Vector3 ab = knockbackNormal[0].transform.position - transform.position;
        Vector3 ac = knockbackNormal[1].transform.position - transform.position;
        Vector3 normal = Vector3.Cross(ab, ac);

        knockBackEffect.KnockAwayTargetsDirectional((pushPower / bulletAmount) * (1f + (float) Math.Log10(bulletAmount)), normal, source, radius);
    }
}
