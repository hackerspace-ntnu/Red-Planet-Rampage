using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.VFX;
public class BulletController : ProjectileController
{
    [SerializeField]
    private LayerMask collisionLayers;

    [SerializeField]
    private float maxDistance = 20;

    [SerializeField]
    private int collisionSamples = 60;

    private int vfxPositionsPerSample = 3;

    private Texture2D trailPosTexture;
    private float[] trailPositions;

    [SerializeField]
    private VisualEffect trail;

    [SerializeField]
    private VisualEffect flash;

    private ProjectileState projectile = new ProjectileState();
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
        state.distanceTraveled += distance;
    }

    public void Awake()
    {
        UpdateProjectileMovement += MoveWithGravity;
        trailPositions = new float[vfxPositionsPerSample * collisionSamples * 3];
        trailPosTexture = new Texture2D(vfxPositionsPerSample * collisionSamples, 3 , TextureFormat.RFloat, false);
        trail.SetTexture("Position", this.trailPosTexture);
    }




    public override void InitializeProjectile(GunStats stats)
    {   
        projectile.active = true;
        projectile.distanceTraveled = 0f;
        projectile.position = projectileOutput.position;
        projectile.oldPosition = projectileOutput.position;
        projectile.direction = projectileOutput.forward;
        projectile.maxDistance = this.maxDistance;
        projectile.rotation = projectileOutput.rotation;
        projectile.initializationTime = Time.fixedTime;
        projectile.speed = stats.ProjectileSpeed;
        projectile.gravity = stats.ProjectileGravityModifier * 9.81f;
        projectile.additionalProperties.Clear();
        projectile.hitHealthControllers.Clear();
        OnProjectileInit?.Invoke(ref projectile, stats);

        int sampleNum = 0;

        while (sampleNum < collisionSamples && projectile.active)
        {
            
            projectile.oldPosition = projectile.position;

            projectile.lastUpdateTime = Time.time;

            for(int j = 0; j < vfxPositionsPerSample; j++)
            {
                trailPositions[(sampleNum * vfxPositionsPerSample + j)] = projectile.position.x;
                trailPositions[(sampleNum * vfxPositionsPerSample + j) + (vfxPositionsPerSample * collisionSamples) * 1] = projectile.position.y;
                trailPositions[(sampleNum * vfxPositionsPerSample + j) + (vfxPositionsPerSample * collisionSamples) * 2] = projectile.position.z;

                UpdateProjectileMovement?.Invoke(maxDistance / (collisionSamples * vfxPositionsPerSample), ref projectile, stats);
            }
            
            Collider[] collisions;

            var direction = projectile.position - projectile.oldPosition;
            RaycastHit[] rayCasts;

            if (stats.ProjectileSize > 0)
            {
                rayCasts = Physics.SphereCastAll(projectile.oldPosition, stats.ProjectileSize, direction, direction.magnitude, collisionLayers);
            }
            else
            {
                rayCasts = Physics.RaycastAll(projectile.oldPosition, direction, direction.magnitude, collisionLayers);
            }

            // Ordered by distance along path hit, so that things are hit in the correct order

            collisions = rayCasts.OrderBy(x => x.distance).Select(x => x.collider).ToArray();
            if(collisions.Length > 0)
            {
                Debug.Log("Collided");
                OnColliderHit?.Invoke(collisions[0], ref projectile, stats);
                HitboxController hitbox = collisions[0].GetComponent<HitboxController>();

                if (hitbox != null)
                {
                    OnHitboxCollision?.Invoke(hitbox, ref projectile, stats);
                }
                projectile.active = false;
                sampleNum += 1;

                trailPositions[(sampleNum * vfxPositionsPerSample)] = projectile.position.x;
                trailPositions[(sampleNum * vfxPositionsPerSample) + (vfxPositionsPerSample * collisionSamples) * 1] = projectile.position.y;
                trailPositions[(sampleNum * vfxPositionsPerSample) + (vfxPositionsPerSample * collisionSamples) * 2] = projectile.position.z;
            }
            else
            {
                sampleNum += 1;
            }
        }

        

        for (int i = sampleNum * vfxPositionsPerSample + 1; i < collisionSamples * vfxPositionsPerSample; i++)
        {
            trailPositions[i] = trailPositions[i - 1];
            trailPositions[i + (vfxPositionsPerSample * collisionSamples) * 1] = trailPositions[i - 1 + (vfxPositionsPerSample * collisionSamples) * 1];
            trailPositions[i + (vfxPositionsPerSample * collisionSamples) * 2] = trailPositions[i - 1 + (vfxPositionsPerSample * collisionSamples) * 2];
        }

        // Set up the trail positions
        trailPosTexture.SetPixelData<float>(trailPositions, 0, 0);
        trailPosTexture.Apply();
        trail.SetTexture("Position", this.trailPosTexture);
        // Play the flash and trail
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

