using UnityEngine;

public class LazurFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public AnimationEvent OnChargeStart;

    public override void OnInitialize(GunStats stats)
    {
        return;
    }

    public override void OnReload(GunStats stats)
    {
        return;
    }

    public void PlayChargeUpSound()
    {
        OnChargeStart?.Invoke();
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
