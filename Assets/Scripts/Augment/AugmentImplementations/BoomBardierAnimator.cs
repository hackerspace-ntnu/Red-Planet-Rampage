using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomBardierAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Renderer[] dynamites;
    private int ammo;

    public override void OnInitialize(GunStats stats)
    {
        animator.speed = Mathf.Max(stats.Firerate, 1f);
        ammo = stats.Ammo;
        VisualizeAmmoCount();
    }

    public override void OnReload(GunStats stats)
    {
        ammo = stats.Ammo;
        VisualizeAmmoCount();
    }

    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Fire");
        ammo = stats.Ammo;
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }
    
    // Also called by animator
    public void VisualizeAmmoCount()
    {
        for (int i = 0; i < dynamites.Length; i++)
            dynamites[i].enabled = ammo - 1 > i;
    }
}
