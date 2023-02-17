using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A modifier to stick a gameObject (stuckObject) to a target after hitting said target.
/// </summary>
public class StickyProjectileModifier : ProjectileModifier
{
    [SerializeField]
    private GameObject stuckObject;
    [SerializeField]
    private float stuckLifeTime = 5f;
    [SerializeField]
    private bool triggerOnHit = true;
    [SerializeField]
    private float onHitInterval = 1f;

    private ProjectileState projectileState;

    private void StickToTarget(HitboxController controller, ref ProjectileState state, GunStats stats)
    {
        projectileState = state;
        var stuck = Instantiate(stuckObject, transform);
        Destroy(stuck, stuckLifeTime);
    }
    void Start()
    {
        // TODO: Rewrite OnHitBoxCollision to use out instead of ref, because this is starting to get stupid.
        projectile.OnHitboxCollision += StickToTarget;
    }

}
