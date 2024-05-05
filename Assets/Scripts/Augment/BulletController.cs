using System;
using System.Linq;
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

    [SerializeField]
    private int collisionsBeforeInactive = 1;

    private const int vfxPositionsPerSample = 3;

    private const float baseSpeed = 50f;

    [SerializeField]
    private VFXTextureFormatter trailPositionBuffer;

    [SerializeField]
    private VisualEffect trail;
    public GameObject Trail => trail.gameObject;

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
    }

    public void SetTrail(VisualEffect newTrail)
    {
        if (trailPositionBuffer.Buffer != null)
            trailPositionBuffer.Buffer.Release();
        trail = newTrail;
    }

    private void Start()
    {
        collisionSamples = Mathf.CeilToInt(collisionSamplesPerUnit * maxDistance);
        var bulletsPerShot = Mathf.CeilToInt(stats.ProjectilesPerShot);
        trailPositionBuffer.Initialize(vfxPositionsPerSample * collisionSamples * bulletsPerShot);
        trail.SetGraphicsBuffer("Position", trailPositionBuffer.Buffer);
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

            projectile = new()
            {
                // TODO: Possibly standardize this better
                active = true,
                distanceTraveled = 0f,
                damage = stats.ProjectileDamage,
                position = projectileOutput.position,
                oldPosition = projectileOutput.position,
                direction = randomSpread * projectileRotation * projectileOutput.forward,
                maxDistance = maxDistance,
                rotation = randomSpread * projectileRotation * projectileOutput.rotation,
                initializationTime = Time.fixedTime,
                speedFactor = stats.ProjectileSpeedFactor,
                gravity = stats.ProjectileGravityModifier * 9.81f
            };
            projectile.additionalProperties.Clear();
            projectile.hitHealthControllers.Clear();

            OnProjectileInit?.Invoke(ref projectile, stats);

            projectile.speed = baseSpeed * stats.ProjectileSpeedFactor;

            int sampleNum = 0;
            int totalCollisions = 0;
            Collider lastCollider = null;

            while (sampleNum < collisionSamples && projectile.active)
            {

                projectile.oldPosition = projectile.position;
                projectile.lastUpdateTime = Time.time;


                for (int j = 0; j < vfxPositionsPerSample; j++)
                {
                    TrySetTextureValue(sampleNum * vfxPositionsPerSample + j + k * vfxPositionsPerSample * collisionSamples, projectile.position);
                    UpdateProjectileMovement?.Invoke(maxDistance / (collisionSamples * vfxPositionsPerSample), ref projectile);
                }

                RaycastHit[] collisions = ProjectileMotions.GetPathCollisions(projectile, collisionLayers).Where(p => p.collider != lastCollider).ToArray();
                sampleNum += 1;
                for (int i = 0; i < collisions.Length && projectile.active; i++)
                {
                    totalCollisions += 1;
                    var collider = collisions[i].collider;
                    HitboxController hitbox = collider.GetComponent<HitboxController>();
                    if (hitbox != null)
                        if (hitbox.health.Player == player && projectile.distanceTraveled < player.GunController.OutputTransitionDistance)
                            continue;

                    projectile.position = collisions[i].point;
                    if (hitbox != null)
                        OnHitboxCollision?.Invoke(hitbox, ref projectile);

                    OnColliderHit?.Invoke(collisions[i], ref projectile);

                    if (totalCollisions == this.collisionsBeforeInactive)
                        projectile.active = false;

                    if (sampleNum < collisionSamples)
                        TrySetTextureValue(sampleNum * vfxPositionsPerSample + k * vfxPositionsPerSample * collisionSamples, projectile.position);
                }
                if (collisions.Length > 0)
                    lastCollider = collisions[collisions.Length - 1].collider;

            }

            for (int i = sampleNum * vfxPositionsPerSample + 1; i < collisionSamples * vfxPositionsPerSample; i++)
            {
                TrySetTextureValue(i + k * vfxPositionsPerSample * collisionSamples, projectile.position);
            }

            trailPositionBuffer.ApplyChanges();
        }
        // Play the trail
        trail.SendEvent(VisualEffectAsset.PlayEventID);
    }

    private void TrySetTextureValue(int index, Vector3 position)
    {
        try
        {
            trailPositionBuffer.setValue(index, position);
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

