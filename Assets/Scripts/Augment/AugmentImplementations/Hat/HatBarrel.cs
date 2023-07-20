using UnityEngine;
using UnityEngine.VFX;
using System.Linq;

/// <summary>
/// Class that processes and renders mesh projectiles
/// </summary>
public class HatBarrel : ProjectileController
{
    [SerializeField]
    private AugmentAnimator animator;

    [SerializeField]
    private int maxProjectiles = 300;

    [SerializeField]
    private float maxDistance = 20f;

    [SerializeField]
    private float speed = 10f;

    [SerializeField]
    private float size = .2f;

    private ProjectileState[] projectiles;

    private ProjectileState loadedProjectile;

    [SerializeField]
    private VisualEffect trail;

    [SerializeField]
    private Mesh mesh;

    [SerializeField]
    private Material material;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;
    private VFXTextureFormatter directionActiveTexture;

    protected override void Awake()
    {
        base.Awake();
        projectiles = new ProjectileState[maxProjectiles];

        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;

        positionActiveTexture = new VFXTextureFormatter(maxProjectiles);
        directionActiveTexture = new VFXTextureFormatter(maxProjectiles);

        if (trail)
        {
            trail.SetTexture("Positions", positionActiveTexture.Texture);
            trail.SetTexture("Directions", directionActiveTexture.Texture);
            trail.SetInt("MaxParticleCount", maxProjectiles);
            trail.SendEvent("OnPlay");
        }

        animator.OnFireAnimationEnd += ReleaseLoadedHat;
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

    public void ReleaseLoadedHat()
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
                directionActiveTexture.setValue(i, loadedProjectile.direction);

                // Neccessary to update the actual texture, so the vfx gets the new info
                positionActiveTexture.ApplyChanges();
                directionActiveTexture.ApplyChanges();

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
                directionActiveTexture.setValue(i, state.direction);

            }
            positionActiveTexture.setAlpha(i, state != null && state.active ? 1f : 0f);
        }
        // render instanced meshes (TODO clean up)
        Graphics.RenderMeshInstanced(new RenderParams(material), mesh, 0, projectiles.Where(p => p != null).Select(p => Matrix4x4.Translate(p.position) * Matrix4x4.Rotate(p.rotation)).ToArray());
        positionActiveTexture.ApplyChanges();
        directionActiveTexture.ApplyChanges();
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
            state.active = false;
            OnColliderHit?.Invoke(collisions[0], ref state);
            HitboxController hitbox = collisions[0].GetComponent<HitboxController>();

            if (hitbox != null)
            {
                OnHitboxCollision?.Invoke(hitbox, ref state);
            }
        }
    }
}
