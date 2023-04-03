using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Class that ties the functions and properties of the completed gun to the animations of the hat barrel
/// </summary>
public class HatBarrel : ProjectileController
{
    private GunController gunController;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private HatBarrelModel hatBarrelModel;

    [SerializeField]
    private int maxHatProjectiles = 300;

    [SerializeField]
    private float hatMaxDistance = 20f;

    [SerializeField]
    private float hatSpeed = 10f;

    private ProjectileState[] projectiles;

    private ProjectileState loadedProjectile;

    [SerializeField]
    private LayerMask collisionLayers;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;

    [SerializeField]
    private VisualEffect hatVfx;
    void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController) 
        {
            Debug.Log("HatBarrel not attached to gun parent!");
            return;
        }
        gunController.onInitializeGun += OnInitialize;
        //gunController.onFire += OnFire;
        gunController.onReload += OnReload;
        projectiles = new ProjectileState[maxHatProjectiles];

        UpdateProjectileMovement += ProjectileMotions.MoveWithGravity;

        positionActiveTexture = new VFXTextureFormatter(maxHatProjectiles);

        hatVfx.SetTexture("Positions", positionActiveTexture.Texture);
    }

    private void OnInitialize(GunStats gunstats)
    {
        animator.speed = Mathf.Max(gunstats.Firerate, 1f);
        hatBarrelModel.OnInitialize(gunstats.magazineSize);
    }

    private void OnReload(GunStats gunstats)
    {
        hatBarrelModel.OnReload(gunstats.Ammo);
    }

    private void OnDestroy()
    {
        gunController.onInitializeGun -= OnInitialize;
        gunController.onReload -= OnReload;
    }
    public override void InitializeProjectile(GunStats stats)
    {
        loadedProjectile = new ProjectileState(stats, projectileOutput);
        loadedProjectile.maxDistance = this.hatMaxDistance;

        animator.SetTrigger("Fire");
        hatBarrelModel.OnFire(stats.Ammo);
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
                loadedProjectile.direction = projectileOutput.forward;
                loadedProjectile.rotation = projectileOutput.rotation;

                projectiles[currentStateIndex] = loadedProjectile;
                // Sets initial position of the projectile
                positionActiveTexture.setValue(i, loadedProjectile.position);
                positionActiveTexture.setAlpha(i,  1f);

                // Neccessary to update the actual texture, so the vfx gets the new info
                positionActiveTexture.ApplyChanges();

                currentStateIndex = (currentStateIndex + 1) % maxHatProjectiles;
                hatVfx.SendEvent("OnPlay");

                loadedProjectile = null;

                return;
            }
            currentStateIndex = (currentStateIndex + 1) % maxHatProjectiles;
        }
    }
    private void FixedUpdate()
    {
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
        print(state.speedFactor);
        UpdateProjectileMovement?.Invoke(state.speed * state.speedFactor * Time.fixedDeltaTime, ref state);
        OnProjectileTravel?.Invoke(ref state);


        if (state.distanceTraveled > state.maxDistance)
        {
            state.active = false;
        }

        var collisions = ProjectileMotions.GetPathCollisions(state, collisionLayers);

        if(collisions.Length > 0)
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
