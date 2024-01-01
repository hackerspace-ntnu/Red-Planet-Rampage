using System;
using UnityEngine;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

public class BulletController : ProjectileController
{
    [SerializeField]
    private float maxDistance = 20;

    [SerializeField]
    private int collisionSamplesPerUnit = 3;

    private int collisionSamples;

    private const int vfxPositionsPerSample = 3;

    private const float baseSpeed = 50f;

    private VFXTextureFormatter trailPosTexture;

    [SerializeField]
    private VisualEffect trail;
    public GameObject Trail => trail.gameObject;

    private bool hitAssist = false;

    [SerializeField]
    private AugmentAnimator animator;

    private ProjectileState projectile = new ProjectileState();

    protected override void Awake()
    {
        base.Awake();
        if (!gunController || !gunController.Player)
            return;
        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;
        animator.OnShotFiredAnimation += FireProjectile;
        hitAssist = !gunController.Player.inputManager.IsMouseAndKeyboard;
    }

    private void Start()
    {
        collisionSamples = Mathf.CeilToInt(collisionSamplesPerUnit * maxDistance);
        var bulletsPerShot = Mathf.CeilToInt(stats.ProjectilesPerShot);
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

    protected override void OnReload(GunStats stats)
    {
        animator.OnReload(stats);
    }

    public override void InitializeProjectile(GunStats stats)
    {
        animator.OnFire(stats);
    }

    private void FireProjectile()
    {
        for (int k = 0; k < stats.ProjectilesPerShot; k++)
        {
            Quaternion randomSpread = Quaternion.Lerp(Quaternion.identity, Random.rotation, stats.ProjectileSpread);

            // TODO: Possibly standardize this better
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

            projectile.speed = baseSpeed * stats.ProjectileSpeedFactor;

            int sampleNum = 0;

            while (sampleNum < collisionSamples && projectile.active)
            {

                projectile.oldPosition = projectile.position;
                projectile.lastUpdateTime = Time.time;


                for (int j = 0; j < vfxPositionsPerSample; j++)
                {
                    TrySetTextureValue(sampleNum * vfxPositionsPerSample + j + k * vfxPositionsPerSample * collisionSamples, projectile.position);
                    UpdateProjectileMovement?.Invoke(maxDistance / (collisionSamples * vfxPositionsPerSample), ref projectile);
                }

                RaycastHit[] collisions = ProjectileMotions.GetPathCollisions(projectile, collisionLayers);

                if (collisions.Length > 0)
                {
                    sampleNum += 1;
                    var collider = collisions[0].collider;
                    HitboxController hitbox = collider.GetComponent<HitboxController>();
                    if (hitbox != null)
                        if (hitbox.health.Player == player && projectile.distanceTraveled < player.GunController.OutputTransitionDistance)
                            continue;

                    if (hitbox != null)
                        OnHitboxCollision?.Invoke(hitbox, ref projectile);

                    OnColliderHit?.Invoke(collider, ref projectile);
                    projectile.position = collisions[0].point;
                    projectile.active = false;
                    if (sampleNum < collisionSamples)
                        TrySetTextureValue(sampleNum * vfxPositionsPerSample + k * vfxPositionsPerSample * collisionSamples, projectile.position);
                }
                else
                {
                    sampleNum += 1;
                }
            }

            for (int i = sampleNum * vfxPositionsPerSample + 1; i < collisionSamples * vfxPositionsPerSample; i++)
            {
                TrySetTextureValue(i + k * vfxPositionsPerSample * collisionSamples, projectile.position);
            }

            trailPosTexture.ApplyChanges();
        }
        // Play the trail
        trail.SendEvent(VisualEffectAsset.PlayEventID);
    }

    private void TrySetTextureValue(int index, Vector3 position)
    {
        try
        {
            trailPosTexture.setValue(index, position);
        }
        catch (IndexOutOfRangeException _)
        {
            Debug.LogWarning($"Index {index} is out of bounds in BulletController texture (max {vfxPositionsPerSample * collisionSamples * Mathf.CeilToInt(stats.ProjectilesPerShot)})");
        }
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

