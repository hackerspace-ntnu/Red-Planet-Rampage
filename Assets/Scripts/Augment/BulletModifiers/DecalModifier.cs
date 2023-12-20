using UnityEngine;
using UnityEngine.Rendering.Universal;

internal enum MarkMode
{
    ON_HIT,
    ON_RICOCHET,
    BOTH
}

public class DecalModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField] private MarkMode mode = MarkMode.ON_HIT;

    // Note that the decal has to be quite deep (like 1 unit or so) because of the somewhat imprecise colliders we have
    [SerializeField] private DecalProjector decal;

    [Range(.2f, .8f)] [SerializeField] private float depthOffsetFraction = .6f;

    [Range(0f, 1f)] [SerializeField] private float stickToNormalFraction = 0;

    [Range(0f, 180f)] [SerializeField] private float angleVariation = 180f;

    [SerializeField] private float lifetime = 60;

    public void Attach(ProjectileController projectile)
    {
        if (mode is MarkMode.ON_HIT or MarkMode.BOTH)
        {
            projectile.OnColliderHit += OnHit;
        }
        if (mode is MarkMode.ON_RICOCHET or MarkMode.BOTH)
        {
            projectile.OnRicochet += OnHit;
        }
    }

    public void Detach(ProjectileController projectile)
    {
        if (mode is MarkMode.ON_HIT or MarkMode.BOTH)
        {
            projectile.OnColliderHit -= OnHit;
        }
        if (mode is MarkMode.ON_RICOCHET or MarkMode.BOTH)
        {
            projectile.OnRicochet -= OnHit;
        }
    }

    private void OnHit(Collider target, ref ProjectileState state)
    {
        // Place the decal some distance away from the target so that it is more likely to be projected onto a surface
        var depthOffset = decal.size.z * depthOffsetFraction;
        var position = target.ClosestPoint(state.position) - state.direction * depthOffset;
        var rotation = DetermineRotation(target, state);

        var spawnedDecal = Instantiate(decal, position, rotation);
        spawnedDecal.transform.parent = target.transform;

        // Rotate it randomly for basic variation
        spawnedDecal.transform.Rotate(Vector3.forward, Random.Range(-angleVariation, angleVariation));

        Destroy(spawnedDecal, lifetime);
    }

    private Quaternion DetermineRotation(Collider target, ProjectileState state)
    {
        var rotation = state.rotation;
        var shouldUseNormal = stickToNormalFraction > 0;
        var raycastDidHit = target.Raycast(new Ray(state.position - state.direction, state.direction),
            out var hit, 100);
        if (shouldUseNormal && raycastDidHit)
        {
            rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(-hit.normal), stickToNormalFraction);
        }

        return rotation;
    }
}