using UnityEngine;

public class DrillFiringAnimator : AugmentAnimator
{
    [SerializeField]
    private GameObject barrel;

    private int tween;

    public override void OnInitialize(GunStats stats) { }

    public override void OnReload(GunStats stats) { }

    public override void OnFire(GunStats stats)
    {
        if (LeanTween.isTweening(tween))
            LeanTween.removeTween(tween);

        tween = LeanTween.rotateAround(barrel, barrel.transform.up, 90f, .2f).setEaseOutQuart().id;

        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }
}
