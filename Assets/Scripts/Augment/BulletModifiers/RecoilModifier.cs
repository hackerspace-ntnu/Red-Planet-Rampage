using System;
using UnityEngine;

public class RecoilModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private float pushPower;

    [SerializeField]
    private float baseFireRateAdder = 2f;

    private float bulletAmount = 1f;

    private GunController gunController;

    private float calculatedPushPower;

    private void Awake()
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
        var bulletController = projectile.gameObject.GetComponent<BulletController>();
        bulletAmount = bulletController == null || bulletController.BulletsPerShot == 0 ? 1f : bulletController.BulletsPerShot;
        calculatedPushPower = (pushPower / bulletAmount) * (1f + (float)Math.Log10(bulletAmount));
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnProjectileInit -= KnockAwayOnShot;
    }

    private void KnockAwayOnShot(ref ProjectileState state, GunStats stats)
    {
        Vector3 normal = -gunController.transform.forward;

        float calculatedPushPowerForPlayer = (calculatedPushPower / stats.Firerate) * (baseFireRateAdder + (float)Math.Log(stats.Firerate));

        gunController.player.GetComponent<Rigidbody>().AddForce(normal * calculatedPushPowerForPlayer, ForceMode.Impulse);

    }
}
