using UnityEngine;

public class CoilFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;
    private bool isDisabledInReload = false;
    public override void OnInitialize(GunStats stats)
    {
        animator.speed = Mathf.Clamp(stats.Firerate * 1.5f, 1f, 6f);
        // TODO: Refactor to generalize check instead of this mess
        if (stats.name.Contains("Revolver"))
            isDisabledInReload = true;
    }

    public override void OnReload(GunStats stats)
    {
    }

    public override void OnFire(GunStats stats)
    {
        if (isDisabledInReload && stats.Ammo <= 1)
            return;
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
