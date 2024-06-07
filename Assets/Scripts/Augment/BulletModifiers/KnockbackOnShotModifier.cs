using System;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackOnShotModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private float pushPower;

    private float bulletAmount = 1f;

    private GunController gunController;

    private float calculatedPushPower;

    private (ProjectileState, List<HitboxController>) collidersHitWithShot;

    private void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;
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
        if (collidersHitWithShot.Item1 == state && collidersHitWithShot.Item2.Contains(controller))
            return;

        if (collidersHitWithShot.Item1 != state)
        {
            collidersHitWithShot.Item2.Clear();
            collidersHitWithShot.Item1 = state;
        }

        collidersHitWithShot.Item2.Add(controller);
        if (controller.health.TryGetComponent<Rigidbody>(out var rigidbody))
        {
            // If AI, enable physics for a small time frame
            if (controller.health.Player is AIManager ai)
                StartCoroutine(ai.WaitAndToggleAgent());
            rigidbody.AddForce(normal * calculatedPushPower * 2f, ForceMode.Impulse);
        }
    }
}
