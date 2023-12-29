using UnityEngine;

public class PinAnimator : AugmentAnimator
{
    [SerializeField]
    private float maxDist;

    private float time;

    [SerializeField]
    private float delay = 0f;

    [SerializeField]
    private AnimationCurve easeCurve;

    public override void OnInitialize(GunStats stats)
    {
        time = 1 / stats.Firerate.Value();
    }

    public override void OnReload(GunStats stats) { }

    public override void OnFire(GunStats stats)
    {
        transform.localPosition = Vector3.zero;
        LeanTween.moveLocalZ(gameObject, maxDist, time * (1 - delay))
            .setDelay(delay * time)
            .setEase(easeCurve);
        OnShotFiredAnimation?.Invoke();
        OnAnimationEnd?.Invoke();
    }
}
