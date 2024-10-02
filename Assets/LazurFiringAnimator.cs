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
        Debug.Log("SHOOP DA WOOP");
        OnShotFiredAnimation?.Invoke();
    }

    public void EndFiring()
    {
        Debug.Log("SHOOP DONE");
        OnAnimationEnd?.Invoke();
    }

    public override void OnFire(GunStats stats)
    {
        animator.SetTrigger("Fire");
    }
}
