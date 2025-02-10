using UnityEngine;

public class LazurFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public AnimationEvent OnChargeStart;

    public bool IsDisabledInReload = false;

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
        if (IsDisabledInReload && stats.Ammo <= 1)
            return;
        animator.SetTrigger("Fire");
    }
}
