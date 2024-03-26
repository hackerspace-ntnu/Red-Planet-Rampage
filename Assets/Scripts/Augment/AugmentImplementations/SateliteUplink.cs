using CollectionExtensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SateliteUplink : MonoBehaviour, ProjectileModifier
{
    [SerializeField]
    private FallingHazard[] spaceGarbage;

    [SerializeField]
    private GameObject targetingReticle;

    [SerializeField]
    private TMP_Text timerText;

    [SerializeField]
    private TMP_Text readyText;

    [SerializeField]
    private float launchHeight = 100;

    [SerializeField]
    private float cooldown = 10;

    [SerializeField]
    private float maxLaunchesPerShot = 20;

    private float launchesThisShot = 0;

    private GunController gunController;
    private Timer timer;

    private HashSet<ProjectileState> trackedProjectiles = new HashSet<ProjectileState>();

    private bool isTrackingCurrentShot = false;
    private bool isReady = false;

    private void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;

        gunController.onFireStart += StartTracking;
        gunController.onFireEnd += StopTracking;

        timer = GetComponent<Timer>();
        timer.OnTimerRunCompleted += OnCooldownEnd;
        timer.StartTimer(cooldown);
    }

    private void RestartCooldown()
    {
        isReady = false;
        timer.StartTimer(cooldown);
        timerText.gameObject.SetActive(true);
        readyText.gameObject.SetActive(false);
    }

    private void OnCooldownEnd()
    {
        isReady = true;
        timerText.gameObject.SetActive(false);
        readyText.gameObject.SetActive(true);
    }

    private void StartTracking(GunStats stats)
    {
        if (!isReady)
            return;
        isTrackingCurrentShot = true;
        launchesThisShot = 0;
        RestartCooldown();
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
        StartCoroutine(UnTrack(state));
    }

    private IEnumerator UnTrack(ProjectileState state)
    {
        yield return new WaitForSeconds(10);
        trackedProjectiles.Remove(state);

    }

    private void Target(RaycastHit hit, ref ProjectileState state)
    {
        if (!trackedProjectiles.Contains(state))
            return;
        if (hit.point.sqrMagnitude < .00001f)
            // Avoid issue with spherecasts not returning proper points sometimes
            Launch(hit.collider.transform.position);
        else
            Launch(hit.point);
    }

    private void Launch(Vector3 target)
    {
        if (!gunController)
            return;

        if (launchesThisShot >= maxLaunchesPerShot)
            return;
        launchesThisShot += 1;

        var offset = Random.Range(30f, 0);
        var launchPoint = target + (launchHeight + offset) * Vector3.up;

        var garbage = spaceGarbage.RandomElement();
        var garbageInstance = Instantiate(garbage, launchPoint, Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up));
        garbageInstance.Player = gunController.Player;

        var targetInstance = Instantiate(targetingReticle, target, Quaternion.identity);
        Destroy(targetInstance, 2); // TODO destroy only when we hit the ground (?)
    }

    private void OnDestroy()
    {
        if (!gunController)
            return;
        gunController.onFireStart -= StartTracking;
        gunController.onFireEnd -= StopTracking;
    }
}
