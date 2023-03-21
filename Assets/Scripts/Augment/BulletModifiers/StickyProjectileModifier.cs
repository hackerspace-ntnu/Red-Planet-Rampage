using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A modifier to stick a gameObject (stuckObject) to a target after hitting said target.
/// </summary>
public class StickyProjectileModifier : MonoBehaviour, ProjectileModifier
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

    [SerializeField]
    private Priority priority = Priority.ARBITRARY;

    public Priority GetPriority()
    {
        return priority;
    }

    public ref ProjectileController ModifyProjectile(ref ProjectileController projectile)
    {
        projectile.OnHitboxCollision += StickToTarget;
        return ref projectile;
    }

    public void StickToTarget(HitboxController hitboxController, ref ProjectileState state, GunStats stats)
    {
        var stuck = Instantiate(stuckObject, state.olderPosition, state.rotation, hitboxController.transform);
        Destroy(stuck, stuckLifeTime);
    }
}
