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
    private AudioClip bonkWeak;
    [SerializeField]
    private AudioClip bonkStrong;
    [SerializeField]
    private AudioClip bulletDrop;

    private GunBarrel barrel;
    private GunExtension extension;

    public override void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Revolver not attached to gun parent!");
            return;
        }
        gunController.onFireEnd += Reload;

        if (!gunController.Player)
            return;
        playerHandLeft.SetPlayer(gunController.Player);
        playerHandRight.SetPlayer(gunController.Player);
        playerHandRight.gameObject.SetActive(true);
    }

    protected override void Reload(GunStats stats)
    {
        if (gunController.stats.Ammo >= 1)
            return;

        barrel = gunController.gameObject.GetComponentInChildren<GunBarrel>();
        if (barrel)
            barrel.transform.SetParent(attachmentSite, true);


        extension = gunController.gameObject.GetComponentInChildren<GunExtension>();
        if (extension)
            extension.transform.SetParent(attachmentSite, true);

        animator.SetTrigger("Reload");
        gunController.onReload?.Invoke(stats);
    }

    public void TriggerSteam()
    {
        steamParticles.Play();
    }

    public void PlayWeakBonk()
    {
        if (!gunController)
            return;
        audioSource.clip = bonkWeak;
        audioSource.Play();
    }
    public void PlayStrongBonk()
    {
        if (!gunController)
            return;
        audioSource.clip = bonkStrong;
        audioSource.Play();
    }
    public void PlayBulletDrop()
    {
        if (!gunController)
            return;
        audioSource.clip = bulletDrop;
        audioSource.Play();
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
    }

}
