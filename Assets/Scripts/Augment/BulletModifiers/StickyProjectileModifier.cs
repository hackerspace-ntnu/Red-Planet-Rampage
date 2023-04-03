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

    public void Attach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision += StickToTarget;
    }
    public void Detach(ProjectileController projectile)
    {
        projectile.OnHitboxCollision -= StickToTarget;
    }
    public void StickToTarget(HitboxController hitboxController, ref ProjectileState state)
    {
        var stuck = Instantiate(stuckObject, state.oldPosition, state.rotation, hitboxController.transform);
        Destroy(stuck, stuckLifeTime);
    }
}
