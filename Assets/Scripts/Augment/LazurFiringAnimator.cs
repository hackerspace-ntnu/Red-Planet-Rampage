using UnityEngine;

public class LazurFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public AnimationEvent OnChargeStart;

    public override void OnInitialize(GunStats stats)
    {
        // Determining animation speed by its duration
        // firerate = 1 / fire duration, animation speed = animation duration / fire duration, ergo this
        // Using a lil more than reality for animation duration to be certain we don't mess up
        const float animationDuration = 1.4f;
        animator.speed = stats.Firerate * animationDuration;
    }

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
