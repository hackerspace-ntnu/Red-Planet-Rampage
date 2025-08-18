using System;
using UnityEngine;

public class RecoilModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private float pushPower;

    private float bulletAmount = 1f;

    private GunController gunController;

    private float calculatedPushPower;

    public void Attach(ProjectileController projectile)
    {
        gunController = projectile.GunController;
        projectile.OnProjectileInit += KnockAwayOnShot;
        bulletAmount = projectile.stats.ProjectilesPerShot;
        calculatedPushPower = (pushPower / bulletAmount) * (1f + (float)Math.Log10(bulletAmount));
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnProjectileInit -= KnockAwayOnShot;
    }

    private void KnockAwayOnShot(ref ProjectileState state, GunStats stats)
    {
        if (!gunController.Player)
            return;

        Vector3 normal = -gunController.transform.forward;

        gunController.Player.GetComponent<Rigidbody>().AddForce(normal * calculatedPushPower, ForceMode.Impulse);
    }
}
