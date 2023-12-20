using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
using UnityEngine.Serialization;

/// <summary>
/// Class that processes and renders mesh projectiles
/// </summary>
public class MeshProjectileController : ProjectileController
{
    [SerializeField]
    private AugmentAnimator animator;
    
    [Header("Parameters")]

    [FormerlySerializedAs("maxHatProjectiles")] [SerializeField]
    private int maxProjectiles = 300;

    [FormerlySerializedAs("hatMaxDistance")] [SerializeField]
    private float maxDistance = 20;

    private const float baseSpeed = 20;

    [FormerlySerializedAs("hatSize")] [SerializeField]
    private float size = .2f;

    [SerializeField]
    private float visualSize = 80f;

    private ProjectileState[] projectiles;

    private ProjectileState loadedProjectile;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;

    [Header("VFX")]

    [FormerlySerializedAs("hatVfx")] [SerializeField]
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

        positionActiveTexture = new VFXTextureFormatter(maxProjectiles);

        vfx.SetTexture("Positions", positionActiveTexture.Texture);
        vfx.SetInt("MaxParticleCount", maxProjectiles);
        vfx.SetFloat("Size", visualSize);
        vfx.SendEvent("OnPlay");

        animator.OnFireAnimationEnd += FireProjectile;
    }

    protected override void OnInitialize(GunStats gunstats)
    {
        animator.OnInitialize(gunstats);
    }

    protected override void OnReload(GunStats gunstats)
    {
        animator.OnReload(gunstats);
    }

    public override void InitializeProjectile(GunStats stats)
    {
        loadedProjectile = new ProjectileState(stats, projectileOutput);
        loadedProjectile.maxDistance = maxDistance;

        animator.OnFire(stats);
    }

    private void FireProjectile()
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
                loadedProjectile.position = projectileOutput.position;
                loadedProjectile.direction = projectileRotation * projectileOutput.forward;
                loadedProjectile.rotation = projectileRotation * projectileOutput.rotation;
                loadedProjectile.size = size;

                projectiles[currentStateIndex] = loadedProjectile;
                // Sets initial position of the projectile
                positionActiveTexture.setValue(i, loadedProjectile.position);
                positionActiveTexture.setAlpha(i, 1f);

                // Neccessary to update the actual texture, so the vfx gets the new info
                positionActiveTexture.ApplyChanges();

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
        {
            return;
        }
        for (int i = 0; i < maxProjectiles; i++)
        {
            var state = projectiles[i];
            if (state != null && state.active)
            {
                UpdateProjectile(state);
                positionActiveTexture.setValue(i, state.position);

            }
            positionActiveTexture.setAlpha(i, state != null && state.active ? 1f : 0f);
        }
        positionActiveTexture.ApplyChanges();
    }

    private void UpdateProjectile(ProjectileState state)
    {
        state.oldPosition = state.position;
        UpdateProjectileMovement?.Invoke(state.speed * state.speedFactor * Time.fixedDeltaTime, ref state);
        OnProjectileTravel?.Invoke(ref state);


        if (state.distanceTraveled > state.maxDistance)
        {
            state.active = false;
        }

        var collisions = ProjectileMotions.GetPathCollisions(state, collisionLayers).Select(x => x.collider).ToArray();

        if (collisions.Length <= 0) return;

        if (collisions[0].TryGetComponent<HitboxController>(out HitboxController hitbox))
        {
            OnColliderHit?.Invoke(collisions[0], ref state);
            OnHitboxCollision?.Invoke(hitbox, ref state);
            state.active = false;
            return;
        }
        if (shouldRicochet && state.distanceTraveled < maxDistanceBeforeStuck)
        {
            OnRicochet?.Invoke(collisions[0], ref state);
            Physics.Raycast(state.oldPosition, state.direction, out RaycastHit hitInfo);
            state.direction = Vector3.Reflect(state.direction, hitInfo.normal);
        }
        else
        {
            OnColliderHit?.Invoke(collisions[0], ref state);
            state.active = false;
        }
    }
}
