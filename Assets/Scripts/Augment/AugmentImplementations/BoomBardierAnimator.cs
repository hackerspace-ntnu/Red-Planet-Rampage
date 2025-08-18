using UnityEngine;

public class BoomBardierAnimator : AugmentAnimator
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Renderer[] dynamites;
    private int ammo;

    public override void OnInitialize(GunStats stats)
    {
        animator.speed = Mathf.Max(stats.Firerate, 1f);
        ammo = stats.Ammo;
        VisualizeAmmoCount();
    }

    public override void OnReload(GunStats stats)
    {
        ammo = stats.Ammo;
        VisualizeAmmoCount();
    }

    public override void OnFire(GunStats stats)
    {
        try
        {
            animator.SetTrigger("Fire");
            ammo = stats.Ammo;
            OnShotFiredAnimation?.Invoke();
            OnAnimationEnd?.Invoke();
        }
        catch (System.NullReferenceException)
        {
            // Avoids specific issue that only appears in build.
            Debug.LogWarning($"Ignoring nullref in {nameof(BoomBardierAnimator)}:OnFire");
            return;
        }
    }

    // Also called by animator
    public void VisualizeAmmoCount()
    {
        for (int i = 0; i < dynamites.Length; i++)
            // TODO nullref here
            dynamites[i].enabled = ammo - 1 > i;
    }
}
