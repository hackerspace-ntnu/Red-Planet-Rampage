using UnityEngine;

public class CoilFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;

    public override void OnInitialize(GunStats stats)
    {
        animator.speed = Mathf.Max(stats.Firerate / 2.5f, 1f);
    }

    public override void OnReload(int ammo)
    {
    }

    public override void OnFire(int remainingAmmo)
    {
        animator.SetTrigger("Fire");
    }

    public void ShotFired()
    {
        OnFireAnimationEnd?.Invoke();
    }
}
