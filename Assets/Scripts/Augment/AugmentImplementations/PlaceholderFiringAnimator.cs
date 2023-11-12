using UnityEngine;

public class PlaceholderFiringAnimator : AugmentAnimator
{
    public override void OnInitialize(GunStats stats) { }
    public override void OnReload(int ammo) { }

    public override void OnFire(int remainingAmmo)
    {
        OnFireAnimationEnd?.Invoke();
    }
}
