using UnityEngine;
using UnityEngine.VFX;

public class BulletController : ProjectileController
{
    [SerializeField]
    private float maxDistance = 20;

    [SerializeField]
    private int collisionSamples = 30;

    private int vfxPositionsPerSample = 3;

    private float bulletSpeed = 50f;

    [SerializeField]
    private int bulletsPerShot = 1;

    private VFXTextureFormatter trailPosTexture;

    [SerializeField]
    private VisualEffect trail;

    [SerializeField]
    private VisualEffect flash;

    [SerializeField]
    private AugmentAnimator animator;

    private ProjectileState projectile = new ProjectileState();


    protected override void Awake()
    {
        base.Awake();
        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;
    }

    private void Start()
    {
        flash.transform.position = projectileOutput.position;
        trailPosTexture = new VFXTextureFormatter(vfxPositionsPerSample * collisionSamples * bulletsPerShot);
        trail.SetTexture("Position", this.trailPosTexture.Texture);
        trail.SetInt("StripLength", vfxPositionsPerSample * collisionSamples);
        trail.SetInt("TextureSize", vfxPositionsPerSample * collisionSamples * bulletsPerShot);
        trail.SetInt("TrailsPerEvent", bulletsPerShot);
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
        animator.OnFire(stats.Ammo);

        // TODO: Possibly standardize this better
        for (int k = 0; k < bulletsPerShot; k++)
        {

            Quaternion randomSpread = Quaternion.Lerp(Quaternion.identity, Random.rotation, stats.ProjectileSpread);

            projectile.active = true;
            projectile.distanceTraveled = 0f;
            projectile.damage = stats.ProjectileDamage;
            projectile.position = projectileOutput.position;
            projectile.oldPosition = projectileOutput.position;
            projectile.direction = randomSpread * projectileRotation * projectileOutput.forward;
            projectile.maxDistance = this.maxDistance;
            projectile.rotation = randomSpread * projectileRotation * projectileOutput.rotation;
            projectile.initializationTime = Time.fixedTime;
            projectile.speedFactor = stats.ProjectileSpeedFactor;
            projectile.gravity = stats.ProjectileGravityModifier * 9.81f;
            projectile.additionalProperties.Clear();
            projectile.hitHealthControllers.Clear();

            OnProjectileInit?.Invoke(ref projectile, stats);

            projectile.speed = bulletSpeed * stats.ProjectileSpeedFactor;

            int sampleNum = 0;

            while (sampleNum < collisionSamples && projectile.active)
            {

                projectile.oldPosition = projectile.position;
                projectile.lastUpdateTime = Time.time;


                for (int j = 0; j < vfxPositionsPerSample; j++)
                {
                    trailPosTexture.setValue(sampleNum * vfxPositionsPerSample + j + k * vfxPositionsPerSample * collisionSamples, projectile.position);
                    UpdateProjectileMovement?.Invoke(maxDistance / (collisionSamples * vfxPositionsPerSample), ref projectile);
                }

                RaycastHit[] collisions = ProjectileMotions.GetPathCollisions(projectile, collisionLayers);

                if (collisions.Length > 0)
                {
                    var collider = collisions[0].collider;
                    OnColliderHit?.Invoke(collider, ref projectile);
                    HitboxController hitbox = collider.GetComponent<HitboxController>();
                    projectile.position = collisions[0].point;
                    if (hitbox != null)
                    {
                        OnHitboxCollision?.Invoke(hitbox, ref projectile);
                    }
                    projectile.active = false;
                    sampleNum += 1;
                    trailPosTexture.setValue(sampleNum * vfxPositionsPerSample + k * vfxPositionsPerSample * collisionSamples, projectile.position);

                }
                else
                {
                    sampleNum += 1;
                }
            }

            for (int i = sampleNum * vfxPositionsPerSample + 1; i < collisionSamples * vfxPositionsPerSample; i++)
            {
                trailPosTexture.setValue(i + k * vfxPositionsPerSample * collisionSamples, projectile.position);
            }

            trailPosTexture.ApplyChanges();
            // Play the flash and trail
        }
        trail.SendEvent("OnPlay");
        flash.SendEvent("OnPlay");
    }

    public Vector3 LerpPos(ProjectileState state)
    {
        var diff = (Time.time - state.lastUpdateTime) / Time.fixedDeltaTime;
        return Vector3.Lerp(state.oldPosition, state.position, diff);
    }

    // Not currently used by anything, and looks ugly as hell, but if we need a quadratic interpolator, here
    // Math is probably a bit fuzzy, hence ugly

    //public Vector3 QerpPos()
    //{
    //    var diff = (Time.time - state.lastUpdateTime) / Time.fixedDeltaTime;
    //
    //    return ((state.olderPosition + state.position) * 0.5f - state.oldPosition) * diff * diff +
    //        (state.position - state.olderPosition) * diff + state.oldPosition;
    //}
}

