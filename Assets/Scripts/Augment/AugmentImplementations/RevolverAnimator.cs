using System.Linq;
using UnityEngine;

public class RevolverAnimator : AugmentAnimator
{
    private Animator animator;
    [SerializeField]
    private Transform attachmentSite;

    public override void OnFire(GunStats stats) { }

    public override void OnInitialize(GunStats stats)
    {
        animator = GetComponent<Animator>();
        var playerHands = GetComponentsInChildren<PlayerHand>(includeInactive: true);
        playerHands.ToList().ForEach(hand => Destroy(hand));
        if (!animator)
            Debug.Log("Revolver Body missing Animator");
    }

    public override void OnReload(GunStats stats)
    {
    }
}
