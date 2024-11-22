using UnityEngine;
using MiscExtensions;
using TransformExtensions;

/// <summary>
/// A modifier to stick a gameObject (stuckObject) to a target after hitting said target.
/// </summary>
public class StickyProjectileModifier : MonoBehaviour, ProjectileModifier
{
    // Model to stick to target
    [SerializeField]
    private StuckObject stuckObject;
    // Should object be despawned after stuckLifeTime seconds?
    [SerializeField]
    private bool isDespawnedAfterTime = true;
    [SerializeField]
    private float stuckLifeTime = 5f;
    // Does stuck object trigger onHit?
    [SerializeField]
    private bool triggerOnHit = true;
    // How often does it trigger onHit (in seconds)?
    [SerializeField]
    private float onHitInterval = 1f;
    [SerializeField]
    private LayerMask affectedLayers = 0;

    [SerializeField]
    private Priority priority = Priority.ARBITRARY;

    private PlayerManager source;

    private Vector3 scale = Vector3.one;

    private ObjectPool<StuckObject> stuckObjects;

    public delegate void StickyModifierEvent(StuckObject stuckObject);
    public StickyModifierEvent OnStuckToTarget;

    public Priority GetPriority()
    {
        return priority;
    }

    public void Attach(ProjectileController projectile)
    {
        stuckObjects = new ObjectPool<StuckObject>(stuckObject);
        projectile.OnColliderHit += StickToTarget;
        scale = stuckObject.transform.localScale * projectile.stats.ProjectileScale;
        source = projectile.player;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnColliderHit -= StickToTarget;
        OnStuckToTarget = null;
        stuckObjects.Flush();
        stuckObjects = null;
    }

    public void StickToTarget(RaycastHit hit, ref ProjectileState state)
    {
        if (!(affectedLayers == (affectedLayers | (1 << hit.collider.gameObject.layer))))
            return;

        var stuck = isDespawnedAfterTime ? 
            stuckObjects.GetAndReturnLater(stuckLifeTime) 
            : stuckObjects.Get();

        stuck.transform.position = hit.ClosestPoint(state.oldPosition);
        stuck.transform.localScale = scale;
        stuck.transform.SetParent(hit.collider.transform, true);

        OnStuckToTarget?.Invoke(stuck);

        if (stuck.TryGetComponent<ContinuousDamage>(out var continuousDamage))
            continuousDamage.source = source;
    }

    private void OnDestroy()
    {
        stuckObjects.Flush();
        stuckObjects = null;
    }
}
