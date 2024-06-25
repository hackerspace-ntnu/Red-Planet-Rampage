using System.Linq;
using UnityEngine;
using UnityEngine.VFX;
using Mirror;

public class LazurController : ProjectileController
{
    public VisualEffect Vfx;

    [SerializeField]
    private LessJallaVFXPositionEncoder encoder;

    [SerializeField]
    private float MaxDistance;

    [SerializeField]
    private int maxCollisions;

    private ProjectileState projectile = new ProjectileState();

    [SerializeField]
    private LazurFiringAnimator animator;

    private AudioSource audioSource;

    [SerializeField]
    private AudioGroup chargeUpAudio;

    private uint currentShotID;

    protected override void Awake()
    {
        base.Awake();
        if (!gunController || !gunController.Player)
            return;
        audioSource = GetComponent<AudioSource>();
        animator.OnShotFiredAnimation += FireLazur;
        animator.OnChargeStart += PlayChargeUpSound;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (!gunController || !gunController.Player || !animator)
            return;
        animator.OnShotFiredAnimation -= FireLazur;
        animator.OnChargeStart -= PlayChargeUpSound;
    }

    private void Start()
    {
        Vfx.SetGraphicsBuffer("StartEndPositions", encoder.StartEndPositionsBuffer);
    }

    private void FireLazur()
    {
        if (authority)
            CmdFireProjectile(projectileOutput.position, projectileRotation * projectileOutput.forward);
    }

    [Command]
    private void CmdFireProjectile(Vector3 output, Vector3 direction)
    {
        // TODO verify that this input is reasonable!
        RpcFireProjectile(output, direction);
    }

    [ClientRpc]
    private void RpcFireProjectile(Vector3 output, Vector3 direction)
    {
        projectile = new()
        {
            shotID = currentShotID,
            active = true,
            distanceTraveled = 0f,
            damage = stats.ProjectileDamage,
            position = output,
            oldPosition = output,
            direction = direction,
            maxDistance = MaxDistance,
            initializationTime = Time.fixedTime,
            size = stats.ProjectileSize
        };
        projectile.additionalProperties.Clear();

        OnProjectileInit?.Invoke(ref projectile, stats);

        int currentLasers = 0;
        Collider lastCollider = null;
        while (projectile.active)
        {
            // Allow hitting the same player multiple times
            projectile.hitHealthControllers.Clear();

            projectile.oldPosition = projectile.position;

            RaycastHit[] rayCasts = Physics.SphereCastAll(projectile.position, projectile.size, projectile.direction, MaxDistance - projectile.distanceTraveled, collisionLayers);
            rayCasts = rayCasts.OrderBy(x => x.distance).Where(p => p.collider != lastCollider).ToArray();


            if (rayCasts.Length > 0)
            {

                Collider collider = rayCasts[0].collider;
                lastCollider = collider;
                HitboxController hitbox = collider.GetComponent<HitboxController>();

                projectile.position = rayCasts[0].point;
                OnRicochet?.Invoke(rayCasts[0], ref projectile);
                if (hitbox != null)
                    OnHitboxCollision?.Invoke(hitbox, ref projectile);
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

            if (currentLasers >= maxCollisions)
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
    public override void InitializeProjectile(GunStats stats, uint shotID)
    {
        currentShotID = shotID;
        animator.OnFire(stats);
    }

    private void PlayChargeUpSound()
    {
        if (!gunController)
            return;
        chargeUpAudio.Play(audioSource);
    }
}
