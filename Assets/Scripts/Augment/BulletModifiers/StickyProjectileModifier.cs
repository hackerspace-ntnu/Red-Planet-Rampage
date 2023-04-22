using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TransformExtensions;

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

    private PlayerManager source;

    public Priority GetPriority()
    {
        return priority;
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.OnColliderHit += StickToTarget;
        source = projectile.player;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= StickToTarget;
    }

    public void StickToTarget(Collider collider, ref ProjectileState state)
    {
        var stuck = Instantiate(stuckObject, state.position, state.rotation, null);
        stuck.transform.ParentUnscaled(collider.transform);
        if (stuck.TryGetComponent<ContinuousDamage>(out var continuousDamage))
        {
            continuousDamage.source = source;
        }
        Destroy(stuck, stuckLifeTime);
    }
}
