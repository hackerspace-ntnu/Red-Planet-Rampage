using CollectionExtensions;
using System.Collections.Generic;
using UnityEngine;

public class SateliteUplink : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private FallingHazard[] spaceGarbage;

    [SerializeField]
    private GameObject targetingReticle;

    [SerializeField]
    private float launchHeight = 100;

    private GunController gunController;
    private Transform targetingReticleInstance;

    private Vector3 target = Vector3.zero;

    private bool isTrackingCurrentShot = true;
    private const float trackingTimeout = .1f;
    private HashSet<ProjectileState> trackedProjectiles = new HashSet<ProjectileState>();

    private void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;

        gunController.onFireStart += StartTracking;
        gunController.onFireEnd += StopTracking;
        //targetingReticleInstance = Instantiate(targetingReticle, Vector3.zero, Quaternion.identity).transform;
    }

    private void StartTracking(GunStats stats)
    {
        isTrackingCurrentShot = true;
    }

    private void StopTracking(GunStats stats)
    {
        isTrackingCurrentShot = false;
    }

    public void Attach(ProjectileController projectile)
    {
        projectile.OnProjectileInit += Track;
        projectile.OnColliderHit += Target;
        projectile.OnRicochet += Target;
    }

    public void Detach(ProjectileController projectile)
    {
        projectile.OnProjectileInit -= Track;
        projectile.OnColliderHit -= Target;
        projectile.OnRicochet += Target;
    }

    private void Track(ref ProjectileState state, GunStats stats)
    {
        if (!isTrackingCurrentShot)
            return;
        trackedProjectiles.Add(state);
        // TODO stop tracking each state after a few seconds, so the hashset doesn't grow huge
    }

    private void Target(RaycastHit hit, ref ProjectileState state)
    {
        if (!trackedProjectiles.Contains(state))
            return;
        Launch(hit.point);
    }

    private void Launch(Vector3 target)
    {
        // TODO have a max cap for the amount that can be spawned
        if (!gunController)
            return;
        // TODO don't launch until cooldown is over
        var offset = Random.Range(30f, 0);
        var launchPoint = target + (launchHeight + offset) * Vector3.up;
        var launchee = spaceGarbage.RandomElement();
        var instance = Instantiate(launchee, launchPoint, Quaternion.identity);
        var targetInstance = Instantiate(targetingReticle, target, Quaternion.identity);
        Destroy(targetInstance, 4); // TODO destroy only when we hit the ground
        // TODO set targeting reticle solid and set its garbage instance
    }

    private void OnDestroy()
    {
        //if (!targetingReticleInstance)
        //return;
        //Destroy(targetingReticleInstance);
        if (!gunController)
            return;
        gunController.onFireStart -= StartTracking;
        gunController.onFireEnd -= StopTracking;
    }
}
