using UnityEngine;
using UnityEngine.VFX;
using static GunStats;

[RequireComponent(typeof(ProjectileController))]
public class GunBarrel : Augment
{
    [SerializeField]
    private CrossHairModes crossHairMode;

    [Tooltip("Where to attach extensions")]
    public Transform[] attachmentPoints;

    public ProjectileController Projectile { get => GetComponent<ProjectileController>(); }

    [SerializeField]
    private VisualEffect muzzleFlash;
    public VisualEffect MuzzleFlash => muzzleFlash;

    protected GunController gunController;

    private void Awake()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (gunController)
            gunController.stats.CrossHairMode = crossHairMode;
    }

    private void Start()
    {
        if (!gunController)
            return;

        if (muzzleFlash)
        {
            muzzleFlash.transform.position = Projectile.projectileOutput.position;
            gunController.onFire += PlayMuzzleFlash;
        }
    }

    public override void Attach(GunController gunController) { }

    public override void Detach(GunController gunController) { }

    private void OnDestroy()
    {
        if (!gunController)
            return;

        if (muzzleFlash)
            gunController.onFire -= PlayMuzzleFlash;

        Detach(gunController);
    }

    public void PlayMuzzleFlash(GunStats stats)
    {
        if (!muzzleFlash)
            return;

        muzzleFlash.SendEvent(VisualEffectAsset.PlayEventID);
    }
}
