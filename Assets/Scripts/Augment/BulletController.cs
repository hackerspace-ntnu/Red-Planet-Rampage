using System;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using Random = System.Random;
using RandomExtensions;
using Mirror;

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

    private uint currentShotID;

    private Random random = new();

    protected override void Awake()
    {
        base.Awake();
        if (!gunController || !gunController.Player)
            return;
        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;
        animator.OnShotFiredAnimation += FireProjectile;
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
        trail.SetFloat("Size", stats.ProjectileScale);

        if (isServer)
            RpcSeedRandom(random.Next());
    }


    [ClientRpc]
    private void RpcSeedRandom(int seed)
    {
        random = new Random(seed);
    }


    public void SetTrail(VisualEffect newTrail)
    {
        trailPositionBuffer.Buffer?.Release();
        trail = newTrail;
        if (trail && stats)
            trail.SetFloat("Size", stats.ProjectileScale);
    }

    protected override void OnInitialize(GunStats gunstats)
    {
        trail.SetFloat("Size", stats.ProjectileScale);
        animator.OnInitialize(gunstats);
    }

    protected override void OnReload(GunStats stats)
    {
        animator.OnReload(stats);
    }

    public override void InitializeProjectile(GunStats stats, uint shotID)
    {
        currentShotID = shotID;
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
        for (int k = 0; k < stats.ProjectilesPerShot; k++)
        {
            Quaternion randomSpread = Quaternion.Lerp(Quaternion.identity, random.Rotation(), stats.ProjectileSpread);

            projectile = new(stats)
            {
                // TODO: Possibly standardize this better
                shotID = currentShotID,
                active = true,
                distanceTraveled = 0f,
                position = output,
                oldPosition = output,
                direction = randomSpread * direction,
                maxDistance = maxDistance,
                rotation = randomSpread * rotation,
                speedFactor = stats.ProjectileSpeedFactor,
                gravity = stats.ProjectileGravityModifier * 9.81f,
                size = Mathf.Max(0, (stats.ProjectileScale - 1) * .1f),
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
                        if (hitbox.health.Player == player && projectile.distanceTraveled < GunController.InvulnerabilityDistance)
                            continue;

                    projectile.position += projectile.direction * collisions[i].distance;
                    if (hitbox != null)
                        OnHitboxCollision?.Invoke(hitbox, ref projectile);

                    OnColliderHit?.Invoke(collisions[i], ref projectile);

                    if (totalCollisions == collisionsBeforeInactive)
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

