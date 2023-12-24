using UnityEngine;

public class CoilFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public override void OnInitialize(GunStats stats)
    {
        animator.speed = Mathf.Max(stats.Firerate / 2.5f, 1f);
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
