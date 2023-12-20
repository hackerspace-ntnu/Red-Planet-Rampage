using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MowerAnimator : AugmentAnimator
{
    [SerializeField]
    private LineRenderer handString;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Animator handAnimator;
    [SerializeField]
    private ParticleSystem exhaustParticles;
    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("PistonPump");
        handAnimator.SetTrigger("Pull");
    }

    public override void OnInitialize(GunStats stats)
    {
        handString.gameObject.SetActive(true);
    }

    public override void OnReload(GunStats stats)
    {
        exhaustParticles.Play();
    }
}
