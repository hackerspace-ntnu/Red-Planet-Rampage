using UnityEngine;

public class PlaceholderFiringAnimator : AugmentAnimator
{
    public override void OnInitialize(GunStats stats) { }
    public override void OnReload(GunStats stats) { }

    public override void OnFire(GunStats stats)
    {
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }
}
