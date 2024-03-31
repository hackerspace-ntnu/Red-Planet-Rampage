using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherAnimator : AugmentAnimator
{
    private Animator animator;

    public override void OnFire(GunStats stats)
    {
        
    }

    public override void OnInitialize(GunStats stats)
    {
        animator = GetComponent<Animator>();
    }

    public override void OnReload(GunStats stats)
    {
        
    }
}
