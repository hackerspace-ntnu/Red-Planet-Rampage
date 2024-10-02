using UnityEngine;

public class Revolver : GunBody
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private ParticleSystem steamParticles;
    [SerializeField]
    private PlayerHand playerHandLeft;
    [SerializeField]
    private PlayerHand playerHandRight;
    private AudioSource audioSource;
    [SerializeField]
    private AudioGroup bonkWeak;
    [SerializeField]
    private AudioGroup bonkStrong;
    [SerializeField]
    private AudioGroup bulletDrop;
    [SerializeField]
    private AudioGroup noAmmo;

    private GunBarrel barrel;
    private GunExtension extension;

    private bool isReloadInProgress = false;

    public override void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
            return;
        gunController.onFireEnd += Reload;

        if (!gunController.Player)
            return;
        playerHandLeft.SetPlayer(gunController.Player);
        playerHandRight.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
        gunController.onFireNoAmmo += NoAmmoAudio;
    }

    protected override void Reload(GunStats stats)
    {
        if (gunController.stats.Ammo > 0 || isReloadInProgress)
            return;

        barrel = gunController.gameObject.GetComponentInChildren<GunBarrel>();
        if (barrel)
            barrel.transform.SetParent(attachmentSite, true);

        extension = gunController.gameObject.GetComponentInChildren<GunExtension>();
        if (extension)
            extension.transform.SetParent(attachmentSite, true);

        isReloadInProgress = true;
        animator.SetTrigger("Reload");
        gunController.onReload?.Invoke(stats);
    }

    public void TriggerSteam()
    {
        steamParticles.Play();
    }

    private void NoAmmoAudio(GunStats _)
    {
        noAmmo.Play(audioSource);
    }

    public void PlayWeakBonk()
    {
        if (!gunController)
            return;
        bonkWeak.Play(audioSource);
    }
    public void PlayStrongBonk()
    {
        if (!gunController)
            return;
        bonkStrong.Play(audioSource);
    }
    public void PlayBulletDrop()
    {
        if (!gunController)
            return;
        bulletDrop.Play(audioSource);
    }

    public void ToggleArm()
    {
        if (!playerHandLeft)
            return;
        playerHandLeft.gameObject.SetActive(!playerHandLeft.gameObject.activeInHierarchy);
    }

    public void ResetReload()
    {
        if (barrel)
            barrel.transform.SetParent(gunController.transform, true);
        if (extension)
            extension.transform.SetParent(gunController.transform, true);
        gunController.Reload(reloadEfficiencyPercentage);
        isReloadInProgress = false;
    }

    private void OnDestroy()
    {
        if (!gunController)
            return;
        gunController.onFireEnd -= Reload;
        gunController.onFireNoAmmo -= NoAmmoAudio;
    }

}
