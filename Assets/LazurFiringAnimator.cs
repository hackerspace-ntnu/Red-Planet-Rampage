using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazurFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;
    public override void OnInitialize(GunStats stats)
    {
        return;
    }
    public override void OnReload(GunStats stats)
    {
        return;
    }
    public void ShootLazer()
    {
        this.OnShotFiredAnimation?.Invoke();
        this.OnAnimationEnd?.Invoke();
    }
    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Fire");
    }
}
