using UnityEngine;

public class Revolver : GunBody
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private ParticleSystem steamParticles;
    [SerializeField]
    private PlayerHand playerHand;

    private GunBarrel barrel;
    private GunExtension extension;

    public override void Start()
    {
        gunController = transform.parent.GetComponent<GunController>();
        if (!gunController)
        {
            Debug.Log("Revolver not attached to gun parent!");
            return;
        }
        gunController.onFire += Reload;

        if (gunController.Player)
            playerHand.SetPlayer(gunController.Player);
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
        gunController.onReload.Invoke(stats);
    }

    public void TriggerSteam()
    {
        steamParticles.Play();
    }

    public void ToggleArm()
    {
        if (!playerHand)
            return;
        playerHand.gameObject.SetActive(!playerHand.gameObject.activeInHierarchy);
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
