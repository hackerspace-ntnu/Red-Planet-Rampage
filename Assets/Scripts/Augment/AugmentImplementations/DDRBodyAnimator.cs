using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDRBodyAnimator : AugmentAnimator
{
    private Animator animator;
    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Vibrate");
    }

    public override void OnInitialize(GunStats stats)
    {
        animator = GetComponent<Animator>();
        if (!animator)
            Debug.Log("Dance Body missing Animator");
    }

    public override void OnReload(GunStats stats)
    {
        animator.SetTrigger("Vibrate");
    }
}
