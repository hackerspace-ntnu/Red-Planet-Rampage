using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public override void OnFire(GunStats stats)
    {
        // TODO perhaps make it vary based on ammo yeah
        // prolly shouldn't be a transition w a trigger
        // but just go directly to state
        // if (stats.Ammo >= 4)
        // animator.SetTrigger("ResetBricks");
        animator.SetTrigger("PistonPump");
        animator.SetTrigger("PushBrick");
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }

    public override void OnInitialize(GunStats stats)
    {
    }

    public override void OnReload(GunStats stats)
    {
        // animator.SetTrigger("ResetBricks");
        // TODO go to state based on ammo count
    }
}
