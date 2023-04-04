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
    private int collisionSamples = 30;

    private int vfxPositionsPerSample = 3;

    private float bulletSpeed = 50f;

    private VFXTextureFormatter trailPosTexture;
    //private Texture2D trailPosTexture;
    //private float[] trailPositions;

    [SerializeField]
    private VisualEffect trail;

    [SerializeField]
    private VisualEffect flash;

    private ProjectileState projectile = new ProjectileState();
    

    public void Awake()
    {
        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;
        //trailPositions = new float[vfxPositionsPerSample * collisionSamples * 3];
        trailPosTexture = new VFXTextureFormatter(vfxPositionsPerSample * collisionSamples);
        trail.SetTexture("Position", this.trailPosTexture.Texture);
    }
    private void Start()
    {
        flash.transform.position = projectileOutput.position;
    }
    public override void InitializeProjectile(GunStats stats)
    {   

        // TODO: Possibly standardize this better

        projectile.active = true;
        projectile.distanceTraveled = 0f;
        projectile.damage = stats.ProjectileDamage;
        projectile.position = projectileOutput.position;
        projectile.oldPosition = projectileOutput.position;
        projectile.direction = projectileOutput.forward;
        projectile.maxDistance = this.maxDistance;
        projectile.rotation = projectileOutput.rotation;
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


            for(int j = 0; j < vfxPositionsPerSample; j++)
            {
                trailPosTexture.setValue(sampleNum * vfxPositionsPerSample + j, projectile.position);
                UpdateProjectileMovement?.Invoke(maxDistance / (collisionSamples * vfxPositionsPerSample), ref projectile);
            }

            Collider[] collisions = ProjectileMotions.GetPathCollisions(projectile, collisionLayers);

            if(collisions.Length > 0)
            {
                OnColliderHit?.Invoke(collisions[0], ref projectile);
                HitboxController hitbox = collisions[0].GetComponent<HitboxController>();

                if (hitbox != null)
                {
                    OnHitboxCollision?.Invoke(hitbox, ref projectile);
                }
                projectile.active = false;
                sampleNum += 1;
                trailPosTexture.setValue(sampleNum * vfxPositionsPerSample, projectile.position);
  
            }
            else
            {
                sampleNum += 1;
            }
        }
  
        for (int i = sampleNum * vfxPositionsPerSample + 1; i < collisionSamples * vfxPositionsPerSample; i++)
        {
            trailPosTexture.setValue(i, projectile.position);
        }

        trailPosTexture.ApplyChanges();
        
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

