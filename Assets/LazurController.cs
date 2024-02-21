using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
public class LazurController : ProjectileController
{
    [SerializeField]
    private VisualEffect Vfx;

    [SerializeField]
    private LessJallaVFXPositionEncoder encoder;

    [SerializeField]
    private float MaxDistance;

    [SerializeField]
    private int maxCollisions;

    private ProjectileState projectile = new ProjectileState();

    [SerializeField]
    private AugmentAnimator animator;

    protected override void Awake()
    {
        base.Awake();
        if (!gunController || !gunController.Player)
            return;
        animator.OnShotFiredAnimation += FireLazur;
    }

    void Start()
    {
        Vfx.SetGraphicsBuffer("StartEndPositions", encoder.StartEndPositionsBuffer);   
    }

    public void FireLazur()
    {
        projectile.active = true;
        projectile.distanceTraveled = 0f;
        projectile.damage = stats.ProjectileDamage;
        projectile.position = projectileOutput.position;
        projectile.oldPosition = projectileOutput.position;
        projectile.direction =  projectileOutput.forward;
        projectile.maxDistance = this.MaxDistance;
        projectile.initializationTime = Time.fixedTime;
        projectile.additionalProperties.Clear();
        projectile.hitHealthControllers.Clear();

        OnProjectileInit?.Invoke(ref projectile, stats);

        int currentLasers = 0;
        Collider lastCollider = null;
        while(projectile.active)
        {
            projectile.oldPosition = projectile.position;

            RaycastHit[] rayCasts = Physics.RaycastAll(projectile.position, projectile.direction, MaxDistance-projectile.distanceTraveled, collisionLayers);
            rayCasts = rayCasts.OrderBy(x => x.distance).Where(p => p.collider != lastCollider).ToArray();


            if (rayCasts.Length > 0)
            {
            
                Collider collider = rayCasts[0].collider;
                lastCollider = collider;
                HitboxController hitbox = collider.GetComponent<HitboxController>();
                
                projectile.position = rayCasts[0].point;
                if (hitbox != null)
                    OnHitboxCollision?.Invoke(hitbox, ref projectile);
                print(rayCasts[0].collider.gameObject.name);
                OnColliderHit?.Invoke(rayCasts[0], ref projectile);

                projectile.distanceTraveled += rayCasts[0].distance;
                
            }
            else
            {
                projectile.position = projectile.position + projectile.direction.normalized * (MaxDistance - projectile.distanceTraveled);
                projectile.active = false;
            }
            encoder.AddLine(projectile.oldPosition, projectile.position);
            currentLasers++;

            if(currentLasers >= maxCollisions)
            {
                projectile.active = false;
            }
        }
        encoder.PopulateBuffer();
        Vfx.SetInt("SpawnCount", currentLasers);
        Vfx.SendEvent("OnPlay");
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
}
