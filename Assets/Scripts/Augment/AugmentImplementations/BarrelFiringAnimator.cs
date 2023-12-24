using UnityEngine;

/// <summary>
/// Class for starting a firing animation.
/// </summary>
public class BarrelFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public override void OnInitialize(GunStats stats)
    {
        animator.speed = Mathf.Max(stats.Firerate, 1f);
    }

    public override void OnReload(GunStats stats)
    {
    }

    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Fire");
        // TODO wait for firing animation to end
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }
}
