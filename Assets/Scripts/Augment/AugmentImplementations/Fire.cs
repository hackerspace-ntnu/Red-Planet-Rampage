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
    private GameObject stuckFirePrefab;
    [SerializeField]
    private LayerMask trailLayers;
    private GunController gunController;
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] lighterSounds;

    [SerializeField]
    private VisualEffect fireTrail;

    private int maxProjectiles = 1000;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;

    private HashSet<ProjectileState> trackedProjectiles = new HashSet<ProjectileState>();

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
    }

    private void TrackProjectile(ref ProjectileState state, GunStats stats)
    {
        trackedProjectiles.Add(state);
    }

    private void ApplyTrails(float distance, ref ProjectileState state)
    {
        var count = 0;
        trackedProjectiles.RemoveWhere(projectile => projectile.active == false);
        foreach (var projectile in trackedProjectiles)
        {
            // Check a certain length for hitboxes along the traveled path of the projectile
            if (projectile.distanceTraveled > 4f)
            {
                CheckTrailPathHits(projectile.oldPosition - projectile.direction);
                CheckTrailPathHits(projectile.oldPosition - projectile.direction * 2f);
                CheckTrailPathHits(projectile.oldPosition - projectile.direction * 3f);
            }

            positionActiveTexture.setValue(count, projectile.oldPosition);
            positionActiveTexture.setAlpha(count, 1f);
            count++;
        }
        fireTrail.SetFloat("Amount", count);
        positionActiveTexture.ApplyChanges();
        fireTrail.SendEvent(VisualEffectAsset.PlayEventID);
    }

    private void CheckTrailPathHits(Vector3 position)
    {
        var hitColliders = Physics.OverlapSphere(position, 0.5f, trailLayers);
        foreach (var hitCollider in hitColliders)
        {
            HitboxController hitbox = hitCollider.GetComponent<HitboxController>();
            if (hitbox != null && !hitbox.health.IsBurning)
            {
                // TODO: Handle this better
                Debug.Log("HitDetected!");
                hitbox.health.IsBurning = true;
                GameObject flame = Instantiate(stuckFirePrefab, hitbox.transform);
                StartCoroutine(WaitAndStopBurning(flame, hitbox.health));
            }
        }
    }

    private IEnumerator WaitAndStopBurning(GameObject flame, HealthController health)
    {
        yield return new WaitForSeconds(3f);
        Destroy(flame);
        health.IsBurning = false;
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var projectile in trackedProjectiles)
        {
            Gizmos.DrawSphere(projectile.oldPosition - projectile.direction, 0.5f);
            Gizmos.DrawSphere(projectile.oldPosition - projectile.direction * 2, 0.5f);
            Gizmos.DrawSphere(projectile.oldPosition - projectile.direction * 3, 0.5f);
        }       
    }
#endif
}
