using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.VFX;
public class BulletController : ProjectileController
{

   
    [SerializeField]
    private float maxDistance = 50;

    [SerializeField]
    private int collisionSamples = 60;

    private int vfxPositionsPerSample = 3;

    private Texture2D trailPosTexture;
    private float[] trailPositions;

    [SerializeField]
    private VisualEffect trail;

    [SerializeField]
    private VisualEffect flash;
    public static void MoveWithGravity(float distance, ref ProjectileState state, GunStats stats)
    {
        //Update the position of the projectile
        state.position += state.direction * distance;

        //Update the velocity of the projectile
        float time = distance / state.speed;
        Vector3 velocity = state.direction * state.speed;
        velocity += Vector3.up * state.gravity * time;
        state.speed = velocity.magnitude;
        state.direction = velocity.normalized;
    }




    public void Awake()
    {
        UpdateProjectileMovement += MoveWithGravity;

        trailPositions = new float[vfxPositionsPerSample * collisionSamples * 3];
        trailPosTexture = new Texture2D(vfxPositionsPerSample * collisionSamples, 3 , TextureFormat.RFloat, false);
        trail.SetTexture("Position", this.trailPosTexture);
    }


    //public void InitializeNewProjectile(Transform output)
    //{
    //    int i = 0;
    //    while (i < maxProjectiles)
    //    {
    //        if (!states[currentProjectileIdx].active)
    //        {
    //
    //            InitalizeProjectileState(ref states[currentProjectileIdx], output);
    //            return;
    //        }
    //        i++;
    //    }
    //    Debug.LogError("Bullets exeeded max number");
    //    //(transform.position, transform.rotation, transform.forward, stats.ProjectileSpeed, stats.ProjectileGravityModifier);
    //}

    public override void InitializeProjectile(GunStats stats)
    {
        var projectile = new ProjectileState();
        projectile.active = true;
        projectile.position = projectileOutput.position;
        projectile.oldPosition = projectileOutput.position;
        projectile.direction = projectileOutput.forward;
        projectile.maxDistance = this.maxDistance;
        projectile.rotation = projectileOutput.rotation;
        projectile.initializationTime = Time.fixedTime;
        projectile.speed = stats.ProjectileSpeed;
        projectile.gravity = stats.ProjectileGravityModifier * 9.81f;

        onProjectileInit?.Invoke(ref projectile, stats);
       
        for (int i = 0; i < collisionSamples; i++)
        {
            projectile.oldPosition = projectile.position;

            projectile.lastUpdateTime = Time.time;

            for (int j = 0; j < vfxPositionsPerSample; j++)
            {
                UpdateProjectileMovement?.Invoke(maxDistance / (collisionSamples * vfxPositionsPerSample), ref projectile, stats);

                trailPositions[(i * vfxPositionsPerSample + j)] = projectile.position.x;
                trailPositions[(i * vfxPositionsPerSample + j) + (vfxPositionsPerSample * collisionSamples) * 1] = projectile.position.y;
                trailPositions[(i * vfxPositionsPerSample + j) + (vfxPositionsPerSample * collisionSamples) * 2] = projectile.position.z;
            }
        }


        // Set up the trail positions
        trailPosTexture.SetPixelData<float>(trailPositions, 0, 0);
        trailPosTexture.Apply();
        trail.SetTexture("Position", this.trailPosTexture);
        // Play the flash and trail
        trail.SendEvent("OnPlay");
        flash.SendEvent("OnPlay");



        Collider[] collisions;

        var direction = projectile.position - projectile.oldPosition;
        RaycastHit[] rayCasts;

        if (stats.ProjectileSize > 0)
        {
            rayCasts = Physics.SphereCastAll(transform.position, stats.ProjectileSize, direction, direction.magnitude);
        }
        else
        {
            rayCasts = Physics.RaycastAll(transform.position, direction, direction.magnitude);
        }

        // Ordered by distance along path hit, so that things are hit in the correct order

        collisions = rayCasts.OrderBy(x => x.distance).Select(x => x.collider).ToArray();
        foreach (var collision in collisions)
        {

            OnColliderHit?.Invoke(collision, ref projectile, stats);
            HitboxController hitbox = collision.GetComponent<HitboxController>();

            if (hitbox != null)
            {
                OnHitboxCollision?.Invoke(hitbox, ref projectile, stats);
            }
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

