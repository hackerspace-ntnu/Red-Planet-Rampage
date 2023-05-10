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

    public override void OnReload(int ammo)
    {
    }

    public override void OnFire(int remainingAmmo)
    {
        animator.SetTrigger("Fire");
    }
}
