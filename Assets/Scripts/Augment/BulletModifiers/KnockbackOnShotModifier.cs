using System;
using UnityEngine;

public class KnockbackOnShotModifier : MonoBehaviour, ProjectileModifier
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
        bulletAmount = projectile.stats.ProjectilesPerShot;
        calculatedPushPower = (pushPower / bulletAmount) * (1f + (float)Math.Log10(bulletAmount));
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision -= KnockAwayTargets;
    }

    private void KnockAwayTargets(HitboxController controller, ref ProjectileState state)
    {
        Vector3 normal = gunController.transform.forward;

        if (controller.health.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            // If AI, enable physics for a small time frame
            if (controller.health.Player is AIManager ai)
                StartCoroutine(ai.WaitAndToggleAgent());
            rigidbody.AddForce(normal * calculatedPushPower * 2f, ForceMode.Impulse);
        }
    }
}
