using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public enum ProjectileType
{
    Hitscan,
    Mesh,
    Laser,
}

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
    private ProjectileType projectileType = ProjectileType.Hitscan;
    [Header("HitscanProjectiles")]
    [SerializeField]
    private VisualEffect fireTrail;
    [Header("MeshProjectiles")]
    [SerializeField]
    private VisualEffect fireTrailInstances;
    private int maxProjectiles = 1000;

    // texture used to update the vfx position and alive-state of particles, RGB is used for position A for alive/dead
    private VFXTextureFormatter positionActiveTexture;

    private HashSet<ProjectileState> trackedProjectiles = new HashSet<ProjectileState>();
    // Used to keep track of the healthControllers currently burning
    public HashSet<HealthController> hitHealthControllers = new HashSet<HealthController>();

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;

        gunController.onInitializeGun += AddFireToProjectile;
        gunController.onFireEnd += PlayShotAudio;
        gunController.projectile.OnProjectileInit += TrackProjectile;
        gunController.projectile.UpdateProjectileMovement += ApplyTrails;

        if (gunController.projectile is MeshProjectileController)
        {
            positionActiveTexture = new VFXTextureFormatter(maxProjectiles);
            fireTrailInstances.SetInt("MaxParticleCount", maxProjectiles);
            fireTrailInstances.SetTexture("Positions", positionActiveTexture.Texture);
            fireTrailInstances.SendEvent(VisualEffectAsset.PlayEventID);
            projectileType = ProjectileType.Mesh;
        }
        else if(gunController.projectile is BulletController)
        {
            ((BulletController)gunController.projectile).SetTrail(fireTrail);
            projectileType = ProjectileType.Hitscan;
        }
        else if (gunController.projectile is LazurController)
        {
            projectileType = ProjectileType.Laser;
        }
            
    }

    private void TrackProjectile(ref ProjectileState state, GunStats stats)
    {
        trackedProjectiles.Add(state);
        if (projectileType == ProjectileType.Hitscan)
            StartCoroutine(WaitAndStopTracking(state));
    }

    private void ApplyTrails(float distance, ref ProjectileState state)
    {
        switch (projectileType)
        {
            case ProjectileType.Mesh:
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
                fireTrailInstances.SetFloat("Amount", count);
                positionActiveTexture.ApplyChanges();
                fireTrailInstances.SendEvent(VisualEffectAsset.PlayEventID);
                break;
            case ProjectileType.Hitscan:
            case ProjectileType.Laser:
                break;
        }
    }

    private void FixedUpdate()
    {
        if (projectileType != ProjectileType.Hitscan)
            return;

        foreach (var state in trackedProjectiles)
        {
            var halfDistance = state.distanceTraveled / 2;
            Debug.DrawRay(state.position - state.direction * halfDistance, state.direction* halfDistance, Color.blue);
            var extraArea = gunController.stats.ProjectileSize.Value() + gunController.stats.ProjectileSpread.Value(); 
            var hitColliders = Physics.OverlapBox(state.position - state.direction * halfDistance * 0.9f, new Vector3(0.4f + extraArea * 0.5f, 0.4f + extraArea * 0.5f, halfDistance), state.rotation, trailLayers);
            foreach (var hitCollider in hitColliders)
            {
                HitboxController hitbox = hitCollider.GetComponent<HitboxController>();
                if (hitbox != null)
                    if (hitbox.health && !hitHealthControllers.Contains(hitbox.health))
                    {
                        hitHealthControllers.Add(hitbox.health);
                        GameObject flame = Instantiate(stuckFirePrefab, hitbox.transform);
                        if (flame.TryGetComponent<ContinuousDamage>(out var damage))
                            damage.source = gunController.Player;
                        StartCoroutine(WaitAndStopBurning(flame, hitbox.health));
                    }
            }
        }
    }

    private void CheckTrailPathHits(Vector3 position)
    {
        var hitColliders = Physics.OverlapSphere(position, 0.5f, trailLayers);
        foreach (var hitCollider in hitColliders)
        {
            HitboxController hitbox = hitCollider.GetComponent<HitboxController>();
            if (hitbox != null)
                if (hitbox.health && !hitHealthControllers.Contains(hitbox.health))
                {
                    hitHealthControllers.Add(hitbox.health);
                    GameObject flame = Instantiate(stuckFirePrefab, hitbox.transform);
                    if (flame.TryGetComponent<ContinuousDamage>(out var damage))
                        damage.source = gunController.Player;
                    StartCoroutine(WaitAndStopBurning(flame, hitbox.health));
                }
        }
    }

    private IEnumerator WaitAndStopTracking(ProjectileState state)
    {
        yield return new WaitForSeconds(2f);
        trackedProjectiles.Remove(state);
    }

    private IEnumerator WaitAndStopBurning(GameObject flame, HealthController health)
    {
        yield return new WaitForSeconds(3f);
        Destroy(flame);
        hitHealthControllers.Remove(health);
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
        switch (projectileType)
        {
            case ProjectileType.Mesh:
                foreach (var projectile in trackedProjectiles)
                {
                    Gizmos.DrawSphere(projectile.oldPosition - projectile.direction, 0.5f);
                    Gizmos.DrawSphere(projectile.oldPosition - projectile.direction * 2, 0.5f);
                    Gizmos.DrawSphere(projectile.oldPosition - projectile.direction * 3, 0.5f);
                }
                break;
            case ProjectileType.Hitscan:
                foreach (var projectile in trackedProjectiles)
                {
                    var extraArea = gunController.stats.ProjectileSize.Value() + gunController.stats.ProjectileSpread.Value();
                    Gizmos.matrix = Matrix4x4.TRS(projectile.position - projectile.direction * (projectile.distanceTraveled / 2) * 0.9f, projectile.rotation, new Vector3(0.4f + extraArea * 0.5f, 0.4f + extraArea * 0.5f, projectile.distanceTraveled / 2)); ;
                    Gizmos.DrawCube(Vector3.zero, Vector3.one);
                }
                break;
        }
    }
#endif
}
