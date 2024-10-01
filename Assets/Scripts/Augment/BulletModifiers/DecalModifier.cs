using MiscExtensions;
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
    private ObjectPool<DecalProjector> decalPool;

    [Range(.2f, .8f)][SerializeField] private float depthOffsetFraction = .6f;

    [Range(0f, 1f)][SerializeField] private float stickToNormalFraction = 0;

    [Range(0f, 180f)][SerializeField] private float angleVariation = 180f;

    private const int allHitboxesAndGunsAndPlayersMask = (1 << 3) | (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    private void Awake()
    {
        decalPool = new ObjectPool<DecalProjector>(decal, 70);
    }

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

    private void OnHit(RaycastHit target, ref ProjectileState state)
    {
        // Avoid placing decals on players or their guns, as that leads to the heebie-jeebies
        if (((1 << target.collider.gameObject.layer) & allHitboxesAndGunsAndPlayersMask) > 0)
            return;
        // Specifically avoid players. This is still required, even with the layer check above.
        if (target.collider.TryGetComponent<HitboxController>(out var hitbox))
            if (hitbox.health && hitbox.health.TryGetComponent<PlayerManager>(out var _))
                return;
        // Avoid decals on other objects that are marked as unsuitable for placing bullet holes on
        // TODO add this check back if you have need for this tag!
        //if (target.collider.gameObject.CompareTag("NoDecal"))
        //    return;

        var spawnedDecal = decalPool.Get();

        // Place the decal some distance away from the target so that it is more likely to be projected onto a surface
        var depthOffset = decal.size.z * depthOffsetFraction;
        spawnedDecal.transform.position = target.ClosestPoint(state.position) - state.direction * depthOffset;
        spawnedDecal.transform.rotation = DetermineRotation(target.collider, state);
        spawnedDecal.transform.parent = target.collider.transform;

        // Rotate it randomly for basic variation
        spawnedDecal.transform.Rotate(Vector3.forward, Random.Range(-angleVariation, angleVariation));
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
