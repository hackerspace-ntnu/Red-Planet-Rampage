using UnityEngine;

public class LazurFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public AnimationEvent OnChargeStart;

    public override void OnInitialize(GunStats stats) { }

    public override void OnReload(GunStats stats) { }

    public void PlayChargeUpSound()
    {
        OnChargeStart?.Invoke();
    }

    public void ShootLazer()
    {
        OnShotFiredAnimation?.Invoke();
    }

    public void EndFiring()
    {
        OnAnimationEnd?.Invoke();
    }

    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Fire");
    }
}
