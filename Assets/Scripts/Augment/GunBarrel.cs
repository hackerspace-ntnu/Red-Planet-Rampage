using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(ProjectileController))]
public class GunBarrel : Augment
{
    // Where to attach extensions
    public Transform[] attachmentPoints;

    public ProjectileController Projectile { get => GetComponent<ProjectileController>(); }

    [SerializeField]
    private VisualEffect muzzleFlash;

    private GunController gunController;

    private void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;

        if (muzzleFlash)
        {
            muzzleFlash.transform.position = Projectile.projectileOutput.position;
            gunController.onFire += PlayMuzzleFlash;
        }
    }

    private void OnDestroy()
    {
        if (!gunController)
            return;

        if (muzzleFlash)
            gunController.onFire -= PlayMuzzleFlash;
    }

    public void PlayMuzzleFlash(GunStats stats)
    {
        if (!muzzleFlash)
            return;

        muzzleFlash.SendEvent(VisualEffectAsset.PlayEventID);
    }
}
