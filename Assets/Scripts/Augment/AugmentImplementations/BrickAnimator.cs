using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("PistonPump");
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }

    public override void OnInitialize(GunStats stats)
    {
    }

    public override void OnReload(GunStats stats)
    {
    }
}
