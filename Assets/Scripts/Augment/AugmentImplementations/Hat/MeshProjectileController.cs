using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using UnityEngine.Serialization;
using Mirror;
using VectorExtensions;

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
    protected float maxDistance = 20;

    private const float baseSpeed = 20;

    [FormerlySerializedAs("hatSize")]
    [SerializeField]
    private float size = .2f;

    [SerializeField]
    private float visualSize = 80f;

    [Tooltip("Rotation offset applied to all particles")]
    [SerializeField]
    private Quaternion visualRotation = Quaternion.identity;

    [Tooltip("Check this box to avoid updating rotation as the projectile travels")]
    [SerializeField]
    private bool useOnlyInitialRotation = false;

    [Tooltip("Check this box to avoid updating rotation at all")]
    [SerializeField]
    private bool disableRotation = false;

    private ProjectileState[] projectiles;

    public Vector3[] ProjectilePositions =>
        projectiles.Where(p => p != null && p.active == true)
            .Select(p => p.position).ToArray();

    private ProjectileState loadedProjectile;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // Texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveBuffer;

    // Texture used to update the vfx rotation, RGB is used for eulerian direction
    private VFXTextureFormatter rotationBuffer;

    [Header("VFX")]

    [FormerlySerializedAs("hatVfx")]
    [SerializeField]
    private VisualEffect vfx;
    public VisualEffect Vfx => vfx;

    [Header("Ricochet")]

    [SerializeField]
    private bool shouldRicochet = true;

    [SerializeField]
    protected float maxDistanceBeforeStuck = 100;

    protected override void Awake()
    {
        base.Awake();
        projectiles = new ProjectileState[maxProjectiles];

        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;

        positionActiveBuffer = new(maxProjectiles);
        vfx.SetGraphicsBuffer("Positions", positionActiveBuffer.Buffer);
        if (!disableRotation)
        {
            rotationBuffer = new(maxProjectiles);
            vfx.SetGraphicsBuffer("Rotations", rotationBuffer.Buffer);
        }
        vfx.SetInt("MaxParticleCount", maxProjectiles);

        if (!gunController || !gunController.Player)
            return;
        animator.OnShotFiredAnimation += FireProjectile;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        positionActiveBuffer?.Dispose();
        rotationBuffer?.Dispose();
        UpdateProjectileMovement -= ProjectileMotions.MoveWithGravity;
        if (animator)
            animator.OnShotFiredAnimation -= FireProjectile;
    }

    protected override void OnInitialize(GunStats gunstats)
    {
        vfx.SetFloat("Size", visualSize * gunstats.ProjectileScale);
        vfx.SendEvent(VisualEffectAsset.PlayEventID);
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
        if (!authority)
            return;
        var data = new ProjectileFireData(this);
        OnNetworkInit?.Invoke(ref data, stats);
        CmdFireProjectile(data);
    }

    public void ClearProjectiles()
    {
        for (int i = 0; i < maxProjectiles; i++)
            if (projectiles[i] != null)
                projectiles[i].active = false;
    }

    [Command]
    private void CmdFireProjectile(ProjectileFireData parameters)
    {
        // TODO verify that this input is reasonable!
        RpcFireProjectile(parameters);
    }

    [ClientRpc]
    private void RpcFireProjectile(ProjectileFireData parameters)
    {
        if (loadedProjectile == null) return;

        loadedProjectile.active = true;
        loadedProjectile.speed = baseSpeed;
        foreach (var pair in parameters.additionalProperties)
            loadedProjectile.additionalProperties[pair.name] = pair.value;

        OnProjectileInit?.Invoke(ref loadedProjectile, stats);
        for (int i = 0; i < maxProjectiles; i++)
        {
            if (projectiles[currentStateIndex] == null || !projectiles[currentStateIndex].active)
            {
                loadedProjectile.initializationTime = Time.fixedTime;
                loadedProjectile.position = parameters.output;
                loadedProjectile.direction = parameters.direction;
                loadedProjectile.rotation = parameters.rotation;

                // Hat hitbox should be smaller when large :/
                // Otherwise they bounce off weirdly
                if (player.identity.Barrel.id == "Hatapult")
                    loadedProjectile.size = size * (1 + (stats.ProjectileScale - 1) * .3f);
                else
                    loadedProjectile.size = size * stats.ProjectileScale;

                loadedProjectile.additionalProperties["lastCollider"] = null;

                projectiles[currentStateIndex] = loadedProjectile;
                // Sets initial position of the projectile
                positionActiveBuffer.setValue(i, loadedProjectile.position);
                positionActiveBuffer.setAlpha(i, 1f);

                // Neccessary to update the actual texture, so the vfx gets the new info
                positionActiveBuffer.ApplyChanges();

                // Set initial rotation
                if (!disableRotation)
                {
                    rotationBuffer.setValue(i, (visualRotation * loadedProjectile.direction).ToEulerAngles());
                    rotationBuffer.ApplyChanges();
                }

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

        for (var i = 0; i < maxProjectiles; i++)
        {
            var state = projectiles[i];
            if (state is { active: true })
            {
                UpdateProjectile(state);
                positionActiveBuffer.setValue(i, state.position);
                if (!useOnlyInitialRotation && !disableRotation)
                    rotationBuffer.setValue(i, (visualRotation * state.direction).ToEulerAngles());
            }
            positionActiveBuffer.setAlpha(i, state is { active: true } ? 1f : 0f);
        }
        positionActiveBuffer.ApplyChanges();
        if (!useOnlyInitialRotation && !disableRotation)
            rotationBuffer.ApplyChanges();
    }

    protected virtual void UpdateProjectile(ProjectileState state)
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

        if (shouldRicochet && state.distanceTraveled < maxDistanceBeforeStuck)
        {
            OnRicochet?.Invoke(hits[0], ref state);
            state.position = state.oldPosition;
            state.direction = Vector3.Reflect(state.direction, hits[0].normal);
        }
        else
        {
            OnColliderHit?.Invoke(hits[0], ref state);
            state.active = false;
        }
    }
}
