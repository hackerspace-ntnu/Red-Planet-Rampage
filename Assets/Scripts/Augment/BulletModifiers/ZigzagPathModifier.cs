using UnityEngine;

public class ZigzagPathModifier : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private float interval = .3f;

    [SerializeField]
    private float radius = .3f;

    private const string nextOffsetId = "nextZigzagOffset";
    private const string lastOffsetId = "lastZigzagOffset";
    private const string lastDistanceId = "lastZigzagDistance";

    public Priority GetPriority() => Priority.EXTENSION;

    private Vector2 RandomOffset(Vector2 lastOffset)
    {
        var random = Random.insideUnitCircle * radius;
        // Restrict to +/-[0.3, 1]
        random = new Vector2(Mathf.Sign(random.x) * (.7f * Mathf.Abs(random.x) + radius * .3f), Mathf.Sign(random.y) * (.7f * Mathf.Abs(random.y) + radius * .3f));
        // Prevent the new offset from being on the same half of the unit circle as the previous one.
        while (Vector2.Dot(random, lastOffset) > 0)
        {
            random = Vector2.Perpendicular(random);
        }
        return random;
    }

    public void AddZigzagDisplacement(float distance, ref ProjectileState state)
    {
        if (state.distanceTraveled > interval + (float)state.additionalProperties[lastDistanceId])
        {
            state.additionalProperties[lastOffsetId] = state.additionalProperties[nextOffsetId];
            state.additionalProperties[nextOffsetId] = RandomOffset((Vector2)state.additionalProperties[lastOffsetId]);
            state.additionalProperties[lastDistanceId] = state.distanceTraveled;
        }
        var offset = Vector2.Lerp((Vector2)state.additionalProperties[lastOffsetId], (Vector2)state.additionalProperties[nextOffsetId], (distance % interval) / interval);
        state.position += state.rotation * new Vector3(offset.x, offset.y, 0);
    }

    public void SetInitialDirection(ref ProjectileState state, GunStats stats)
    {
        state.additionalProperties[lastOffsetId] = Vector2.zero;
        state.additionalProperties[nextOffsetId] = RandomOffset(Vector2.zero);
        state.additionalProperties[lastDistanceId] = 0f;
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.UpdateProjectileMovement += AddZigzagDisplacement;
        projectile.OnProjectileInit += SetInitialDirection;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.UpdateProjectileMovement -= AddZigzagDisplacement;
        projectile.OnProjectileInit -= SetInitialDirection;
    }
}
