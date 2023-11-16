using System;
using UnityEngine;

public class KnockbackOnShotModifier: MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private float pushPower;

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
        projectile.OnHitboxCollision += KnockAwayTargets;
        var bulletController = projectile.gameObject.GetComponent<BulletController>();
        bulletAmount = bulletController == null || bulletController.BulletsPerShot == 0 ? 1f : bulletController.BulletsPerShot;
        calculatedPushPower = (pushPower / bulletAmount) * (1f + (float)Math.Log10(bulletAmount));
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision -= KnockAwayTargets;
    }

    private void KnockAwayTargets(HitboxController controller, ref ProjectileState state)
    {
        Vector3 normal = gunController.transform.forward;

        controller.health.GetComponent<Rigidbody>().AddForce(normal * calculatedPushPower * 2f, ForceMode.Impulse);
    }
}
