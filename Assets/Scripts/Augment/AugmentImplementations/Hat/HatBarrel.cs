using UnityEngine;
using UnityEngine.VFX;
using System.Linq;
/// <summary>
/// Class that ties the functions and properties of the completed gun to the animations of the hat barrel
/// </summary>
public class HatBarrel : ProjectileController
{
    [SerializeField]
    private AugmentAnimator animator;

    [SerializeField]
    private int maxHatProjectiles = 300;

    [SerializeField]
    private float hatMaxDistance = 20f;

    [SerializeField]
    private float hatSpeed = 10f;

    [SerializeField]
    private float hatSize = .2f;

    private ProjectileState[] projectiles;

    private ProjectileState loadedProjectile;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;

    [SerializeField]
    private VisualEffect hatVfx;

    protected override void Awake()
    {
        base.Awake();
        projectiles = new ProjectileState[maxHatProjectiles];

        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;

        positionActiveTexture = new VFXTextureFormatter(maxHatProjectiles);

        hatVfx.SetTexture("Positions", positionActiveTexture.Texture);
        hatVfx.SetInt("MaxParticleCount", maxHatProjectiles);
        hatVfx.SendEvent("OnPlay");
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
        loadedProjectile.maxDistance = this.hatMaxDistance;

        animator.OnFire(stats.Ammo);
    }

    public void ReleaseLoadedHat()
    {
        if (loadedProjectile == null) return;

        loadedProjectile.active = true;
        loadedProjectile.speed = hatSpeed;
        OnProjectileInit?.Invoke(ref loadedProjectile, stats);
        for (int i = 0; i < maxHatProjectiles; i++)
        {
            if (projectiles[currentStateIndex] == null || !projectiles[currentStateIndex].active)
            {
                loadedProjectile.initializationTime = Time.fixedTime;
                loadedProjectile.position = projectileOutput.position;
                loadedProjectile.direction = projectileRotation * projectileOutput.forward;
                loadedProjectile.rotation = projectileRotation * projectileOutput.rotation;
                loadedProjectile.size = hatSize;

                projectiles[currentStateIndex] = loadedProjectile;
                // Sets initial position of the projectile
                positionActiveTexture.setValue(i, loadedProjectile.position);
                positionActiveTexture.setAlpha(i, 1f);

                // Neccessary to update the actual texture, so the vfx gets the new info
                positionActiveTexture.ApplyChanges();

                currentStateIndex = (currentStateIndex + 1) % maxHatProjectiles;
                loadedProjectile = null;

                return;
            }
            currentStateIndex = (currentStateIndex + 1) % maxHatProjectiles;
        }
    }

    private void FixedUpdate()
    {
        if (!gunController)
        {
            return;
        }
        for (int i = 0; i < maxHatProjectiles; i++)
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

        if (collisions.Length > 0 && !collisions[0].gameObject.CompareTag("IgnoreCollider"))
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
