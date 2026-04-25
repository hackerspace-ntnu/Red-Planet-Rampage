using UnityEngine;
using System.Linq;

/// <summary>
/// Class that processes and renders penetrating mesh projectiles
/// </summary>
public class PenetratingProjectileController : MeshProjectileController
{
    protected override void UpdateProjectile(ProjectileState state)
    {
        state.oldPosition = state.position;
        UpdateProjectileMovement?.Invoke(state.speed * state.speedFactor * Time.fixedDeltaTime, ref state);
        OnProjectileTravel?.Invoke(ref state);
        Collider lastCollider = (Collider)state.additionalProperties["lastCollider"];

        if (state.distanceTraveled > state.maxDistance)
        {
            state.active = false;
        }

        var hits = ProjectileMotions.GetPathCollisions(state, collisionLayers).Where(p => p.collider != lastCollider).OrderBy(p => p.distance).ToArray();

        state.additionalProperties["lastCollider"] = hits.Length > 0 ? hits[0].collider : null;

        if (hits.Length <= 0) return;

        if (hits[0].collider.TryGetComponent<HitboxController>(out var hitbox))
        {
            var hasHitYourselfTooEarly = hitbox.health.Player == player && state.distanceTraveled < GunController.InvulnerabilityDistance;
            if (hasHitYourselfTooEarly)
                return;

            OnColliderHit?.Invoke(hits[0], ref state);
            OnHitboxCollision?.Invoke(hitbox, ref state);
            state.active = false;
            return;
        }

        if (state.distanceTraveled < maxDistanceBeforeStuck)
        {
            // TODO tried this, didn't help...
            // var preciseHit = Physics.Raycast(state.oldPosition, state.direction, out var hit, state.maxDistance, collisionLayers);
            // if (preciseHit) hits[0] = hit;

            var distanceLeft =  maxDistanceBeforeStuck - state.distanceTraveled;
            var oppositeHits = Physics.RaycastAll(state.oldPosition + state.direction * distanceLeft, -state.direction,
                distanceLeft - state.size - 0.001f, collisionLayers)
                .Where(p => p.collider != lastCollider)
                .OrderByDescending(p => p.distance)
                .ToArray();
            if (oppositeHits.Length > 0 && oppositeHits[0].point.magnitude > .0001f)
            {
                // Ricochet (kinda) from where we hit
                OnRicochet?.Invoke(hits[0], ref state);
                // Also trigger ricochet on exit point
                OnRicochet?.Invoke(oppositeHits[0], ref state);
                // Teleport to hit on opposite side
                state.position = oppositeHits[0].point + state.size * state.direction;
                return;
            }
            else if (hits[0].point.sqrMagnitude > .001f)
            {
                // Ignore and march onward
                state.position += state.size * state.direction;
                return;
            }
        }
        // Give up
        OnColliderHit?.Invoke(hits[0], ref state);
        state.active = false;
    }
}
