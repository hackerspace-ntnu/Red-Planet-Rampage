using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A modifier to stick a gameObject (stuckObject) to a target after hitting said target.
/// </summary>
public class StickyProjectileModifier : ProjectileModifier
{
    // Model to stick to target
    [SerializeField]
    private GameObject stuckObject;
    [SerializeField]
    private float stuckLifeTime = 5f;
    // Does stuck object trigger onHit?
    [SerializeField]
    private bool triggerOnHit = true;
    // How often does it trigger onHit (in seconds)?
    [SerializeField]
    private float onHitInterval = 1f;

    public void StickToTarget(HitboxController controller, ref ProjectileState state, GunStats stats)
    {
        var stuck = Instantiate(stuckObject, state.oldPosition, state.rotation, controller.transform);
        Destroy(stuck, stuckLifeTime);
    }
    public override void Attach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision += StickToTarget;
    }
    public override void Detach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision -= StickToTarget;
    }

}
