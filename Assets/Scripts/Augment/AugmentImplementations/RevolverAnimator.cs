using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolverAnimator : AugmentAnimator
{
    private Animator animator;
    private bool canReload = false;
    [SerializeField]
    private Transform attachmentSite;
    private GunBarrel barrel;
    private GunExtension extension;
    public override void OnFire(GunStats stats)
    {

    }

    public override void OnInitialize(GunStats stats)
    {
        animator = GetComponent<Animator>();
        var playerHand = GetComponentInChildren<PlayerHand>(includeInactive: true);
        playerHand.transform.SetParent(null);
        Destroy(playerHand);
        if (!animator)
            Debug.Log("Revolver Body missing Animator");
    }

    public override void OnReload(GunStats stats)
    {
        canReload = !canReload;
        if (!canReload)
            return;

        barrel = transform.parent.gameObject.GetComponentInChildren<GunBarrel>();
        if (barrel)
            barrel.transform.SetParent(attachmentSite, true);

        extension = transform.parent.gameObject.GetComponentInChildren<GunExtension>();
        if (extension)
            extension.transform.SetParent(attachmentSite, true);

        animator.SetTrigger("Reload");
    }
}
