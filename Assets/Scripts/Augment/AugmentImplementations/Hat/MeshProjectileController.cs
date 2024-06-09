using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using UnityEngine.Serialization;
using Mirror;

/// <summary>
/// Class that processes and renders mesh projectiles
/// </summary>
public class MeshProjectileController : ProjectileController
{
    [SerializeField]
    private AugmentAnimator animator;

    [Header("Parameters")]

    [FormerlySerializedAs("maxHatProjectiles")]
    [SerializeField]
    private int maxProjectiles = 300;

    [FormerlySerializedAs("hatMaxDistance")]
    [SerializeField]
    private float maxDistance = 20;

    private const float baseSpeed = 20;

    [FormerlySerializedAs("hatSize")]
    [SerializeField]
    private float size = .2f;

    [SerializeField]
    private float visualSize = 80f;

    private ProjectileState[] projectiles;

    private ProjectileState loadedProjectile;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    [SerializeField]
    private VFXTextureFormatter positionActiveBuffer;

    [Header("VFX")]

    [FormerlySerializedAs("hatVfx")]
    [SerializeField]
    private VisualEffect vfx;
    public VisualEffect Vfx => vfx;

    [Header("Ricochet")]

    [SerializeField]
    private bool shouldRicochet = true;

    [SerializeField]
    private float maxDistanceBeforeStuck = 100;

    protected override void Awake()
    {
        base.Awake();
        projectiles = new ProjectileState[maxProjectiles];

        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;
        positionActiveBuffer.Initialize(maxProjectiles);
        vfx.SetGraphicsBuffer("Positions", positionActiveBuffer.Buffer);
        vfx.SetInt("MaxParticleCount", maxProjectiles);
        vfx.SetFloat("Size", visualSize);
        vfx.SendEvent(VisualEffectAsset.PlayEventID);

        if (!gunController || !gunController.Player)
            return;
        animator.OnShotFiredAnimation += FireProjectile;
    }

    protected override void OnInitialize(GunStats gunstats)
    {
        animator.OnInitialize(gunstats);
    }

    protected override void OnReload(GunStats gunstats)
    {
        animator.OnReload(gunstats);
    }

    public override void InitializeProjectile(GunStats stats, uint shotID)
    {
        loadedProjectile = new ProjectileState(stats, projectileOutput)
        {
            maxDistance = maxDistance,
            shotID = shotID
        };

        animator.OnFire(stats);
    }

    private void FireProjectile()
    {
        if (authority)
            CmdFireProjectile(projectileOutput.position, projectileRotation * projectileOutput.forward, projectileRotation * projectileOutput.rotation);
    }

    [Command]
    private void CmdFireProjectile(Vector3 output, Vector3 direction, Quaternion rotation)
    {
        // TODO verify that this input is reasonable!
        RpcFireProjectile(output, direction, rotation);
    }

    [ClientRpc]
    private void RpcFireProjectile(Vector3 output, Vector3 direction, Quaternion rotation)
    {
        if (loadedProjectile == null) return;

        loadedProjectile.active = true;
        loadedProjectile.speed = baseSpeed;

        OnProjectileInit?.Invoke(ref loadedProjectile, stats);
        for (int i = 0; i < maxProjectiles; i++)
        {

            if (projectiles[currentStateIndex] == null || !projectiles[currentStateIndex].active)
            {
                loadedProjectile.initializationTime = Time.fixedTime;
                loadedProjectile.position = output;
                loadedProjectile.direction = direction;
                loadedProjectile.rotation = rotation;
                loadedProjectile.size = size;
                loadedProjectile.additionalProperties["lastCollider"] = null;

                projectiles[currentStateIndex] = loadedProjectile;
                // Sets initial position of the projectile
                positionActiveBuffer.setValue(i, loadedProjectile.position);
                positionActiveBuffer.setAlpha(i, 1f);

                // Neccessary to update the actual texture, so the vfx gets the new info
                positionActiveBuffer.ApplyChanges();

                currentStateIndex = (currentStateIndex + 1) % maxProjectiles;
                loadedProjectile = null;

                return;
            }
            currentStateIndex = (currentStateIndex + 1) % maxProjectiles;
        }
    }

    private void FixedUpdate()
    {
        if (!gunController)
            return;

        for (int i = 0; i < maxProjectiles; i++)
        {
            var state = projectiles[i];
            if (state != null && state.active)
            {
                UpdateProjectile(state);
                positionActiveBuffer.setValue(i, state.position);

            }
            positionActiveBuffer.setAlpha(i, state != null && state.active ? 1f : 0f);
        }
        positionActiveBuffer.ApplyChanges();
    }

    private void UpdateProjectile(ProjectileState state)
    {
        state.oldPosition = state.position;
        UpdateProjectileMovement?.Invoke(state.speed * state.speedFactor * Time.fixedDeltaTime, ref state);
        OnProjectileTravel?.Invoke(ref state);
        Collider lastCollider = (Collider)state.additionalProperties["lastCollider"];

        if (state.distanceTraveled > state.maxDistance)
        {
            state.active = false;
        }

        var collisions = ProjectileMotions.GetPathCollisions(state, collisionLayers).Where(p => p.collider != lastCollider).ToArray();

        state.additionalProperties["lastCollider"] = collisions.Length > 0 ? collisions[0].collider : null;

        if (collisions.Length <= 0) return;


        if (collisions[0].collider.TryGetComponent<HitboxController>(out HitboxController hitbox))
        {
            var hasHitYourselfTooEarly = hitbox.health.Player == player && state.distanceTraveled < player.GunController.OutputTransitionDistance;
            if (hasHitYourselfTooEarly)
                return;

            OnColliderHit?.Invoke(collisions[0], ref state);
            OnHitboxCollision?.Invoke(hitbox, ref state);
            state.active = false;
            return;
        }

        if (shouldRicochet && state.distanceTraveled < maxDistanceBeforeStuck)
        {
            OnRicochet?.Invoke(collisions[0], ref state);
            state.position = state.oldPosition;
            state.direction = Vector3.Reflect(state.direction, collisions[0].normal);
        }
        else
        {
            OnColliderHit?.Invoke(collisions[0], ref state);
            state.active = false;
        }
    }
}
