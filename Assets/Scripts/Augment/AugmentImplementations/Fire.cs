using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class Fire : GunExtension
{
    [SerializeField]
    private GameObject fire;
    [SerializeField]
    private LayerMask trailLayers;
    private GunController gunController;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] lighterSounds;

    [SerializeField]
    private VisualEffect fireTrail;

    private int maxProjectiles = 1000;
    private ProjectileState[] projectiles;

    private ProjectileState loadedProjectile;

    //index of last initialized state in array
    private int currentStateIndex = 0;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Fire not attached to gun parent!");
            return;
        }
        gunController.onInitializeGun += AddFireToProjectile;
        gunController.onFireEnd += PlayShotAudio;
        gunController.projectile.OnProjectileInit += TrackProjectile;
        gunController.projectile.UpdateProjectileMovement += ApplyTrails;
        positionActiveTexture = new VFXTextureFormatter(maxProjectiles);
        fireTrail.SetInt("MaxParticleCount", maxProjectiles);
        fireTrail.SetTexture("Positions", positionActiveTexture.Texture);
        fireTrail.SendEvent(VisualEffectAsset.PlayEventID);
        projectiles = new ProjectileState[maxProjectiles];
        //InvokeRepeating("ApplyTrails", 0, 0.05f);
    }

    private void TrackProjectile(ref ProjectileState state, GunStats stats)
    {
        projectiles[currentStateIndex] = state;
        currentStateIndex++;
        currentStateIndex %= maxProjectiles;
        Debug.Log("Tracking projectile!");
    }

    private void ApplyTrails(float distance, ref ProjectileState state)
    {
        for (int i = 0; i < maxProjectiles; i++)
        {
            var projectile = projectiles[i];
            if (projectile == null || !projectile.active)
            {
                positionActiveTexture.setAlpha(i, 0f);
                continue;
            }
            
            Collider[] hitColliders = Physics.OverlapSphere(projectile.oldPosition, 0.5f, trailLayers);
            foreach (var hitCollider in hitColliders)
            {
                HitboxController hitbox = hitCollider.GetComponent<HitboxController>();

                if (hitbox != null)
                    Debug.Log("Hit!");
            }
            //fireTrail.SetVector3("Position", projectile.oldPosition);

            positionActiveTexture.setValue(i, projectile.oldPosition);
            positionActiveTexture.setAlpha(i, 1f);
        }
        positionActiveTexture.ApplyChanges();
        //fireTrail.SetInt("Amount", count);
        //fireTrail.SendEvent(VisualEffectAsset.PlayEventID);
    }

    private void AddFireToProjectile(GunStats gunstats)
    {
        GameObject fireObject = Instantiate(fire, gunController.projectile.transform);
        fireObject.SetActive(false);
    }

    private void PlayShotAudio(GunStats stats)
    {
        if (!gunController)
            return;
        audioSource.clip = lighterSounds.RandomElement();
        audioSource.Play();
    }
}
