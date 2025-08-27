using UnityEngine;

public class CoilFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public override void OnInitialize(GunStats stats)
    {
        // Determining animation speed by its duration
        // firerate = 1 / fire duration, animation speed = animation duration / fire duration, ergo this
        // Using a lil more than reality for animation duration to be certain we don't mess up
        // Note that firerate accounts for the entire animation here, not each individual shot
        const float animationDuration = 2.1f;
        animator.speed = stats.Firerate * animationDuration;
    }

    public override void OnReload(GunStats stats)
    {
    }

    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Fire");
    }

    public void ShotFired(int number)
    {
        OnShotFiredAnimation?.Invoke();
    }

    public void AnimationEnd()
    {
        OnAnimationEnd?.Invoke();
    }
}
