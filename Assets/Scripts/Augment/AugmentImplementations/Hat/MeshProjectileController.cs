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

    [FormerlySerializedAs("maxHatProjectiles")] [SerializeField]
    private int maxProjectiles = 300;

    [FormerlySerializedAs("hatMaxDistance")] [SerializeField]
    private float maxDistance = 20f;

    [FormerlySerializedAs("hatSpeed")] [SerializeField]
    private float speed = 10f;

    [FormerlySerializedAs("hatSize")] [SerializeField]
    private float size = .2f;

    private ProjectileState[] projectiles;

    private ProjectileState loadedProjectile;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;

    [FormerlySerializedAs("hatVfx")] [SerializeField]
    private VisualEffect vfx;

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
        vfx.SendEvent("OnPlay");

        animator.OnFireAnimationEnd += FireProjectile;
    }

    protected override void OnInitialize(GunStats gunstats)
    {
        animator.OnInitialize(gunstats);
    }

    protected override void OnReload(GunStats gunstats)
    {
        animator.OnReload(gunstats.Ammo);
    }

    public override void InitializeProjectile(GunStats stats)
    {
        loadedProjectile = new ProjectileState(stats, projectileOutput);
        loadedProjectile.maxDistance = this.maxDistance;

        animator.OnFire(stats.Ammo);
    }

    private void FireProjectile()
    {
        if (loadedProjectile == null) return;

        loadedProjectile.active = true;
        loadedProjectile.speed = speed;
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

        if (collisions.Length > 0)
        {
            if (collisions[0].TryGetComponent<HitboxController>(out HitboxController hitbox))
            {
                OnColliderHit?.Invoke(collisions[0], ref state);
                OnHitboxCollision?.Invoke(hitbox, ref state);
                state.active = false;
                return;
            }
            if (state.distanceTraveled < maxDistanceBeforeStuck)
            {
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
}
