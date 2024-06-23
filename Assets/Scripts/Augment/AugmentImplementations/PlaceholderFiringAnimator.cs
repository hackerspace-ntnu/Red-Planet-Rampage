using UnityEngine;

public class PlaceholderFiringAnimator : AugmentAnimator
{
    public override void OnInitialize(GunStats stats) { }
    public override void OnReload(GunStats stats) { }

    public override void OnFire(GunStats stats)
    {
        if (DoWeNeedToEscape())
            return;
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }
}
